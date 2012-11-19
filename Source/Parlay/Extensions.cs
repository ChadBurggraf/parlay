//-----------------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Provides helpers and extensions.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Hashes a string value into an SHA-1 hex string.
        /// </summary>
        /// <param name="value">The string value to hash.</param>
        /// <returns>An SHA-1 hash of the value.</returns>
        public static string Hash(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                using (SHA1CryptoServiceProvider hashAlgorithm = new SHA1CryptoServiceProvider())
                {
                    return BitConverter.ToString(hashAlgorithm.ComputeHash(Encoding.Unicode.GetBytes(value))).Replace("-", string.Empty);
                }
            }

            return value;
        }

        /// <summary>
        /// Converts the given byte array to a hex string.
        /// </summary>
        /// <param name="buffer">The byte array to convert.</param>
        /// <returns>A hex string.</returns>
        public static string ToHex(this byte[] buffer)
        {
            StringBuilder sb = new StringBuilder(buffer.Length * 2);

            foreach (byte b in buffer)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", b);
            }

            return sb.ToString();
        }
    }
}
