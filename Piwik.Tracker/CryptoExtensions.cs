using System;
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
        public static string ToSha1(this string valueToEncrypt)
        {
            if (valueToEncrypt == null)
            {
                throw new ArgumentNullException(nameof(valueToEncrypt));
            }
            return Encoding.UTF8.GetBytes(valueToEncrypt).ToSha1();
        }

        /// <summary>
        /// Creates a sha1 hash from given <paramref name="valueToEncrypt" />.
        /// </summary>
        /// <param name="valueToEncrypt">The value to encrypt.</param>
        /// <returns></returns>
        public static string ToSha1(this byte[] valueToEncrypt)
        {
            if (valueToEncrypt == null)
            {
                throw new ArgumentNullException(nameof(valueToEncrypt));
            }
            using (var provider = new SHA1CryptoServiceProvider())
            {
                var encodedBytes = provider.ComputeHash(valueToEncrypt);
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