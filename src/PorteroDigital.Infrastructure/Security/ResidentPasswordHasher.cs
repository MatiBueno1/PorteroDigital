using System.Security.Cryptography;

namespace PorteroDigital.Infrastructure.Security;

public sealed class ResidentPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100000;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        return Hash(password, salt);
    }

    public bool Verify(string password, string storedHash)
    {
        var parts = storedHash.Split('.', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 3 || !string.Equals(parts[0], "v1", StringComparison.Ordinal))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[1]);
        return string.Equals(Hash(password, salt), storedHash, StringComparison.Ordinal);
    }

    public static string HashSeed(string password, Guid residentId)
    {
        return Hash(password, residentId.ToByteArray());
    }

    private static string Hash(string password, byte[] salt)
    {
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"v1.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }
}
