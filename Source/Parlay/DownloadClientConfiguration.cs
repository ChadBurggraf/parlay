//-----------------------------------------------------------------------------
// <copyright file="DownloadClientConfiguration.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;
    using System.Globalization;
    using System.Reflection;

    /// <summary>
    /// Provides configuration options for a <see cref="DownloadClient"/>.
    /// </summary>
    public sealed class DownloadClientConfiguration
    {
        private static readonly string DefaultUserAgent = DownloadClientConfiguration.GetDefaultUserAgent();

        /// <summary>
        /// Initializes a new instance of the DownloadClientConfiguration class.
        /// </summary>
        public DownloadClientConfiguration()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DownloadClientConfiguration class.
        /// </summary>
        /// <param name="config">The existing configuration to initialize this instance with.</param>
        public DownloadClientConfiguration(DownloadClientConfiguration config)
        {
            if (config != null)
            {
            }
            else
            {
                this.UserAgent = DownloadClientConfiguration.DefaultUserAgent;
            }
        }

        /// <summary>
        /// Gets or sets the user-agent string to use when making HTTP requests.
        /// </summary>
        public string UserAgent { get; set; }

        private static string GetDefaultUserAgent()
        {
            Version version = typeof(DownloadClientConfiguration).Assembly.GetName().Version;

            return string.Format(
                CultureInfo.InvariantCulture,
                "Parlay/{0} {1}/{2}",
                version.ToString(4),
                Environment.OSVersion.Platform,
                Environment.OSVersion.Version.ToString(3));
        }
    }
}