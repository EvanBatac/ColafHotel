using System.Security.Cryptography;

namespace ColafHotel.Helpers;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"pbkdf2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static string HashPasswordForSeed(string password, string seedSalt)
    {
        var salt = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(seedSalt)).Take(SaltSize).ToArray();
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"pbkdf2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPassword(string password, string hash)
    {
        if (VerifyModernHash(password, hash))
        {
            return true;
        }

        return VerifyLegacyHash(password, hash);
    }

    private static bool VerifyModernHash(string password, string hash)
    {
        var parts = hash.Split('$');
        if (parts.Length != 4 || parts[0] != "pbkdf2")
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        return VerifyPbkdf2(password, iterations, parts[2], parts[3]);
    }

    private static bool VerifyLegacyHash(string password, string hash)
    {
        var parts = hash.Split(':');
        if (parts.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        return VerifyPbkdf2(password, iterations, parts[0], parts[2]);
    }

    private static bool VerifyPbkdf2(string password, int iterations, string saltBase64, string hashBase64)
    {
        try
        {
            var salt = Convert.FromBase64String(saltBase64);
            var storedHash = Convert.FromBase64String(hashBase64);
            var computedHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, storedHash.Length);

            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
