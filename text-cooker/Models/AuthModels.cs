namespace text_cooker.Models;

public static class AuthModels
{
    public record RegisterRequest(string Login, string Password, string ConfirmPassword);
    public record LoginRequest(string Login, string Password);
    public record RefreshRequest(string RefreshToken);
    public record LogoutRequest(string RefreshToken);
}