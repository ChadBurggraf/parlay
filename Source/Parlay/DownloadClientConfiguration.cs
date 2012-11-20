//-----------------------------------------------------------------------------
// <copyright file="DownloadClientConfiguration.cs" company="Tasty Codes">
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
                this.Credentials = config.Credentials;
                this.Headers = config.Headers.Clone();
                this.QueryString = config.QueryString.Clone();
                this.UserAgent = config.UserAgent;
            }
            else
            {
                this.Headers = new WebHeaderCollection();
                this.QueryString = new NameValueCollection();
                this.UserAgent = DownloadClientConfiguration.DefaultUserAgent;
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
        /// Gets a collection of query string key/value pairs to send when making HTTP requests.
        /// </summary>
        public NameValueCollection QueryString { get; private set; }

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