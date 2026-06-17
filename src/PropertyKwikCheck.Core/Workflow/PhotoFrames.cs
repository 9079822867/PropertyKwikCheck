namespace PropertyKwikCheck.Core.Workflow;

/// <summary>The fixed photo/video frames per asset family (spec §16.4).</summary>
public static class PhotoFrames
{
    public static readonly IReadOnlyDictionary<string, string[]> Photos = new Dictionary<string, string[]>
    {
        [AssetFamily.Property] =
        [
            "Entrance with Customer", "Selfie with Property", "Approach Road", "Site Plan",
            "Map Image", "Electric Meter", "Electricity Bill", "Building Elevation",
        ],
        [AssetFamily.Plot] =
        [
            "Plot — Front View", "Plot — Rear View", "Approach Road", "Boundary / Corner Peg",
            "Layout / Site Plan", "Map / Satellite", "Adjacent Landmark", "Surveyor at Site",
        ],
        [AssetFamily.Agri] =
        [
            "Cultivated Parcel", "Irrigation Source", "Approach Track", "Boundary Bund",
            "Khasra Map", "Map / Satellite", "Power Connection", "Surveyor at Site",
        ],
    };

    public static readonly IReadOnlyDictionary<string, string[]> Videos = new Dictionary<string, string[]>
    {
        [AssetFamily.Property] = ["Property Walkthrough", "Site Approach Drive", "Interior 360° Pan"],
        [AssetFamily.Plot] = ["Plot Walkthrough", "Approach Road Drive", "Boundary Pan"],
        [AssetFamily.Agri] = ["Parcel Walkthrough", "Approach Track Drive", "Irrigation Source Clip"],
    };

    /// <summary>True if the frame label is valid for the family + kind (photo/video).</summary>
    public static bool IsValid(string family, string kind, string? frameLabel)
    {
        if (string.IsNullOrWhiteSpace(frameLabel)) return false;
        var map = kind == "video" ? Videos : Photos;
        return map.TryGetValue(family, out var frames) && Array.IndexOf(frames, frameLabel) >= 0;
    }
}
