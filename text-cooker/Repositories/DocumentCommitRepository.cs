using Npgsql;
using text_cooker.Core.Interfaces;
using text_cooker.Entities;

namespace text_cooker.Repositories;

public class DocumentCommitRepository(string conn) : IDocumentCommitRepository
{
    public async Task<int> CreateAsync(DocumentCommit commit)
    {
        await using var conn1 = new NpgsqlConnection(conn);
        await conn1.OpenAsync();

        string sql = @"
            INSERT INTO text_cooker.DocumentCommit
                (document_id, change_start, change_end, version_number, template_id, snapshot)
            VALUES
                (@document, @start, @end, @version, @template, @snapshot)
            RETURNING commit_id;";

        await using var cmd = new NpgsqlCommand(sql, conn1);
        cmd.Parameters.AddWithValue("document", commit.DocumentId);
        cmd.Parameters.AddWithValue("start", commit.ChangeStart);
        cmd.Parameters.AddWithValue("end", commit.ChangeEnd);
        cmd.Parameters.AddWithValue("version", commit.VersionNumber);
        cmd.Parameters.AddWithValue("template", (object?)commit.TemplateId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("snapshot", commit.Snapshot);

        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<DocumentCommit?> GetLastAsync(int documentId)
    {
        await using var conn1 = new NpgsqlConnection(conn);
        await conn1.OpenAsync();

        string sql = @"
            SELECT * FROM text_cooker.DocumentCommit
            WHERE document_id=@id
            ORDER BY version_number DESC
            LIMIT 1";

        await using var cmd = new NpgsqlCommand(sql, conn1);
        cmd.Parameters.AddWithValue("id", documentId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!reader.Read()) return null;

        return new DocumentCommit
        {
            CommitId = reader.GetInt32(0),
            DocumentId = reader.GetInt32(1),
            ChangeStart = reader.GetInt32(2),
            ChangeEnd = reader.GetInt32(3),
            VersionNumber = reader.GetInt32(4),
            TemplateId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
            Snapshot = reader.GetString(6),
            CreatedAt = reader.GetDateTime(7)
        };
    }

    public async Task<List<DocumentCommit>> GetHistoryAsync(int documentId)
    {
        var list = new List<DocumentCommit>();
        await using var conn1 = new NpgsqlConnection(conn);
        await conn1.OpenAsync();

        string sql = @"
            SELECT * FROM text_cooker.DocumentCommit
            WHERE document_id = @id
            ORDER BY version_number ASC";

        await using var cmd = new NpgsqlCommand(sql, conn1);
        cmd.Parameters.AddWithValue("id", documentId);

        await using var rdr = await cmd.ExecuteReaderAsync();
        while (await rdr.ReadAsync())
        {
            list.Add(new DocumentCommit
            {
                CommitId = rdr.GetInt32(0),
                DocumentId = rdr.GetInt32(1),
                ChangeStart = rdr.GetInt32(2),
                ChangeEnd = rdr.GetInt32(3),
                VersionNumber = rdr.GetInt32(4),
                TemplateId = rdr.IsDBNull(5) ? null : rdr.GetInt32(5),
                Snapshot = rdr.GetString(6),
                CreatedAt = rdr.GetDateTime(7)
            });
        }

        return list;
    }
}
