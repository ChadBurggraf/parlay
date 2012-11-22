//-----------------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Provides helpers and extensions.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Clones a <see cref="WebHeaderCollection"/>'s contents into a new instance.
        /// </summary>
        /// <param name="collection">The <see cref="WebHeaderCollection"/> to clone.</param>
        /// <returns>A cloned <see cref="WebHeaderCollection"/> instance.</returns>
        public static WebHeaderCollection Clone(this WebHeaderCollection collection)
        {
            WebHeaderCollection clone = new WebHeaderCollection();

            if (collection != null)
            {
                foreach (string key in collection.AllKeys)
                {
                    clone.Add(key, collection.Get(key));
                }
            }

            return clone;
        }

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
                    return hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(value)).ToHex();
                }
            }

            return value;
        }

        /// <summary>
        /// Normalizes the given <see cref="DateTime"/> to UTC.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> to normalize.</param>
        /// <returns>The normalized <see cref="DateTime"/>.</returns>
        public static DateTime NormalizeToUtc(this DateTime value)
        {
            return NormalizeToUtc(value as DateTime?).Value;
        }

        /// <summary>
        /// Normalizes the given <see cref="DateTime"/> to UTC.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> to normalize.</param>
        /// <returns>The normalized <see cref="DateTime"/>.</returns>
        public static DateTime? NormalizeToUtc(this DateTime? value)
        {
            if (value != null)
            {
                switch (value.Value.Kind)
                {
                    case DateTimeKind.Local:
                        value = value.Value.ToUniversalTime();
                        break;
                    case DateTimeKind.Unspecified:
                        value = new DateTime(value.Value.Ticks, DateTimeKind.Utc);
                        break;
                    case DateTimeKind.Utc:
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return value;
        }

        /// <summary>
        /// Reads all of the data from a stream as a byte array.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The entire contents of the stream.</returns>
        public static byte[] ReadAllBytes(this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", "stream cannot be null.");
            }

            long length = stream.Length;
            int offset = 0;
            byte[] buffer = new byte[length];

            while (offset < length)
            {
                offset += stream.Read(buffer, offset, (int)(length - offset));
            }

            return buffer;
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
