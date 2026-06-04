using text_cooker.Core.Interfaces;
using text_cooker.Entities;

namespace text_cooker.Repositories;
using Npgsql;

public class RefreshTokenRepository(string connectionString) : IRefreshTokenRepository
{
    public async Task AddAsync(RefreshToken token)
    {
        const string sql = @"
            INSERT INTO refreshtoken 
            (user_id, token_hash, expires_at, created_at, create_by_ip, revoked_at, replaced_by_token_hash)
            VALUES (@user_id, @token_hash, @expires_at, @created_at, @create_by_ip, @revoked_at, @replaced_by_token_hash)
            RETURNING refresh_token_id;";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@user_id", token.UserId);
        cmd.Parameters.AddWithValue("@token_hash", token.TokenHash);
        cmd.Parameters.AddWithValue("@expires_at", token.ExpiresAt);
        cmd.Parameters.AddWithValue("@created_at", token.CreatedAt);
        cmd.Parameters.AddWithValue("@create_by_ip", (object?)token.CreatedByIp ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@revoked_at", (object?)token.RevokedAt ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@replaced_by_token_hash", (object?)token.ReplacedByTokenHash ?? DBNull.Value);

        var id = await cmd.ExecuteScalarAsync();
        token.Id = Convert.ToInt32(id);
    }

    public async Task<RefreshToken?> GetByHashAsync(string tokenHash)
    {
        const string sql = @"
            SELECT refresh_token_id, user_id, token_hash, expires_at, created_at, create_by_ip, revoked_at, replaced_by_token_hash
            FROM refreshtoken
            WHERE token_hash = @hash;";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@hash", tokenHash);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new RefreshToken
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                TokenHash = reader.GetString(2),
                ExpiresAt = reader.GetDateTime(3),
                CreatedAt = reader.GetDateTime(4),
                CreatedByIp = reader.IsDBNull(5) ? null : reader.GetString(5),
                RevokedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                ReplacedByTokenHash = reader.IsDBNull(7) ? null : reader.GetString(7)
            };
        }

        return null;
    }

    public async Task<IEnumerable<RefreshToken>> GetAllByUserAsync(int userId)
    {
        const string sql = @"
            SELECT refresh_token_id, user_id, token_hash, expires_at, created_at, create_by_ip, revoked_at, replaced_by_token_hash
            FROM refreshtoken
            WHERE user_id = @user_id
            ORDER BY created_at DESC;";

        var tokens = new List<RefreshToken>();

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@user_id", userId);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tokens.Add(new RefreshToken
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                TokenHash = reader.GetString(2),
                ExpiresAt = reader.GetDateTime(3),
                CreatedAt = reader.GetDateTime(4),
                CreatedByIp = reader.IsDBNull(5) ? null : reader.GetString(5),
                RevokedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                ReplacedByTokenHash = reader.IsDBNull(7) ? null : reader.GetString(7)
            });
        }

        return tokens;
    }

    public async Task UpdateAsync(RefreshToken token)
    {
        const string sql = @"
            UPDATE refreshtoken
            SET 
                revoked_at = @revoked_at,
                replaced_by_token_hash = @replaced_by_token_hash
            WHERE refresh_token_id = @id;";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@revoked_at", (object?)token.RevokedAt ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@replaced_by_token_hash", (object?)token.ReplacedByTokenHash ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id", token.Id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteExpiredAsync()
    {
        const string sql = "DELETE FROM refreshtoken WHERE expires_at < NOW();";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }
}
