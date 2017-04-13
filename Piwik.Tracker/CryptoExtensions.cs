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
        /// <param name="encoding">The encoding.</param>
        /// <param name="hashAsHexadecimal">if set to <c>true</c> resulting hash will be formated as hexadecimal string.</param>
        /// <returns></returns>
        public static string CreateSha1(this string valueToEncrypt, Encoding encoding, bool hashAsHexadecimal)
        {
            using (var provider = new SHA1CryptoServiceProvider())
            {
                var bytes = encoding.GetBytes(valueToEncrypt);
                var encodedBytes = provider.ComputeHash(bytes);
                if (hashAsHexadecimal)
                {
                    return GetHexStringFromBytes(encodedBytes);
                }
                return BitConverter.ToString(encodedBytes);
            }
        }

        /// <summary>
        /// Creates the a MD5 hash from given <paramref name="valueToEncrypt"/>.
        /// </summary>
        /// <param name="valueToEncrypt">The value to encrypt.</param>
        internal static string CreateMd5(this string valueToEncrypt)
        {
            using (var provider = new MD5CryptoServiceProvider())
            {
                var bytes = Encoding.Default.GetBytes(valueToEncrypt);
                var encodedBytes = provider.ComputeHash(bytes);
                var hash = BitConverter.ToString(encodedBytes);
                return hash.Replace("-", "");
            }
        }

        /// <summary>
        /// Gets the hexadecimal string from bytes.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        private static string GetHexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }
    }
}