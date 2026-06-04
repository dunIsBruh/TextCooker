using text_cooker.Core.Interfaces;
using text_cooker.Entities;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;

namespace text_cooker.Helper;

public class JwtService : IJwtService
{
    private readonly JwtOptions _opts;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly SigningCredentials _signingCredentials;
    private readonly JwtSecurityTokenHandler _handler = new();

    public JwtService(JwtOptions opts)
    {
        _opts = opts;
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.SecretKey));
        _signingCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
    }

    public string GenerateAccessToken(User user)
    {
        var now = DateTime.UtcNow;
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Login),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: _opts.Audience,
            claims: claims,
            notBefore: now,
            expires: now.Add(_opts.AccessTokenLifetime),
            signingCredentials: _signingCredentials
        );

        return _handler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _opts.Issuer,
            ValidateAudience = true,
            ValidAudience = _opts.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        try
        {
            var principal = _handler.ValidateToken(token, validationParams, out var validatedToken);

            // дополнительные проверки можно сделать здесь (алгоритм, etc.)
            return principal;
        }
        catch
        {
            // TODO: обработать разные виды ошибок в ErrMiddleware
            return null;
        }
    }
}
