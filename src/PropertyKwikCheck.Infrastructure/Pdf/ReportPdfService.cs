using System.Text.Json.Nodes;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Common;
using PropertyKwikCheck.Core.Domain;
using PropertyKwikCheck.Core.Dtos;
using PropertyKwikCheck.Core.Mapping;
using PropertyKwikCheck.Core.Rbac;
using PropertyKwikCheck.Core.Security;
using PropertyKwikCheck.Core.Workflow;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PropertyKwikCheck.Infrastructure.Pdf;

/// <summary>
/// Renders the lead's valuation report to PDF from its report_data (spec §8.12, §11),
/// using QuestPDF (pure .NET, no headless browser). Issued (completed) reports are
/// rendered once, cached on disk, and re-served thereafter (immutability).
/// </summary>
public sealed class ReportPdfService(
    ILeadRepository leads,
    IPhotoRepository photos,
    IFileStorage storage) : IReportPdfService
{
    private static readonly string[] AllowedStages =
        [Stage.Qc, Stage.QcHold, Stage.Pricing, Stage.Completed];

    static ReportPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<FileDownload> GenerateAsync(long leadId, CurrentUser user)
    {
        user.Require(Capability.ViewLeads);
        var lead = await leads.GetByIdAsync(leadId) ?? throw AppException.NotFound();
        LeadAccess.EnsureVisible(lead, user);

        if (Array.IndexOf(AllowedStages, lead.Stage) < 0)
            throw AppException.Conflict("Report is not available before QC stage", "REPORT_NOT_READY");

        var fileName = $"{lead.ReqId}.pdf";
        var cacheKey = $"leads/{leadId}/report.pdf";

        // Issued reports are immutable: serve the stored copy if present.
        if (lead.Stage == Stage.Completed)
        {
            var existing = await storage.OpenAsync(cacheKey);
            if (existing is not null) return new FileDownload(existing, "application/pdf", fileName);
        }

        var images = await LoadPhotoImagesAsync(leadId);
        var bytes = Build(lead, images);

        if (lead.Stage == Stage.Completed)
        {
            await storage.SaveAsync(cacheKey, new MemoryStream(bytes));
        }

        return new FileDownload(new MemoryStream(bytes), "application/pdf", fileName);
    }

    private async Task<List<byte[]>> LoadPhotoImagesAsync(long leadId)
    {
        var result = new List<byte[]>();
        var all = await photos.ListByLeadAsync(leadId);
        foreach (var p in all.Where(x => x.Kind == "photo").Take(8))
        {
            var stream = await storage.OpenAsync(p.StorageKey);
            if (stream is null) continue;
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            stream.Dispose();
            result.Add(ms.ToArray());
        }
        return result;
    }

    private static byte[] Build(Lead lead, List<byte[]> images)
    {
        var data = LeadMapper.ParseData(lead.ReportData) ?? new JsonObject();

        return QuestPDF.Fluent.Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Margin(36);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(t => t.FontSize(9).FontColor("#16263a"));

                page.Header().Element(c => Header(c, lead, data));
                page.Content().Element(c => Content(c, lead, data, images));
                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("KwikCheck Valuation Report · ").FontSize(7).FontColor("#94a3b6");
                    t.Span($"Generated {DateTime.UtcNow:dd MMM yyyy} · ").FontSize(7).FontColor("#94a3b6");
                    t.Span("Page ").FontSize(7).FontColor("#94a3b6");
                    t.CurrentPageNumber().FontSize(7).FontColor("#94a3b6");
                    t.Span(" of ").FontSize(7).FontColor("#94a3b6");
                    t.TotalPages().FontSize(7).FontColor("#94a3b6");
                });
            });
        }).GeneratePdf();
    }

    private static void Header(IContainer c, Lead lead, JsonObject data)
    {
        c.PaddingBottom(8).BorderBottom(1.5f).BorderColor("#1f5fae").Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    left.Item().Text("KWIK CHECK").FontSize(16).Bold().FontColor("#0c2742");
                    left.Item().Text(Str(data, "reportType", "Valuation Report")).FontSize(10).FontColor("#1f5fae");
                });
                row.ConstantItem(180).AlignRight().Column(right =>
                {
                    right.Item().Text(lead.ReqId).FontSize(11).Bold();
                    right.Item().Text($"Loan/Prospect: {lead.LoanNo ?? "—"}").FontSize(8).FontColor("#516074");
                    right.Item().Text($"Status: {lead.ReportStatus}").FontSize(8).FontColor("#516074");
                });
            });
        });
    }

    private static void Content(IContainer c, Lead lead, JsonObject data, List<byte[]> images)
    {
        c.PaddingTop(10).Column(col =>
        {
            col.Spacing(12);

            Section(col, "Case & Lender", new (string, string?)[]
            {
                ("Applicant", lead.Applicant), ("Co-Applicant", Str(data, "coApplicant", null)),
                ("Lender / Bank", lead.LenderName), ("Branch", lead.Branch),
                ("Valuer (RO)", lead.ValuatorName), ("RO Company", lead.RoCompany),
                ("Bank Executive", lead.ExecName), ("Source", lead.Source),
                ("Address (As per Document)", Str(data, "addrDoc", null)),
                ("Address (At Site)", Str(data, "addrActual", null)),
            });

            Section(col, AssetSectionTitle(lead.AssetFamily), AssetFields(lead.AssetFamily, data));

            Section(col, "Valuation & Sign-off", new (string, string?)[]
            {
                ("Fair Market Value", MoneyStr(data, "fairMarketValue")),
                ("Adopted Value", MoneyStr(data, "adoptedValue")),
                ("Realizable Value", MoneyStr(data, "realizableValue")),
                ("Distress Value", MoneyStr(data, "distressValue")),
                ("Overall Risk", Str(data, "overallRisk", null)),
                ("Adopted Rate", Str(data, "adoptedRate", null)),
            });

            Section(col, "Certification", new (string, string?)[]
            {
                ("Inspected By", Str(data, "inspectedBy", null)),
                ("Inspector Licence", Str(data, "inspectedLicence", null)),
                ("Inspected On", Str(data, "inspectedDate", null)),
                ("Reviewed By", Str(data, "reviewedBy", null)),
                ("Authorised By", Str(data, "authorisedBy", null)),
                ("Authorised On", Str(data, "authorisedDate", null)),
            });

            if (images.Count > 0)
            {
                col.Item().Text("Site Photographs").FontSize(11).Bold().FontColor("#0c2742");
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(d => { d.RelativeColumn(); d.RelativeColumn(); d.RelativeColumn(); });
                    foreach (var img in images)
                        table.Cell().Padding(3).Height(110).Image(img).FitArea();
                });
            }
        });
    }

    private static void Section(ColumnDescriptor col, string title, (string Label, string? Value)[] fields)
    {
        col.Item().Column(s =>
        {
            s.Item().PaddingBottom(4).Text(title).FontSize(11).Bold().FontColor("#0c2742");
            s.Item().Border(0.5f).BorderColor("#dde5ee").Table(table =>
            {
                table.ColumnsDefinition(d => { d.RelativeColumn(); d.RelativeColumn(2); });
                foreach (var (label, value) in fields)
                {
                    table.Cell().Background("#f6f9fc").Padding(5).Text(label).FontColor("#516074").FontSize(8.5f);
                    table.Cell().Padding(5).Text(string.IsNullOrWhiteSpace(value) ? "—" : value).FontSize(9);
                }
            });
        });
    }

    private static string AssetSectionTitle(string family) => family switch
    {
        AssetFamily.Plot => "Plot Identification & Site",
        AssetFamily.Agri => "Agri Land Identification & Site",
        _ => "Property Details & Site",
    };

    private static (string, string?)[] AssetFields(string family, JsonObject d) => family switch
    {
        AssetFamily.Plot =>
        [
            ("Plot Number", Str(d, "plotNumber", null)), ("Survey / Khasra", Str(d, "surveyKhasra", null)),
            ("Village / Colony", Str(d, "villageColony", null)), ("Tehsil", Str(d, "tehsil", null)),
            ("District / State", Str(d, "districtState", null)), ("Owner", Str(d, "ownerName", null)),
            ("Ownership Type", Str(d, "ownershipType", null)), ("Reg. Number", Str(d, "regNumber", null)),
        ],
        AssetFamily.Agri =>
        [
            ("Khasra Number", Str(d, "khasraNumber", null)), ("Khata Number", Str(d, "khataNumber", null)),
            ("Village", Str(d, "village", null)), ("Tehsil", Str(d, "tehsil", null)),
            ("District / State", Str(d, "districtState", null)), ("Jamabandi Year", Str(d, "jamabandiYear", null)),
            ("Khatedar", Str(d, "khatedarName", null)), ("Tenure Type", Str(d, "tenureType", null)),
        ],
        _ =>
        [
            ("Configuration", Str(d, "config", null)), ("Carpet Area (sqft)", Str(d, "carpet", null)),
            ("Built-up Area", Str(d, "builtup", null)), ("Super Built-up", Str(d, "superBuiltup", null)),
            ("Year Built", Str(d, "yearBuilt", null)), ("Facing", Str(d, "facing", null)),
            ("Floor / Total", $"{Str(d, "floorNo", "—")} / {Str(d, "totalFloors", "—")}"),
            ("Ownership", Str(d, "ownership", null)),
        ],
    };

    private static string? Str(JsonObject o, string key, string? fallback)
    {
        if (o.TryGetPropertyValue(key, out var v) && v is JsonValue jv && jv.TryGetValue<string>(out var s) && !string.IsNullOrWhiteSpace(s))
            return s;
        return fallback;
    }

    private static string? MoneyStr(JsonObject o, string key)
    {
        var s = Str(o, key, null);
        return s is not null && long.TryParse(s, out var n) ? Inr.Format(n) : s;
    }
}
