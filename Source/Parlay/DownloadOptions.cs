//-----------------------------------------------------------------------------
// <copyright file="DownloadOptions.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Net;
    using System.Reflection;

    /// <summary>
    /// Provides configuration options for a <see cref="Downloader"/> operation.
    /// </summary>
    public sealed class DownloadOptions
    {
        private static readonly string DefaultUserAgent = DownloadOptions.GetDefaultUserAgent();
        
        /// <summary>
        /// Initializes a new instance of the DownloadOptions class.
        /// </summary>
        public DownloadOptions()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DownloadOptions class.
        /// </summary>
        /// <param name="config">The existing configuration to initialize this instance with.</param>
        public DownloadOptions(DownloadOptions config)
        {
            if (config != null)
            {
                this.Credentials = config.Credentials;
                this.Headers = config.Headers.Clone();
                this.UserAgent = config.UserAgent;
            }
            else
            {
                this.Headers = new WebHeaderCollection();
                this.UserAgent = DownloadOptions.DefaultUserAgent;
            }
        }

        /// <summary>
        /// Gets or sets the credentials to use when making HTTP requests.
        /// </summary>
        public ICredentials Credentials { get; set; }

        /// <summary>
        /// Gets a collection of additional custom headers to send when making HTTP requests.
        /// </summary>
        public WebHeaderCollection Headers { get; private set; }

        /// <summary>
        /// Gets or sets the user-agent string to use when making HTTP requests.
        /// </summary>
        public string UserAgent { get; set; }

        private static string GetDefaultUserAgent()
        {
            Version version = typeof(DownloadOptions).Assembly.GetName().Version;

            return string.Format(
                CultureInfo.InvariantCulture,
                "Parlay/{0} {1}/{2}",
                version.ToString(4),
                Environment.OSVersion.Platform,
                Environment.OSVersion.Version.ToString(3));
        }
    }
}