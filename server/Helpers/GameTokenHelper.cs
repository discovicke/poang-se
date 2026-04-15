using System.Security.Cryptography;
using System.Text;

namespace server.Helpers;

public static class GameTokenHelper
{
    private static string Secret =>
        Environment.GetEnvironmentVariable("GAME_TOKEN_SECRET") ?? "dev-secret-change-me-in-production";

    public static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }

    public static string GenerateToken(Guid gameId)
    {
        var keyBytes = Encoding.UTF8.GetBytes(Secret);
        var msgBytes = Encoding.UTF8.GetBytes(gameId.ToString());
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(msgBytes);
        return Convert.ToBase64String(hash);
    }

    public static bool ValidateToken(Guid gameId, string token)
    {
        return GenerateToken(gameId) == token;
    }
}
