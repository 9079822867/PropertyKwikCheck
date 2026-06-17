using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyKwikCheck.Core.Abstractions;

namespace PropertyKwikCheck.Api.Controllers;

[Authorize]
public sealed class DocumentsController(IFileService files) : ApiControllerBase
{
    [HttpPost("leads/{leadId:long}/documents")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(long leadId, [FromForm] IFormFile file, [FromForm] string docType)
    {
        if (file is null) return BadRequest(new { error = "file is required", code = "VALIDATION" });
        await using var stream = file.OpenReadStream();
        var upload = new UploadFile(stream, file.FileName, file.ContentType, file.Length);
        var dto = await files.UploadDocumentAsync(leadId, upload, docType, CurrentUser, Audit);
        return StatusCode(201, dto);
    }

    [HttpGet("leads/{leadId:long}/documents")]
    public async Task<IActionResult> List(long leadId) => Ok(await files.ListDocumentsAsync(leadId, CurrentUser));

    [HttpGet("documents/{docId:long}/download")]
    public async Task<IActionResult> Download(long docId)
    {
        var dl = await files.DownloadDocumentAsync(docId, CurrentUser);
        return File(dl.Content, dl.Mime, dl.FileName);
    }

    [HttpDelete("documents/{docId:long}")]
    public async Task<IActionResult> Delete(long docId)
    {
        await files.DeleteDocumentAsync(docId, CurrentUser, Audit);
        return Ok(new { ok = true });
    }
}
