using text_cooker.Core;
using text_cooker.Core.Attributes;
using text_cooker.Core.Interfaces;
using text_cooker.Entities;
using text_cooker.Helper;
using text_cooker.Models;

namespace text_cooker.Controllers;

public class AuthController(
    IUserRepository users,
    IJwtService jwt,
    IRefreshTokenRepository refreshRepo,
    JwtOptions opts)
{
    public async Task Register([Body] AuthModels.RegisterRequest registerRequest, ContextEx ctx)
    {
        if (string.IsNullOrWhiteSpace(registerRequest.Login) || string.IsNullOrWhiteSpace(registerRequest.Password))
        {
            await ctx.BadRequest(new { error = "username and password required" });
            return;
        }

        var existing = await users.GetByLoginAsync(registerRequest.Login);
        if (existing != null)
        {
            await ctx.Send(409, new { error = "username already exists" });
            return;
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);
        var user = new User { Login = registerRequest.Login, PasswordHash = hash };
        await users.AddAsync(user);
        
        await ctx.Created(new { message = "registered" });
    }

    public async Task Login([Body] AuthModels.LoginRequest loginRequest, ContextEx ctx)
    {
        var user = await users.GetByLoginAsync(loginRequest.Login);
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
        {
            await ctx.Send(401, new { error = "invalid credentials" });
            return;
        }

        var access = jwt.GenerateAccessToken(user);
        var refreshRaw = jwt.GenerateRefreshToken();
        var refreshHash = TokenHasher.Hash(refreshRaw);

        var token = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = DateTime.UtcNow.Add(opts.RefreshTokenLifetime),
            CreatedByIp = ctx.Request.RemoteEndPoint?.Address.MapToIPv4().ToString()
        };

        await refreshRepo.AddAsync(token);

        await ctx.Ok(new
        {
            accessToken = access,
            refreshToken = refreshRaw
        });
    }

    public async Task Refresh([Body] AuthModels.RefreshRequest refreshRequest, ContextEx ctx)
    {
        if (string.IsNullOrWhiteSpace(refreshRequest.RefreshToken))
        {
            await ctx.BadRequest(new { error = "refreshToken required" });
            return;
        }

        var hash = TokenHasher.Hash(refreshRequest.RefreshToken);
        var stored = await refreshRepo.GetByHashAsync(hash);
        if (stored == null || !stored.IsActive)
        {
            await ctx.Unauthorized(new { error = "invalid or expired refresh token" });
            return;
        }

        var user = await users.GetByIdAsync(stored.UserId);
        if (user == null)
        {
            await ctx.NotFound(new { error = "user not found" });
            return;
        }

        // Ротация токена
        stored.RevokedAt = DateTime.UtcNow;
        var newRaw = jwt.GenerateRefreshToken();
        var newHash = TokenHasher.Hash(newRaw);

        var newToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newHash,
            ExpiresAt = DateTime.UtcNow.Add(opts.RefreshTokenLifetime),
            CreatedByIp = ctx.Request.RemoteEndPoint.Address.MapToIPv4().ToString()
        };
        stored.ReplacedByTokenHash = newHash;

        await refreshRepo.UpdateAsync(stored);
        await refreshRepo.AddAsync(newToken);

        var access = jwt.GenerateAccessToken(user);
        await ctx.Ok(new
        {
            accessToken = access,
            refreshToken = newRaw
        });
    }

    public async Task Logout([Body] AuthModels.LogoutRequest logoutRequest, ContextEx ctx)
    {
        var hash = TokenHasher.Hash(logoutRequest.RefreshToken);
        var stored = await refreshRepo.GetByHashAsync(hash);
        if (stored != null)
        {
            stored.RevokedAt = DateTime.UtcNow;
            await refreshRepo.UpdateAsync(stored);
        }

        ctx.NoContent();
    }
}
