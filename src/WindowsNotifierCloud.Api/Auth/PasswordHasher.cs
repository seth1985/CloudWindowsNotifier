using System.Security.Cryptography;
using System.Text;

namespace WindowsNotifierCloud.Api.Auth;

public static class PasswordHasher
{
    public static string HashPassword(string password, int iterations = 100_000)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, 16, iterations, HashAlgorithmName.SHA256);
        var salt = pbkdf2.Salt;
        var key = pbkdf2.GetBytes(32);
        var payload = new byte[1 + 4 + salt.Length + key.Length];
        payload[0] = 0x01; // version
        BitConverter.GetBytes(iterations).CopyTo(payload, 1);
        Buffer.BlockCopy(salt, 0, payload, 5, salt.Length);
        Buffer.BlockCopy(key, 0, payload, 5 + salt.Length, key.Length);
        return Convert.ToBase64String(payload);
    }

    public static bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(hash)) return false;
        var payload = Convert.FromBase64String(hash);
        if (payload.Length < 1 + 4 + 16 + 32) return false;
        var version = payload[0];
        if (version != 0x01) return false;
        var iterations = BitConverter.ToInt32(payload, 1);
        var salt = new byte[16];
        Buffer.BlockCopy(payload, 5, salt, 0, 16);
        var key = new byte[32];
        Buffer.BlockCopy(payload, 5 + 16, key, 0, 32);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        var computed = pbkdf2.GetBytes(32);
        return CryptographicOperations.FixedTimeEquals(computed, key);
    }
}
