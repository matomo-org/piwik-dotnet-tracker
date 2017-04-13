using System.Security.Cryptography;
using System.Text;

namespace Piwik.Tracker
{
    internal static class CryptoExtensions
    {
        /// <summary>
        /// Creates a sha1 hash from given <paramref name="valueToEncrypt" />.
        /// </summary>
        /// <param name="valueToEncrypt">The value to encrypt.</param>
        /// <returns></returns>
        public static string CreateSha1(this string valueToEncrypt)
        {
            using (var provider = new SHA1CryptoServiceProvider())
            {
                var bytes = Encoding.UTF8.GetBytes(valueToEncrypt);
                var encodedBytes = provider.ComputeHash(bytes);
                var sb = new StringBuilder();
                foreach (byte b in encodedBytes)
                {
                    var hex = b.ToString("x2");
                    sb.Append(hex);
                }
                return sb.ToString();
            }
        }
    }
}