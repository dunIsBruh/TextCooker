namespace text_cooker.Helper;

using System.Security.Cryptography;
using System.Text;

public static class TokenHasher
{
    public static string Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}