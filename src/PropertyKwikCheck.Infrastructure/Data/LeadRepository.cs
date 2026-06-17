using Dapper;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Domain;
using PropertyKwikCheck.Core.Rbac;

namespace PropertyKwikCheck.Infrastructure.Data;

public sealed class LeadRepository(IDbConnectionFactory factory) : ILeadRepository
{
    private const string Columns = """
        id, req_id, asset_family, property_type, stage, report_status,
        applicant, co_applicant, contact, pin, location,
        lender_company_id, lender_name, branch,
        valuator_user_id, valuator_name, ro_company,
        exec_name, exec_phone, exec_email,
        loan_no, claim_no, source, reg_no,
        lead_date, assigned_on, inspection_date, issued_date, tat_due, tat_pct, tat_state,
        value, remarks, hold_remarks, report_data,
        created_by, created_at, updated_at, deleted_at
        """;

    public async Task<Lead?> GetByIdAsync(long id)
    {
        using var conn = await factory.OpenAsync();
        return await conn.QuerySingleOrDefaultAsync<Lead>(
            $"SELECT {Columns} FROM leads WHERE id = @id AND deleted_at IS NULL", new { id });
    }

    public async Task<(List<Lead> Rows, int Total)> ListAsync(LeadQuery query)
    {
        using var conn = await factory.OpenAsync();
        var p = new DynamicParameters();
        var where = BuildWhere(query, p);

        var total = await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM leads {where}", p);

        var orderBy = ResolveSort(query.Sort);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, LeadQuery.MaxPageSize);
        p.Add("offset", (page - 1) * pageSize);
        p.Add("pageSize", pageSize);

        var rows = (await conn.QueryAsync<Lead>(
            $"SELECT {Columns} FROM leads {where} ORDER BY {orderBy} OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY", p))
            .ToList();

        return (rows, total);
    }

    public async Task<Dictionary<string, int>> CountsByStageAsync(LeadScope scope)
    {
        using var conn = await factory.OpenAsync();
        var p = new DynamicParameters();
        var where = BuildScopeWhere(scope, p);
        var rows = await conn.QueryAsync<(string stage, int n)>(
            $"SELECT stage, COUNT(*) AS n FROM leads {where} GROUP BY stage", p);
        return rows.ToDictionary(r => r.stage, r => r.n);
    }

    public async Task<List<Lead>> RecentAsync(int count, LeadScope scope)
    {
        using var conn = await factory.OpenAsync();
        var p = new DynamicParameters();
        var where = BuildScopeWhere(scope, p);
        p.Add("count", count);
        return (await conn.QueryAsync<Lead>(
            $"SELECT TOP (@count) {Columns} FROM leads {where} ORDER BY created_at DESC", p)).ToList();
    }

    public async Task<long> InsertAsync(Lead lead)
    {
        using var conn = await factory.OpenAsync();
        if (string.IsNullOrEmpty(lead.ReqId)) lead.ReqId = $"TMP-{Guid.NewGuid():N}";
        const string sql = """
            INSERT INTO leads
              (req_id, asset_family, property_type, stage, report_status,
               applicant, co_applicant, contact, pin, location,
               lender_company_id, lender_name, branch,
               valuator_user_id, valuator_name, ro_company,
               exec_name, exec_phone, exec_email,
               loan_no, claim_no, source, reg_no,
               lead_date, assigned_on, inspection_date, issued_date, tat_due, tat_pct, tat_state,
               value, remarks, hold_remarks, report_data, created_by)
            OUTPUT INSERTED.id
            VALUES
              (@ReqId, @AssetFamily, @PropertyType, @Stage, @ReportStatus,
               @Applicant, @CoApplicant, @Contact, @Pin, @Location,
               @LenderCompanyId, @LenderName, @Branch,
               @ValuatorUserId, @ValuatorName, @RoCompany,
               @ExecName, @ExecPhone, @ExecEmail,
               @LoanNo, @ClaimNo, @Source, @RegNo,
               @LeadDate, @AssignedOn, @InspectionDate, @IssuedDate, @TatDue, @TatPct, @TatState,
               @Value, @Remarks, @HoldRemarks, @ReportData, @CreatedBy);
            """;
        return await conn.ExecuteScalarAsync<long>(sql, lead);
    }

    public async Task UpdateAsync(Lead lead)
    {
        using var conn = await factory.OpenAsync();
        const string sql = """
            UPDATE leads SET
              req_id=@ReqId, asset_family=@AssetFamily, property_type=@PropertyType, stage=@Stage, report_status=@ReportStatus,
              applicant=@Applicant, co_applicant=@CoApplicant, contact=@Contact, pin=@Pin, location=@Location,
              lender_company_id=@LenderCompanyId, lender_name=@LenderName, branch=@Branch,
              valuator_user_id=@ValuatorUserId, valuator_name=@ValuatorName, ro_company=@RoCompany,
              exec_name=@ExecName, exec_phone=@ExecPhone, exec_email=@ExecEmail,
              loan_no=@LoanNo, claim_no=@ClaimNo, source=@Source, reg_no=@RegNo,
              lead_date=@LeadDate, assigned_on=@AssignedOn, inspection_date=@InspectionDate, issued_date=@IssuedDate,
              tat_due=@TatDue, tat_pct=@TatPct, tat_state=@TatState,
              value=@Value, remarks=@Remarks, hold_remarks=@HoldRemarks, report_data=@ReportData,
              updated_at=SYSUTCDATETIME()
            WHERE id=@Id;
            """;
        await conn.ExecuteAsync(sql, lead);
    }

    public async Task SoftDeleteAsync(long id)
    {
        using var conn = await factory.OpenAsync();
        await conn.ExecuteAsync("UPDATE leads SET deleted_at=SYSUTCDATETIME() WHERE id=@id", new { id });
    }

    public async Task AddStageHistoryAsync(LeadStageHistory h)
    {
        using var conn = await factory.OpenAsync();
        await conn.ExecuteAsync("""
            INSERT INTO lead_stage_history (lead_id, from_stage, to_stage, actor_user_id, note)
            VALUES (@LeadId, @FromStage, @ToStage, @ActorUserId, @Note)
            """, h);
    }

    // ---- query building ------------------------------------------------------

    private static string BuildWhere(LeadQuery query, DynamicParameters p)
    {
        var clauses = new List<string> { "deleted_at IS NULL" };

        if (!string.IsNullOrWhiteSpace(query.Bucket))
        {
            clauses.Add("stage = @bucket");
            p.Add("bucket", query.Bucket);
        }

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            clauses.Add("""
                (applicant LIKE @q OR req_id LIKE @q OR lender_name LIKE @q OR valuator_name LIKE @q
                 OR exec_name LIKE @q OR location LIKE @q OR property_type LIKE @q OR loan_no LIKE @q
                 OR source LIKE @q OR ro_company LIKE @q)
                """);
            p.Add("q", $"%{query.Q}%");
        }

        AppendScopeClauses(query.Scope, clauses, p);
        return "WHERE " + string.Join(" AND ", clauses);
    }

    private static string BuildScopeWhere(LeadScope scope, DynamicParameters p)
    {
        var clauses = new List<string> { "deleted_at IS NULL" };
        AppendScopeClauses(scope, clauses, p);
        return "WHERE " + string.Join(" AND ", clauses);
    }

    private static void AppendScopeClauses(LeadScope scope, List<string> clauses, DynamicParameters p)
    {
        switch (scope.Mode)
        {
            case Scope.OwnLeads:
                clauses.Add("valuator_user_id = @scopeValuator");
                p.Add("scopeValuator", scope.ValuatorUserId);
                break;
            case Scope.OwnCompany:
                clauses.Add("lender_company_id = @scopeCompany");
                p.Add("scopeCompany", scope.CompanyId);
                break;
        }
    }

    private static string ResolveSort(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return "created_at DESC";
        var parts = sort.Split(':', 2);
        var col = parts[0] switch
        {
            "assignedOn" => "assigned_on",
            "createdAt" => "created_at",
            "value" => "value",
            "tatPct" => "tat_pct",
            "leadDate" => "lead_date",
            _ => "created_at",
        };
        var dir = parts.Length > 1 && parts[1].Equals("asc", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";
        return $"{col} {dir}";
    }
}
