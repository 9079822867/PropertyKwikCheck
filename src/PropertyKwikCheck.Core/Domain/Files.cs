namespace PropertyKwikCheck.Core.Domain;

/// <summary>An uploaded document attached to a lead (spec §6.2 documents).</summary>
public sealed class Document
{
    public long Id { get; set; }
    public long LeadId { get; set; }
    public string DocType { get; set; } = "";
    public string FileName { get; set; } = "";
    public string StorageKey { get; set; } = "";
    public string? Mime { get; set; }
    public long? SizeBytes { get; set; }
    public long? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
}

/// <summary>An uploaded site photo/video frame attached to a lead (spec §6.2 photos / §7.6).</summary>
public sealed class Photo
{
    public long Id { get; set; }
    public long LeadId { get; set; }
    public string Kind { get; set; } = "photo";
    public string? FrameLabel { get; set; }
    public string StorageKey { get; set; } = "";
    public string? Mime { get; set; }
    public long? SizeBytes { get; set; }
    public decimal? Lat { get; set; }
    public decimal? Lng { get; set; }
    public DateTime? CapturedAt { get; set; }
    public long? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
}
