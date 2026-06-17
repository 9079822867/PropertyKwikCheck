using System.Text.Json.Serialization;

namespace PropertyKwikCheck.Core.Dtos;

public sealed class DocumentDto
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("leadId")] public long LeadId { get; set; }
    [JsonPropertyName("docType")] public string DocType { get; set; } = "";
    [JsonPropertyName("fileName")] public string FileName { get; set; } = "";
    [JsonPropertyName("mime")] public string? Mime { get; set; }
    [JsonPropertyName("sizeBytes")] public long? SizeBytes { get; set; }
    [JsonPropertyName("uploadedAt")] public string? UploadedAt { get; set; }
    [JsonPropertyName("downloadUrl")] public string DownloadUrl { get; set; } = "";
}

public sealed class PhotoDto
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("leadId")] public long LeadId { get; set; }
    [JsonPropertyName("kind")] public string Kind { get; set; } = "photo";
    [JsonPropertyName("frameLabel")] public string? FrameLabel { get; set; }
    [JsonPropertyName("mime")] public string? Mime { get; set; }
    [JsonPropertyName("sizeBytes")] public long? SizeBytes { get; set; }
    [JsonPropertyName("lat")] public decimal? Lat { get; set; }
    [JsonPropertyName("lng")] public decimal? Lng { get; set; }
    [JsonPropertyName("capturedAt")] public string? CapturedAt { get; set; }
    [JsonPropertyName("uploadedAt")] public string? UploadedAt { get; set; }
    [JsonPropertyName("downloadUrl")] public string DownloadUrl { get; set; } = "";
}

/// <summary>Bytes + content type returned by a download request.</summary>
public sealed record FileDownload(Stream Content, string Mime, string FileName);
