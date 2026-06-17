using System.Globalization;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Common;
using PropertyKwikCheck.Core.Domain;
using PropertyKwikCheck.Core.Dtos;
using PropertyKwikCheck.Core.Rbac;
using PropertyKwikCheck.Core.Security;
using PropertyKwikCheck.Core.Workflow;

namespace PropertyKwikCheck.Infrastructure.Services;

/// <summary>
/// Upload / list / download / delete of lead documents and site photos (spec §8.11, §10).
/// Files are stored on local disk via <see cref="IFileStorage"/>; metadata in SQL Server.
/// </summary>
public sealed class FileService(
    ILeadRepository leads,
    IDocumentRepository documents,
    IPhotoRepository photos,
    IFileStorage storage,
    IAuditRepository audit) : IFileService
{
    private const long MaxDoc = 10L * 1024 * 1024;    // 10 MB
    private const long MaxPhoto = 10L * 1024 * 1024;  // 10 MB
    private const long MaxVideo = 100L * 1024 * 1024; // 100 MB

    private static readonly HashSet<string> DocMimes = new(StringComparer.OrdinalIgnoreCase)
        { "application/pdf", "image/jpeg", "image/png" };
    private static readonly HashSet<string> PhotoMimes = new(StringComparer.OrdinalIgnoreCase)
        { "image/jpeg", "image/png" };
    private static readonly HashSet<string> VideoMimes = new(StringComparer.OrdinalIgnoreCase)
        { "video/mp4" };

    // ---- documents -----------------------------------------------------------

    public async Task<DocumentDto> UploadDocumentAsync(long leadId, UploadFile file, string docType, CurrentUser user, AuditContext auditCtx)
    {
        user.Require(Capability.UploadFiles);
        await RequireLead(leadId, user);
        Validate(file, DocMimes, MaxDoc, "document");
        if (string.IsNullOrWhiteSpace(docType)) throw AppException.Validation("docType is required");

        var safe = SafeName(file.FileName);
        var key = $"leads/{leadId}/docs/{Guid.NewGuid():N}-{safe}";
        await storage.SaveAsync(key, file.Content);

        var doc = new Document
        {
            LeadId = leadId, DocType = docType, FileName = safe, StorageKey = key,
            Mime = file.ContentType, SizeBytes = file.Length, UploadedBy = user.Id,
        };
        doc.Id = await documents.InsertAsync(doc);
        doc.UploadedAt = DateTime.UtcNow; // reflect the just-persisted timestamp in the response
        await Audit(auditCtx, "document.upload", "document", doc.Id);
        return ToDto(doc);
    }

    public async Task<List<DocumentDto>> ListDocumentsAsync(long leadId, CurrentUser user)
    {
        user.Require(Capability.ViewLeads);
        await RequireLead(leadId, user);
        return (await documents.ListByLeadAsync(leadId)).Select(ToDto).ToList();
    }

    public async Task<FileDownload> DownloadDocumentAsync(long docId, CurrentUser user)
    {
        var doc = await documents.GetByIdAsync(docId) ?? throw AppException.NotFound("Document not found");
        await RequireLead(doc.LeadId, user);
        var stream = await storage.OpenAsync(doc.StorageKey) ?? throw AppException.NotFound("File missing");
        return new FileDownload(stream, doc.Mime ?? "application/octet-stream", doc.FileName);
    }

    public async Task DeleteDocumentAsync(long docId, CurrentUser user, AuditContext auditCtx)
    {
        user.Require(Capability.UploadFiles);
        var doc = await documents.GetByIdAsync(docId) ?? throw AppException.NotFound("Document not found");
        await RequireLead(doc.LeadId, user);
        await storage.DeleteAsync(doc.StorageKey);
        await documents.DeleteAsync(docId);
        await Audit(auditCtx, "document.delete", "document", docId);
    }

    // ---- photos --------------------------------------------------------------

    public async Task<PhotoDto> UploadPhotoAsync(long leadId, UploadFile file, PhotoMeta meta, CurrentUser user, AuditContext auditCtx)
    {
        user.Require(Capability.UploadFiles);
        var lead = await RequireLead(leadId, user);

        var kind = meta.Kind == "video" ? "video" : "photo";
        if (kind == "video") Validate(file, VideoMimes, MaxVideo, "video");
        else Validate(file, PhotoMimes, MaxPhoto, "photo");

        if (!PhotoFrames.IsValid(lead.AssetFamily, kind, meta.FrameLabel))
            throw AppException.Validation($"Invalid frame '{meta.FrameLabel}' for {lead.AssetFamily} {kind}");

        var ext = Path.GetExtension(file.FileName);
        var frameSlug = Slug(meta.FrameLabel);
        var key = $"leads/{leadId}/photos/{frameSlug}-{Guid.NewGuid():N}{ext}";
        await storage.SaveAsync(key, file.Content);

        var photo = new Photo
        {
            LeadId = leadId, Kind = kind, FrameLabel = meta.FrameLabel, StorageKey = key,
            Mime = file.ContentType, SizeBytes = file.Length, Lat = meta.Lat, Lng = meta.Lng,
            CapturedAt = meta.CapturedAt, UploadedBy = user.Id,
        };
        photo.Id = await photos.InsertAsync(photo);
        photo.UploadedAt = DateTime.UtcNow; // reflect the just-persisted timestamp in the response
        await Audit(auditCtx, "photo.upload", "photo", photo.Id);
        return ToDto(photo);
    }

    public async Task<List<PhotoDto>> ListPhotosAsync(long leadId, CurrentUser user)
    {
        user.Require(Capability.ViewLeads);
        await RequireLead(leadId, user);
        return (await photos.ListByLeadAsync(leadId)).Select(ToDto).ToList();
    }

    public async Task<FileDownload> DownloadPhotoAsync(long photoId, CurrentUser user)
    {
        var photo = await photos.GetByIdAsync(photoId) ?? throw AppException.NotFound("Photo not found");
        await RequireLead(photo.LeadId, user);
        var stream = await storage.OpenAsync(photo.StorageKey) ?? throw AppException.NotFound("File missing");
        var name = $"{Slug(photo.FrameLabel ?? "photo")}{Path.GetExtension(photo.StorageKey)}";
        return new FileDownload(stream, photo.Mime ?? "application/octet-stream", name);
    }

    public async Task DeletePhotoAsync(long photoId, CurrentUser user, AuditContext auditCtx)
    {
        user.Require(Capability.UploadFiles);
        var photo = await photos.GetByIdAsync(photoId) ?? throw AppException.NotFound("Photo not found");
        await RequireLead(photo.LeadId, user);
        await storage.DeleteAsync(photo.StorageKey);
        await photos.DeleteAsync(photoId);
        await Audit(auditCtx, "photo.delete", "photo", photoId);
    }

    // ---- helpers -------------------------------------------------------------

    private async Task<Lead> RequireLead(long leadId, CurrentUser user)
    {
        var lead = await leads.GetByIdAsync(leadId) ?? throw AppException.NotFound();
        LeadAccess.EnsureVisible(lead, user);
        return lead;
    }

    private static void Validate(UploadFile file, HashSet<string> mimes, long max, string label)
    {
        if (file.Length <= 0) throw AppException.Validation($"Empty {label} upload");
        if (file.Length > max) throw AppException.Validation($"{label} exceeds {max / (1024 * 1024)} MB limit");
        if (!mimes.Contains(file.ContentType))
            throw AppException.Validation($"Unsupported {label} type '{file.ContentType}'");
    }

    private static string SafeName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        foreach (var c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
        return string.IsNullOrWhiteSpace(name) ? "file" : name;
    }

    private static string Slug(string s) =>
        new string(s.ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray())
            .Trim('-').Replace("--", "-");

    private async Task Audit(AuditContext ctx, string action, string entity, long id) =>
        await audit.AddAsync(new AuditEntry
        {
            ActorUserId = ctx.Actor?.Id, Action = action, EntityType = entity,
            EntityId = id.ToString(CultureInfo.InvariantCulture), Ip = ctx.Ip, UserAgent = ctx.UserAgent,
        });

    private static DocumentDto ToDto(Document d) => new()
    {
        Id = d.Id, LeadId = d.LeadId, DocType = d.DocType, FileName = d.FileName, Mime = d.Mime,
        SizeBytes = d.SizeBytes, UploadedAt = d.UploadedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
        DownloadUrl = $"/api/documents/{d.Id}/download",
    };

    private static PhotoDto ToDto(Photo p) => new()
    {
        Id = p.Id, LeadId = p.LeadId, Kind = p.Kind, FrameLabel = p.FrameLabel, Mime = p.Mime,
        SizeBytes = p.SizeBytes, Lat = p.Lat, Lng = p.Lng,
        CapturedAt = p.CapturedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
        UploadedAt = p.UploadedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
        DownloadUrl = $"/api/photos/{p.Id}/download",
    };
}
