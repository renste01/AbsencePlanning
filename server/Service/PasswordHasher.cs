using System.Security.Cryptography;

namespace Service;

public sealed class PasswordHasher
{
    private const int SaltSize = 16; //recommended number of bits 128 = 16 bytes
    private const int HashSize = 32; //recommended number of bits 256 = 32 bytes
    private const int Iterations = 100000;

    private readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512; //algorithm for hashing passwords

    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize); //helps create password hash in a safe way that is resistant to Brute Force attacks

        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    public bool Verify(string password, string stored) //used since hashing is one way, this just checks if the password matches the stored hashed password
    {
        var parts = stored.Split('.', 3); //splits the stored hash in 3
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
            return false;
        
        //AI helped with the suggestion of using TryFromBase64
        var eSalt = new byte[SaltSize];
        var eHash = new byte[HashSize];

        if (!Convert.TryFromBase64String(parts[0], eSalt, out var saltLen) || saltLen != SaltSize) return false;
        if (!Convert.TryFromBase64String(parts[2], eHash, out var hashLen) || hashLen != HashSize) return false;

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, eSalt, iterations, Algorithm, HashSize);
        return CryptographicOperations.FixedTimeEquals(actual, eHash);
    }
    
}