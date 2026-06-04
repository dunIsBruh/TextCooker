using Npgsql;
using text_cooker.Core.Interfaces;
using text_cooker.Entities;

namespace text_cooker.Repositories;

public class TemplateRepository(string connectionString) : ITemplateRepository
{
    public async Task<int> CreateAsync(Template template)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        string sql = @"
            INSERT INTO text_cooker.Templates (name, text, owner_id, is_public)
            VALUES (@name, @structure, @owner_id, @is_public)
            RETURNING template_id;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", template.Name);
        cmd.Parameters.AddWithValue("structure", template.Text);
        cmd.Parameters.AddWithValue("owner_id", template.OwnerId);
        cmd.Parameters.AddWithValue("is_public", template.IsPublic);

        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<Template?> GetAsync(int id)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        string sql = @"SELECT * FROM text_cooker.Templates WHERE template_id=@id;";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!reader.Read()) return null;

        return new Template
        {
            TemplateId = reader.GetInt32(0),
            Name = reader.GetString(1),
            Text = reader.GetString(2),
            OwnerId = reader.GetInt32(3),
            CreatedAt = reader.GetDateTime(4),
            IsPublic = reader.GetBoolean(5)
        };
    }

    // search by owner or public
    public async Task<IEnumerable<Template>> GetAvailableAsync(int ownerId)
    {
        var list = new List<Template>();
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        string sql = @"
            SELECT * FROM text_cooker.Templates
            WHERE owner_id = @owner OR is_public = true";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("owner", ownerId);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Template
            {
                TemplateId = reader.GetInt32(0),
                Name = reader.GetString(1),
                Text = reader.GetString(2),
                OwnerId = reader.GetInt32(3),
                CreatedAt = reader.GetDateTime(4),
                IsPublic = reader.GetBoolean(5)
            });
        }

        return list;
    }
}