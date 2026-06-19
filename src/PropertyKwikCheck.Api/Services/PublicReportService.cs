using System.Globalization;
using System.Reflection;
using System.Text.Json.Nodes;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Common;
using PropertyKwikCheck.Core.Domain;

namespace PropertyKwikCheck.Api.Services;

/// <summary>
/// Composes the public inspection-report payload for a lead by overlaying its live
/// columns + stored <c>ReportData</c> onto the bundled sample template. Missing fields
/// fall back to the template so the report always renders complete (faithful clone).
/// </summary>
public interface IPublicReportService
{
    Task<JsonObject> GetReportAsync(long leadId);
}

public sealed class PublicReportService(ILeadRepository leads, IPhotoRepository photos) : IPublicReportService
{
    // The template ships as an embedded resource; parse-and-cache the raw text once.
    private static readonly string TemplateJson = LoadTemplate();

    public async Task<JsonObject> GetReportAsync(long leadId)
    {
        var lead = await leads.GetByIdAsync(leadId)
            ?? throw AppException.NotFound("Report not found");

        var report = (JsonObject)JsonNode.Parse(TemplateJson)!;
        Overlay(report, lead);

        // Stored per-template report payload wins over both template and derived values.
        if (!string.IsNullOrWhiteSpace(lead.ReportData) &&
            JsonNode.Parse(lead.ReportData) is JsonObject stored)
            DeepMerge(report, stored);

        // Surface actually-uploaded site photos (public image URLs) over the placeholders.
        await MergePhotosAsync(report, leadId);

        return report;
    }

    /// <summary>
    /// Attaches each uploaded photo's public image URL to the matching template slot (by
    /// frame-label slug), and appends any unmatched uploads as a "Site Photographs" group so
    /// nothing the inspector captured is hidden.
    /// </summary>
    private async Task MergePhotosAsync(JsonObject report, long leadId)
    {
        var uploaded = await photos.ListByLeadAsync(leadId);
        if (uploaded.Count == 0) return;

        var bySlug = new Dictionary<string, Photo>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in uploaded)
            bySlug[Slug(p.FrameLabel ?? "photo")] = p;

        var photosNode = Child(report, "photos");
        var groups = photosNode["groups"] as JsonArray ?? (JsonArray)(photosNode["groups"] = new JsonArray());
        var matched = new HashSet<long>();

        foreach (var group in groups.OfType<JsonObject>())
            if (group["items"] is JsonArray items)
                foreach (var item in items.OfType<JsonObject>())
                {
                    var slug = Slug(item["title"]?.GetValue<string>() ?? "");
                    if (slug.Length > 0 && bySlug.TryGetValue(slug, out var p))
                    {
                        item["url"] = PhotoUrl(leadId, p.Id);
                        item["stamp"] = (p.CapturedAt ?? p.UploadedAt).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                        matched.Add(p.Id);
                    }
                }

        var extras = uploaded.Where(p => !matched.Contains(p.Id)).ToList();
        if (extras.Count == 0) return;

        var extraItems = new JsonArray();
        var n = 0;
        foreach (var p in extras)
            extraItems.Add(new JsonObject
            {
                ["no"] = (++n).ToString("D2", CultureInfo.InvariantCulture),
                ["title"] = p.FrameLabel ?? "Site Photograph",
                ["note"] = p.Kind == "video" ? "Captured video frame" : "Captured on site",
                ["tag"] = "ON-SITE",
                ["stamp"] = (p.CapturedAt ?? p.UploadedAt).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                ["url"] = PhotoUrl(leadId, p.Id),
            });
        groups.Add(new JsonObject { ["title"] = "Site Photographs", ["count"] = $"{extras.Count} uploaded", ["items"] = extraItems });
    }

    private static string PhotoUrl(long leadId, long photoId) => $"/api/public/reports/{leadId}/photos/{photoId}";

    /// <summary>Maps live lead columns onto the report, only when the column has a value.</summary>
    private static void Overlay(JsonObject report, Lead lead)
    {
        var year = (lead.IssuedDate ?? lead.InspectionDate ?? lead.LeadDate ?? DateTime.UtcNow).Year;
        SetIn(report, "meta", "reportNo", $"KC-INSP-{year}-{lead.Id:D5}");

        var cover = Child(report, "cover");
        SetIfValue(cover, "reportStatus", lead.ReportStatus);
        SetIfValue(cover, "propertyClass", lead.PropertyType);
        SetIfValue(cover, "inspectionDate", FmtDate(lead.InspectionDate));
        if (!string.IsNullOrWhiteSpace(lead.Location)) cover["address"] = lead.Location;
        if (lead.Value is > 0)
        {
            cover["valuationAmount"] = $"₹ {FmtInr(lead.Value.Value)}";
        }

        var customer = Child(report, "customer");
        SetIfValue(customer, "applicantName", lead.Applicant);
        SetIfValue(customer, "coApplicantName", lead.CoApplicant);
        SetIfValue(customer, "contactNumber", lead.Contact);
        SetIfValue(customer, "personMet", lead.Applicant);

        var inspection = Child(report, "inspection");
        SetIfValue(inspection, "inspectedBy", lead.ValuatorName);
        SetIfValue(inspection, "requestedBy", lead.LenderName);
        SetIfValue(inspection, "branch", lead.Branch);
        SetIfValue(inspection, "claimNumber", lead.ClaimNo);
        SetIfValue(inspection, "fieldDate", FmtDate(lead.InspectionDate));
        if (!string.IsNullOrWhiteSpace(lead.ReqId)) inspection["leadId"] = lead.ReqId;

        SetIfValue(Child(report, "claim"), "claimNo", lead.ClaimNo);
        SetIfValue(Child(report, "summary"), "issuedOn", FmtDate(lead.IssuedDate));
    }

    // ---- helpers ------------------------------------------------------------

    private static JsonObject Child(JsonObject parent, string key)
        => parent[key] as JsonObject ?? (JsonObject)(parent[key] = new JsonObject());

    private static void SetIfValue(JsonObject obj, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)) obj[key] = value;
    }

    private static void SetIn(JsonObject root, string section, string key, string value)
        => Child(root, section)[key] = value;

    private static string? FmtDate(DateTime? d) => d?.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

    /// <summary>Same slug rule as FileService so a photo's frame label maps to its template slot.</summary>
    private static string Slug(string s) =>
        new string(s.ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray())
            .Trim('-').Replace("--", "-");

    /// <summary>Indian-grouping integer format, e.g. 21540000 → "2,15,40,000".</summary>
    private static string FmtInr(long value)
    {
        var s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Length <= 3) return s;
        var head = s[..^3];
        var tail = s[^3..];
        var grouped = string.Empty;
        while (head.Length > 2)
        {
            grouped = "," + head[^2..] + grouped;
            head = head[..^2];
        }
        return head + grouped + "," + tail;
    }

    /// <summary>Recursively merges <paramref name="src"/> into <paramref name="dst"/> (objects deep, scalars/arrays replace).</summary>
    private static void DeepMerge(JsonObject dst, JsonObject src)
    {
        foreach (var (key, node) in src)
        {
            if (node is JsonObject srcObj && dst[key] is JsonObject dstObj)
                DeepMerge(dstObj, srcObj);
            else
                dst[key] = node?.DeepClone();
        }
    }

    private static string LoadTemplate()
    {
        var asm = Assembly.GetExecutingAssembly();
        var name = Array.Find(asm.GetManifestResourceNames(), n => n.EndsWith("report-template.json", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Embedded report-template.json not found.");
        using var stream = asm.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
