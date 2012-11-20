//-----------------------------------------------------------------------------
// <copyright file="DownloadClient.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;
    using System.IO;
    using System.Net;

    /// <summary>
    /// Provides the primary interface for downloading cached + queued HTTP content.
    /// </summary>
    public sealed class DownloadClient
    {
        /// <summary>
        /// Initiates a cached + queued download operation.
        /// </summary>
        /// <param name="url">The URL of the resource to download.</param>
        /// <param name="options">The options to use when downloading the resource.</param>
        /// <returns>The operation's result.</returns>
        public DownloadResult Download(Uri url)
        {
            return this.Download(url, null);
        }

        /// <summary>
        /// Initiates a cached + queued download operation.
        /// </summary>
        /// <param name="url">The URL of the resource to download.</param>
        /// <param name="callback">A function to call when the operation is complete.</param>
        /// <returns>The operation's result.</returns>
        public DownloadResult Download(Uri url, Action<DownloadResult> callback)
        {
            return this.Download(url, callback, null);
        }

        /// <summary>
        /// Initiates a cached + queued download operation.
        /// </summary>
        /// <param name="url">The URL of the resource to download.</param>
        /// <param name="callback">A function to call when the operation is complete.</param>
        /// <param name="options">The options to use when downloading the resource.</param>
        /// <returns>The operation's result.</returns>
        public DownloadResult Download(Uri url, Action<DownloadResult> callback, DownloadOptions options)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url", "url cannot be null.");
            }

            options = new DownloadOptions(options);
            throw new NotImplementedException();
        }
    }
}