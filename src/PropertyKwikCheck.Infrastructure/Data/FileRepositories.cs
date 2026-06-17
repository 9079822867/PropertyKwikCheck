using Dapper;
using PropertyKwikCheck.Core.Abstractions;
using PropertyKwikCheck.Core.Domain;

namespace PropertyKwikCheck.Infrastructure.Data;

public sealed class DocumentRepository(IDbConnectionFactory factory) : IDocumentRepository
{
    private const string Cols = "id, lead_id, doc_type, file_name, storage_key, mime, size_bytes, uploaded_by, uploaded_at";

    public async Task<Document?> GetByIdAsync(long id)
    {
        using var conn = await factory.OpenAsync();
        return await conn.QuerySingleOrDefaultAsync<Document>(
            $"SELECT {Cols} FROM documents WHERE id = @id", new { id });
    }

    public async Task<List<Document>> ListByLeadAsync(long leadId)
    {
        using var conn = await factory.OpenAsync();
        return (await conn.QueryAsync<Document>(
            $"SELECT {Cols} FROM documents WHERE lead_id = @leadId ORDER BY uploaded_at DESC", new { leadId })).ToList();
    }

    public async Task<long> InsertAsync(Document doc)
    {
        using var conn = await factory.OpenAsync();
        return await conn.ExecuteScalarAsync<long>("""
            INSERT INTO documents (lead_id, doc_type, file_name, storage_key, mime, size_bytes, uploaded_by)
            OUTPUT INSERTED.id
            VALUES (@LeadId, @DocType, @FileName, @StorageKey, @Mime, @SizeBytes, @UploadedBy)
            """, doc);
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = await factory.OpenAsync();
        await conn.ExecuteAsync("DELETE FROM documents WHERE id = @id", new { id });
    }
}

public sealed class PhotoRepository(IDbConnectionFactory factory) : IPhotoRepository
{
    private const string Cols = "id, lead_id, kind, frame_label, storage_key, mime, size_bytes, lat, lng, captured_at, uploaded_by, uploaded_at";

    public async Task<Photo?> GetByIdAsync(long id)
    {
        using var conn = await factory.OpenAsync();
        return await conn.QuerySingleOrDefaultAsync<Photo>(
            $"SELECT {Cols} FROM photos WHERE id = @id", new { id });
    }

    public async Task<List<Photo>> ListByLeadAsync(long leadId)
    {
        using var conn = await factory.OpenAsync();
        return (await conn.QueryAsync<Photo>(
            $"SELECT {Cols} FROM photos WHERE lead_id = @leadId ORDER BY uploaded_at DESC", new { leadId })).ToList();
    }

    public async Task<long> InsertAsync(Photo photo)
    {
        using var conn = await factory.OpenAsync();
        return await conn.ExecuteScalarAsync<long>("""
            INSERT INTO photos (lead_id, kind, frame_label, storage_key, mime, size_bytes, lat, lng, captured_at, uploaded_by)
            OUTPUT INSERTED.id
            VALUES (@LeadId, @Kind, @FrameLabel, @StorageKey, @Mime, @SizeBytes, @Lat, @Lng, @CapturedAt, @UploadedBy)
            """, photo);
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = await factory.OpenAsync();
        await conn.ExecuteAsync("DELETE FROM photos WHERE id = @id", new { id });
    }
}
