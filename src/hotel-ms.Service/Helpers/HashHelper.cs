using System.Security.Cryptography;
using System.Text;

namespace hotelier_core_app.Service.Helpers
{
    internal class HashHelper
    {
        public static string GenerateSHA256Hash(string email, int randomStringLength = 16)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] combinedBytes = Encoding.UTF8.GetBytes(email + Guid.NewGuid().ToString("N")[..randomStringLength]);
            byte[] hashBytes = sha256.ComputeHash(combinedBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
