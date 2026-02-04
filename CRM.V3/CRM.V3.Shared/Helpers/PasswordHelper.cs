using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace CRM.V3.Shared.Helpers
{
    public class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                10000,
                32);

            return Convert.ToBase64String(salt.Concat(hash).ToArray());
        }

        public static bool VerifyPassword(string password, string hashed)
        {
            byte[] fullHash = Convert.FromBase64String(hashed);
            byte[] salt = fullHash.Take(16).ToArray();
            byte[] expectedHash = fullHash.Skip(16).ToArray();

            byte[] actualHash = KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                10000,
                32);

            return actualHash.SequenceEqual(expectedHash);
        }
    }
}
