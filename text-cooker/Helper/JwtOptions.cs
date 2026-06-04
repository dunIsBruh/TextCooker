namespace text_cooker.Helper;
public class JwtOptions(
    string issuer,
    string audience,
    string secret,
    TimeSpan accessTokenLifetime = default,
    TimeSpan refreshTokenLifetime = default)
{
    public string Issuer { get; init; } = issuer;
    public string Audience { get; init; } = audience;
    public string SecretKey { get; init; } = secret;
    public TimeSpan AccessTokenLifetime { get; init; } = accessTokenLifetime  == default 
        ? TimeSpan.FromMinutes(15) : accessTokenLifetime;
    public TimeSpan RefreshTokenLifetime { get; init; } = refreshTokenLifetime  == default 
        ? TimeSpan.FromDays(7) : refreshTokenLifetime;
}