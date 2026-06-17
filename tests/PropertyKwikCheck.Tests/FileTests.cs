using FluentAssertions;
using Moq;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Common;
using PropertyKwikCheck.Core.Workflow;
using PropertyKwikCheck.Infrastructure.Services;

namespace PropertyKwikCheck.Tests;

public class PhotoFramesTests
{
    [Theory]
    [InlineData("property", "photo", "Approach Road", true)]
    [InlineData("plot", "photo", "Boundary / Corner Peg", true)]
    [InlineData("agri", "video", "Parcel Walkthrough", true)]
    [InlineData("property", "photo", "Not A Frame", false)]
    [InlineData("plot", "photo", "Cultivated Parcel", false)] // agri frame on plot
    [InlineData("agri", "photo", null, false)]
    public void Validates_frame_for_family_and_kind(string family, string kind, string? frame, bool expected)
        => PhotoFrames.IsValid(family, kind, frame).Should().Be(expected);
}

public class FileServiceTests
{
    private readonly Mock<ILeadRepository> _leads = new();
    private readonly Mock<IDocumentRepository> _docs = new();
    private readonly Mock<IPhotoRepository> _photos = new();
    private readonly Mock<IFileStorage> _storage = new();
    private readonly Mock<IAuditRepository> _audit = new();
    private readonly FileService _service;

    public FileServiceTests()
    {
        _leads.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(TestData.Lead(id: 1));
        _storage.Setup(s => s.SaveAsync(It.IsAny<string>(), It.IsAny<Stream>()))
            .ReturnsAsync((string k, Stream _) => k);
        _docs.Setup(r => r.InsertAsync(It.IsAny<Core.Domain.Document>())).ReturnsAsync(5L);
        _photos.Setup(r => r.InsertAsync(It.IsAny<Core.Domain.Photo>())).ReturnsAsync(7L);
        _service = new FileService(_leads.Object, _docs.Object, _photos.Object, _storage.Object, _audit.Object);
    }

    private static UploadFile File(string mime, long size = 1024, string name = "f.pdf")
        => new(new MemoryStream(new byte[size > 4096 ? 4096 : size]), name, mime, size);

    [Fact]
    public async Task Upload_document_stores_and_returns_dto()
    {
        var dto = await _service.UploadDocumentAsync(1, File("application/pdf"), "Sale Deed", TestData.SuperAdmin(), AuditContext.System);
        dto.Id.Should().Be(5);
        dto.DownloadUrl.Should().Be("/api/documents/5/download");
        _storage.Verify(s => s.SaveAsync(It.Is<string>(k => k.StartsWith("leads/1/docs/")), It.IsAny<Stream>()), Times.Once);
    }

    [Fact]
    public async Task Upload_document_rejects_unsupported_mime()
    {
        var act = () => _service.UploadDocumentAsync(1, File("text/plain"), "Sale Deed", TestData.SuperAdmin(), AuditContext.System);
        (await act.Should().ThrowAsync<AppException>()).Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Upload_document_rejects_oversize()
    {
        var big = new UploadFile(new MemoryStream(), "f.pdf", "application/pdf", 11L * 1024 * 1024);
        var act = () => _service.UploadDocumentAsync(1, big, "Sale Deed", TestData.SuperAdmin(), AuditContext.System);
        (await act.Should().ThrowAsync<AppException>()).Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Upload_is_forbidden_without_capability()
    {
        // Client Executive lacks UploadFiles.
        var act = () => _service.UploadDocumentAsync(1, File("application/pdf"), "Sale Deed", TestData.Client(), AuditContext.System);
        (await act.Should().ThrowAsync<AppException>()).Which.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task Upload_photo_rejects_invalid_frame()
    {
        var act = () => _service.UploadPhotoAsync(1, File("image/jpeg", name: "p.jpg"),
            new PhotoMeta("Not A Real Frame", "photo", null, null, null), TestData.SuperAdmin(), AuditContext.System);
        (await act.Should().ThrowAsync<AppException>()).Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Upload_photo_accepts_valid_frame()
    {
        var dto = await _service.UploadPhotoAsync(1, File("image/jpeg", name: "p.jpg"),
            new PhotoMeta("Approach Road", "photo", null, null, null), TestData.SuperAdmin(), AuditContext.System);
        dto.Id.Should().Be(7);
        dto.FrameLabel.Should().Be("Approach Road");
    }
}
