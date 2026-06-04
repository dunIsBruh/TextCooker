using text_cooker.Core.Interfaces;
using text_cooker.Entities;

namespace text_cooker.Repositories;

using Npgsql;

public class UserRepository(string connectionString) : IUserRepository
{
    public async Task<User?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT user_id, login, password_hash, role, created_at
            FROM users
            WHERE user_id = @id;";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Login = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Role = Enum.Parse<Role>(reader.GetString(3), true),
                CreatedAt = reader.GetDateTime(4)
            };
        }

        return null;
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        const string sql = @"
            SELECT user_id, login, password_hash, role, created_at
            FROM users
            WHERE login = @login;";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@login", login);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Login = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Role = Enum.Parse<Role>(reader.GetString(3), true),
                CreatedAt = reader.GetDateTime(4)
            };
        }

        return null;
    }

    public async Task AddAsync(User user)
    {
        const string sql = @"
            INSERT INTO users (login, password_hash, role, created_at)
            VALUES (@login, @password_hash, @role, NOW())
            RETURNING user_id;";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@login", user.Login);
        cmd.Parameters.AddWithValue("@password_hash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@role", user.Role.ToString().ToLower());

        var newId = await cmd.ExecuteScalarAsync();
        user.Id = Convert.ToInt32(newId);
    }

    public async Task UpdateLastLoginAsync(int userId, DateTime dateTime)
    {
        const string sql = "UPDATE users SET last_login = @date WHERE user_id = @id;";

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@date", dateTime);
        cmd.Parameters.AddWithValue("@id", userId);

        await cmd.ExecuteNonQueryAsync();
    }
}
