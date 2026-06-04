using Npgsql;
using text_cooker.Core.Interfaces;
using text_cooker.Entities;

namespace text_cooker.Repositories;

public class DocumentRepository(string conn) : IDocumentRepository
{
    public async Task<int> CreateAsync(Document document)
    {
        await using var conn1 = new NpgsqlConnection(conn);
        await conn1.OpenAsync();

        string sql = @"
            INSERT INTO text_cooker.Documents(title, owner_id)
            VALUES (@title, @owner)
            RETURNING document_id;";

        await using var cmd = new NpgsqlCommand(sql, conn1);
        cmd.Parameters.AddWithValue("title", document.Title);
        cmd.Parameters.AddWithValue("owner", document.OwnerId);

        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<Document?> GetAsync(int id)
    {
        await using var conn1 = new NpgsqlConnection(conn);
        await conn1.OpenAsync();

        string sql = @"SELECT * FROM text_cooker.Documents WHERE document_id=@id;";
        await using var cmd = new NpgsqlCommand(sql, conn1);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!reader.Read()) return null;

        return new Document
        {
            DocumentId = reader.GetInt32(0),
            Title = reader.GetString(1),
            OwnerId = reader.GetInt32(2),
            CreatedAt = reader.GetDateTime(3),
            UpdatedAt = reader.GetDateTime(4)
        };
    }

    public async Task<IEnumerable<Document>> GetAllAsync(int userId)
    {
        List<Document> documents = [];
        
        await using var conn1 = new NpgsqlConnection(conn);
        await conn1.OpenAsync();
        
        string sql = @"SELECT * FROM text_cooker.Documents WHERE owner_id=@owner;";
        await using var cmd = new NpgsqlCommand(sql, conn1);
        cmd.Parameters.AddWithValue("owner", userId);
        
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            documents.Add(new Document
                {
                    DocumentId = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    OwnerId = reader.GetInt32(2),
                    CreatedAt = reader.GetDateTime(3),
                    UpdatedAt = reader.GetDateTime(4)
                }
            );
        }
        
        return documents;
    }
    

    public async Task UpdateTimestampAsync(int id)
    {
        await using var conn1 = new NpgsqlConnection(conn);
        await conn1.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            @"UPDATE text_cooker.Documents SET updated_at = NOW() WHERE document_id=@id;", conn1);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }
}
