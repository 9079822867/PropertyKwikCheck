using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyKwikCheck.Core.Abstractions;

namespace PropertyKwikCheck.Api.Controllers;

[Authorize]
public sealed class PhotosController(IFileService files) : ApiControllerBase
{
    [HttpPost("leads/{leadId:long}/photos")]
    [RequestSizeLimit(100 * 1024 * 1024)] // allow video frames up to 100 MB
    public async Task<IActionResult> Upload(
        long leadId,
        [FromForm] IFormFile file,
        [FromForm] string frameLabel,
        [FromForm] string? kind,
        [FromForm] decimal? lat,
        [FromForm] decimal? lng,
        [FromForm] string? capturedAt)
    {
        if (file is null) return BadRequest(new { error = "file is required", code = "VALIDATION" });
        DateTime? captured = DateTime.TryParse(capturedAt, CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal, out var c) ? c : null;

        await using var stream = file.OpenReadStream();
        var upload = new UploadFile(stream, file.FileName, file.ContentType, file.Length);
        var meta = new PhotoMeta(frameLabel, kind ?? "photo", lat, lng, captured);
        var dto = await files.UploadPhotoAsync(leadId, upload, meta, CurrentUser, Audit);
        return StatusCode(201, dto);
    }

    [HttpGet("leads/{leadId:long}/photos")]
    public async Task<IActionResult> List(long leadId) => Ok(await files.ListPhotosAsync(leadId, CurrentUser));

    [HttpGet("photos/{photoId:long}/download")]
    public async Task<IActionResult> Download(long photoId)
    {
        var dl = await files.DownloadPhotoAsync(photoId, CurrentUser);
        return File(dl.Content, dl.Mime, dl.FileName);
    }

    [HttpDelete("photos/{photoId:long}")]
    public async Task<IActionResult> Delete(long photoId)
    {
        await files.DeletePhotoAsync(photoId, CurrentUser, Audit);
        return Ok(new { ok = true });
    }
}
