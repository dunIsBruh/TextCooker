using text_cooker.Entities;

namespace text_cooker.Core.Interfaces;

using System.Security.Claims;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateAccessToken(string token);
}