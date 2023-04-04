using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Hedonist.Business {
    public static class PasswordHasher {

        private static byte[] salt;
        private const int SaltSize = 32;
        private const int Iterations = 1000;
        private const int HashSize = 20;

        static PasswordHasher() {
            salt = new byte[] { 0x2, 0x5, 0x7, 0x8, 0xA, 0x5, 0x7, 0xE, 0xA, 0x1, 0x6, 0xC, 0x4, 0x4, 0x6, 0x7, 0x8, 0xC, 0xC, 0x7, 0xE, 0xF, 0x7, 0x3, 0xE, 0x5, 0xD, 0x9, 0x6, 0x4, 0x3, 0xC };
        }
        public static string Hash(string password) {
            
            // Create hash
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
            var hash = pbkdf2.GetBytes(HashSize);

            // Combine salt and hash
            var hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            // Convert to base64
            var base64Hash = Convert.ToBase64String(hashBytes);
            return base64Hash;
        }

      
        public static bool Verify(string password, string hashedPassword) {
            var base64Hash = hashedPassword;

            // Get hash bytes
            var hashBytes = Convert.FromBase64String(base64Hash);

            // Get salt
            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Create hash with given salt
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            // Get result
            for (var i = 0; i < HashSize; i++) {
                if (hashBytes[i + SaltSize] != hash[i]) {
                    return false;
                }
            }
            return true;
        }
    }
}