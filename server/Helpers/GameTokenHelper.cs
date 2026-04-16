using System.Security.Cryptography;
using System.Text;

namespace server.Helpers;

/// <summary>
/// Kryptografiska hjälpmetoder för lösenordsskyddad
/// </summary>
public static class GameTokenHelper
{
    private static readonly byte[] SecretKey =
        Encoding.UTF8.GetBytes(
            Environment.GetEnvironmentVariable("GAME_TOKEN_SECRET")
            ?? "dev-secret-change-in-production");

    public static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static string GenerateToken(Guid gameId)
    {
        var data = Encoding.UTF8.GetBytes(gameId.ToString());
        var hash = HMACSHA256.HashData(SecretKey, data);
        return Convert.ToBase64String(hash);
    }

    public static bool ValidateToken(Guid gameId, string token)
    {
        var expected = GenerateToken(gameId);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var actualBytes = Encoding.UTF8.GetBytes(token);
        return CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }
}