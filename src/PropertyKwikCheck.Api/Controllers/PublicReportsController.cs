using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyKwikCheck.Api.Services;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Common;

namespace PropertyKwikCheck.Api.Controllers;

/// <summary>
/// Public, no-login access to a lead's inspection report payload + its site photos —
/// backs the shareable "View Report" page (and the verify.kwikcheck.in concept). All data is
/// composed dynamically from the lead; see <see cref="IPublicReportService"/>.
/// </summary>
[AllowAnonymous]
public sealed class PublicReportsController(
    IPublicReportService reports,
    IPhotoRepository photos,
    IFileStorage storage) : ApiControllerBase
{
    [HttpGet("public/reports/{id:long}")]
    public async Task<IActionResult> Get(long id) => Ok(await reports.GetReportAsync(id));

    /// <summary>
    /// Streams an uploaded site photo for public display in the report. Scoped to the lead so
    /// the route can't be used to enumerate photos belonging to other leads.
    /// </summary>
    [HttpGet("public/reports/{leadId:long}/photos/{photoId:long}")]
    public async Task<IActionResult> Photo(long leadId, long photoId)
    {
        var photo = await photos.GetByIdAsync(photoId);
        if (photo is null || photo.LeadId != leadId) throw AppException.NotFound("Photo not found");

        var stream = await storage.OpenAsync(photo.StorageKey) ?? throw AppException.NotFound("File missing");
        Response.Headers.CacheControl = "public, max-age=86400";
        return File(stream, photo.Mime ?? "application/octet-stream");
    }
}
