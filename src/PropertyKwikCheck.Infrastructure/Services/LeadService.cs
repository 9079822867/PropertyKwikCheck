using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Common;
using PropertyKwikCheck.Core.Domain;
using PropertyKwikCheck.Core.Dtos;
using PropertyKwikCheck.Core.Mapping;
using PropertyKwikCheck.Core.Rbac;
using PropertyKwikCheck.Core.Security;
using PropertyKwikCheck.Core.Workflow;

namespace PropertyKwikCheck.Infrastructure.Services;

/// <summary>
/// Lead workflow: list/get (scoped), create, edit-save / reassign / reject (PATCH),
/// soft-delete. Enforces the stage machine (spec §4), RBAC (spec §9), and writes
/// stage history + audit on every mutation (spec §13).
/// </summary>
public sealed class LeadService(
    ILeadRepository leads,
    IAuditRepository audit,
    IClock clock) : ILeadService
{
    public async Task<LeadListResponse> ListAsync(LeadQuery query)
    {
        var (rows, total) = await leads.ListAsync(query);
        var counts = await EnsureAllBuckets(leads.CountsByStageAsync(query.Scope));
        return new LeadListResponse
        {
            Rows = rows.Select(ToScopedDto).ToList(),
            Total = total,
            Counts = counts,
        };
    }

    public async Task<LeadDto> GetAsync(long id, CurrentUser user)
    {
        var lead = await leads.GetByIdAsync(id) ?? throw AppException.NotFound();
        EnsureVisible(lead, user);
        return ToScopedDto(lead);
    }

    public async Task<Dictionary<string, int>> BucketCountsAsync(CurrentUser user) =>
        await EnsureAllBuckets(leads.CountsByStageAsync(LeadScope.From(user)));

    public async Task<LeadDto> CreateAsync(CreateLeadRequest request, CurrentUser user, AuditContext auditCtx)
    {
        user.Require(Capability.CreateLead);

        var meta = PropertyTypes.Resolve(request.Ptype)
            ?? throw AppException.Validation($"Unknown ptype '{request.Ptype}'",
                new Dictionary<string, string> { ["ptype"] = "Unsupported property type" });

        var data = request.Data is not null ? JsonMerge.Merge(null, request.Data) : new JsonObject();

        var lead = new Lead
        {
            AssetFamily = meta.Family,
            PropertyType = request.Ptype,
            Stage = Stage.Fresh,
            ReportStatus = "Open",
            TatPct = 8,
            TatState = TatState.Ok,
            Value = null,
            CreatedBy = user.Id,
        };

        // Copy convenience columns out of the intake data (spec §8.3).
        lead.Applicant = Str(data, "applicant");
        lead.CoApplicant = Str(data, "coApplicant");
        lead.Contact = Str(data, "contact");
        lead.LenderName = Str(data, "lender");
        lead.Branch = Str(data, "branch");
        lead.ValuatorName = Str(data, "assignedRO");
        lead.RoCompany = Str(data, "roCompany");
        lead.ExecName = Str(data, "execName");
        lead.ExecPhone = Str(data, "execPhone");
        lead.ExecEmail = Str(data, "execEmail");
        lead.LoanNo = Str(data, "loanNo");
        lead.ClaimNo = Str(data, "claimNo");
        lead.Source = Str(data, "source");
        lead.Remarks = Str(data, "remarks");
        lead.LeadDate = Date(data, "leadDate");

        var id = await leads.InsertAsync(lead);
        lead.Id = id;
        lead.ReqId = ReqIdGenerator.Generate(request.Ptype, id);

        // Backfill canonical data keys (spec §8.3).
        data["reportType"] ??= meta.ReportLabel;
        data["reportStatus"] = "Open";
        data["leadId"] = lead.ReqId;
        data["reqId"] = lead.ReqId;
        data["propertyType"] ??= request.Ptype;
        lead.ReportData = data.ToJsonString();

        await leads.UpdateAsync(lead);
        await leads.AddStageHistoryAsync(new LeadStageHistory
        {
            LeadId = id,
            FromStage = null,
            ToStage = Stage.Fresh,
            ActorUserId = user.Id,
            Note = "Lead created",
        });
        await WriteAudit(auditCtx, "lead.create", id, null, lead);

        return LeadMapper.ToDto(lead);
    }

    public async Task<LeadDto> UpdateAsync(long id, UpdateLeadRequest request, CurrentUser user, AuditContext auditCtx)
    {
        var lead = await leads.GetByIdAsync(id) ?? throw AppException.NotFound();
        EnsureVisible(lead, user);
        var before = Snapshot(lead);

        switch (request.Action)
        {
            case "reassign":
                await ReassignAsync(lead, request, user, auditCtx, before);
                break;
            case "reject":
                await RejectAsync(lead, user, auditCtx, before);
                break;
            case null or "":
                await EditSaveAsync(lead, request, user, auditCtx, before);
                break;
            default:
                throw AppException.Validation($"Unknown action '{request.Action}'");
        }

        return LeadMapper.ToDto(lead);
    }

    public async Task DeleteAsync(long id, CurrentUser user, AuditContext auditCtx)
    {
        user.Require(Capability.DeleteLead);
        var lead = await leads.GetByIdAsync(id) ?? throw AppException.NotFound();
        EnsureVisible(lead, user);
        await leads.SoftDeleteAsync(id);
        await WriteAudit(auditCtx, "lead.delete", id, lead, null);
    }

    // ---- transitions ---------------------------------------------------------

    private async Task ReassignAsync(Lead lead, UpdateLeadRequest request, CurrentUser user,
        AuditContext auditCtx, JsonObject before)
    {
        user.Require(Capability.AssignReassign);
        if (string.IsNullOrWhiteSpace(request.Valuator))
            throw AppException.Validation("valuator is required for reassign");

        lead.ValuatorName = request.Valuator;
        var from = lead.Stage;
        if (lead.Stage == Stage.Assigned)
            await MoveStage(lead, Stage.Reassigned, user, "Reassigned");

        await SaveAndAudit(lead, auditCtx, "lead.reassign", before, from);
    }

    private async Task RejectAsync(Lead lead, CurrentUser user, AuditContext auditCtx, JsonObject before)
    {
        user.Require(Capability.RejectDuplicate);
        var from = lead.Stage;
        await MoveStage(lead, Stage.Rejected, user, "Rejected");
        await SaveAndAudit(lead, auditCtx, "lead.reject", before, from);
    }

    private async Task EditSaveAsync(Lead lead, UpdateLeadRequest request, CurrentUser user,
        AuditContext auditCtx, JsonObject before)
    {
        user.Require(Capability.EditReport);

        if (lead.Stage == Stage.Completed && request.Data is not null)
            throw AppException.Conflict("Report is issued and locked", "REPORT_LOCKED");

        var from = lead.Stage;

        // Deep-merge (top-level) the report data patch.
        if (request.Data is not null)
        {
            var merged = JsonMerge.Merge(LeadMapper.ParseData(lead.ReportData), request.Data);
            lead.ReportData = merged.ToJsonString();
            SyncValueFromData(lead, merged, request.Value);
        }
        else if (request.Value is not null)
        {
            lead.Value = request.Value;
        }

        // Shallow-assign top-level columns.
        if (request.Remarks is not null) lead.Remarks = request.Remarks;
        if (request.HoldRemarks is not null) lead.HoldRemarks = request.HoldRemarks;

        // Stage change (validated through the machine; capability gated by target).
        if (request.Stage is not null && request.Stage != lead.Stage)
        {
            RequireTransitionCapability(user, from, request.Stage);
            await MoveStage(lead, request.Stage, user, "Stage updated");
        }
        else if (request.ReportStatus is not null)
        {
            lead.ReportStatus = request.ReportStatus;
        }

        await SaveAndAudit(lead, auditCtx, "lead.update", before, from);
    }

    /// <summary>Applies a stage move with machine validation, status sync, history + issue side-effects.</summary>
    private async Task MoveStage(Lead lead, string to, CurrentUser user, string note)
    {
        if (!StageMachine.CanTransition(lead.Stage, to))
            throw AppException.InvalidTransition(lead.Stage, to);

        var from = lead.Stage;
        lead.Stage = to;
        lead.ReportStatus = ReportStatusForStage(to) ?? lead.ReportStatus;

        if (to == Stage.Completed && lead.IssuedDate is null)
            lead.IssuedDate = clock.UtcNow.Date;
        // holdRemarks only applies while on hold; clear it once the lead moves off qc_hold.
        if (from == Stage.QcHold && to != Stage.QcHold)
            lead.HoldRemarks = null;

        await leads.AddStageHistoryAsync(new LeadStageHistory
        {
            LeadId = lead.Id,
            FromStage = from,
            ToStage = to,
            ActorUserId = user.Id,
            Note = note,
        });
    }

    private async Task SaveAndAudit(Lead lead, AuditContext auditCtx, string action, JsonObject before, string fromStage)
    {
        await leads.UpdateAsync(lead);
        await WriteAudit(auditCtx, action, lead.Id, before, lead, fromStage != lead.Stage ? $"{fromStage}→{lead.Stage}" : null);
    }

    // ---- helpers -------------------------------------------------------------

    private static void RequireTransitionCapability(CurrentUser user, string from, string to)
    {
        var cap = to switch
        {
            Stage.Qc when from == Stage.RoConfirmation => Capability.SubmitForQc,
            Stage.QcHold => Capability.QcApproveHold,
            Stage.Qc when from == Stage.QcHold => Capability.QcApproveHold,
            Stage.Pricing => Capability.QcApproveHold,
            Stage.Completed => Capability.AuthoriseIssue,
            Stage.Rejected => Capability.RejectDuplicate,
            Stage.Duplicate => Capability.RejectDuplicate,
            Stage.Assigned or Stage.Reassigned => Capability.AssignReassign,
            _ => Capability.EditReport,
        };
        user.Require(cap);
    }

    private static string? ReportStatusForStage(string stage) => stage switch
    {
        Stage.Fresh or Stage.Ro => "Open",
        Stage.Assigned or Stage.Reassigned => "Assigned to Valuer",
        Stage.RoConfirmation => "Awaiting RO Confirmation",
        Stage.Qc => "In QC Review",
        Stage.QcHold => "On Hold",
        Stage.Pricing => "Pricing",
        Stage.Completed => "Verified & Issued",
        Stage.Rejected or Stage.Duplicate => "Rejected",
        _ => null,
    };

    private static void SyncValueFromData(Lead lead, JsonObject data, long? explicitValue)
    {
        if (explicitValue is not null) { lead.Value = explicitValue; return; }
        var fmv = Long(data, "fairMarketValue") ?? Long(data, "adoptedValue");
        if (fmv is not null) lead.Value = fmv;
    }

    private LeadDto ToScopedDto(Lead lead)
    {
        ApplyLiveTat(lead);
        return LeadMapper.ToDto(lead);
    }

    /// <summary>Recompute TAT at read time for non-terminal leads (spec §12).</summary>
    private void ApplyLiveTat(Lead lead)
    {
        if (Stage.IsTerminal(lead.Stage) || lead.AssignedOn is null || lead.TatDue is null) return;
        var pct = TatCalculator.PercentElapsed(lead.AssignedOn, lead.TatDue, clock.UtcNow);
        lead.TatPct = (int)Math.Min(pct, 999);
        lead.TatState = TatCalculator.StateFor(pct);
    }

    private void EnsureVisible(Lead lead, CurrentUser user)
    {
        var visible = user.Scope switch
        {
            Scope.OwnLeads => lead.ValuatorUserId == user.Id,
            Scope.OwnCompany => lead.LenderCompanyId == user.CompanyId,
            _ => true,
        };
        // Scope failure on a single resource → 404 (don't leak existence, spec §9.2).
        if (!visible) throw AppException.NotFound();
    }

    private static async Task<Dictionary<string, int>> EnsureAllBuckets(Task<Dictionary<string, int>> task)
    {
        var counts = await task;
        foreach (var s in Stage.All)
            counts.TryAdd(s, 0);
        return counts;
    }

    private async Task WriteAudit(AuditContext ctx, string action, long id, object? before, object? after, string? note = null)
    {
        await audit.AddAsync(new AuditEntry
        {
            ActorUserId = ctx.Actor?.Id,
            Action = action,
            EntityType = "lead",
            EntityId = id.ToString(CultureInfo.InvariantCulture),
            BeforeJson = before is null ? null : JsonSerializer.Serialize(before),
            AfterJson = after is null ? null : JsonSerializer.Serialize(after),
            Ip = ctx.Ip,
            UserAgent = ctx.UserAgent,
        });
    }

    private static JsonObject Snapshot(Lead lead) => new()
    {
        ["stage"] = lead.Stage,
        ["reportStatus"] = lead.ReportStatus,
        ["valuator"] = lead.ValuatorName,
        ["value"] = lead.Value,
    };

    private static string? Str(JsonObject o, string key) =>
        o.TryGetPropertyValue(key, out var v) && v is not null ? v.GetValue<string?>() : null;

    private static long? Long(JsonObject o, string key)
    {
        if (!o.TryGetPropertyValue(key, out var v) || v is null) return null;
        if (v is JsonValue jv)
        {
            if (jv.TryGetValue<long>(out var l)) return l;
            if (jv.TryGetValue<string>(out var s) && long.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var ls)) return ls;
            if (jv.TryGetValue<double>(out var d)) return (long)d;
        }
        return null;
    }

    private static DateTime? Date(JsonObject o, string key)
    {
        var s = Str(o, key);
        return DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
    }
}
