using PropertyKwikCheck.Core.Domain;
using PropertyKwikCheck.Core.Dtos;
using PropertyKwikCheck.Core.Security;

namespace PropertyKwikCheck.Core.Abstractions;

/// <summary>An uploaded file, decoupled from ASP.NET's IFormFile.</summary>
public sealed record UploadFile(Stream Content, string FileName, string ContentType, long Length);

/// <summary>Binary storage backend. The local-disk implementation lives in Infrastructure.</summary>
public interface IFileStorage
{
    /// <summary>Persists the stream under <paramref name="key"/> and returns the stored key.</summary>
    Task<string> SaveAsync(string key, Stream content);

    /// <summary>Opens a readable stream for the key, or null if missing.</summary>
    Task<Stream?> OpenAsync(string key);

    Task DeleteAsync(string key);
}

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(long id);
    Task<List<Document>> ListByLeadAsync(long leadId);
    Task<long> InsertAsync(Document doc);
    Task DeleteAsync(long id);
}

public interface IPhotoRepository
{
    Task<Photo?> GetByIdAsync(long id);
    Task<List<Photo>> ListByLeadAsync(long leadId);
    Task<long> InsertAsync(Photo photo);
    Task DeleteAsync(long id);
}

public sealed record PhotoMeta(string FrameLabel, string Kind, decimal? Lat, decimal? Lng, DateTime? CapturedAt);

/// <summary>Generates (and caches, for issued leads) the lead's valuation report PDF (spec §8.12, §11).</summary>
public interface IReportPdfService
{
    Task<FileDownload> GenerateAsync(long leadId, CurrentUser user);
}

public interface IFileService
{
    Task<DocumentDto> UploadDocumentAsync(long leadId, UploadFile file, string docType, CurrentUser user, AuditContext audit);
    Task<List<DocumentDto>> ListDocumentsAsync(long leadId, CurrentUser user);
    Task<FileDownload> DownloadDocumentAsync(long docId, CurrentUser user);
    Task DeleteDocumentAsync(long docId, CurrentUser user, AuditContext audit);

    Task<PhotoDto> UploadPhotoAsync(long leadId, UploadFile file, PhotoMeta meta, CurrentUser user, AuditContext audit);
    Task<List<PhotoDto>> ListPhotosAsync(long leadId, CurrentUser user);
    Task<FileDownload> DownloadPhotoAsync(long photoId, CurrentUser user);
    Task DeletePhotoAsync(long photoId, CurrentUser user, AuditContext audit);
}
