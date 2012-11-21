//-----------------------------------------------------------------------------
// <copyright file="DownloadClient.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    /// <summary>
    /// Provides the primary interface for downloading cached + queued HTTP content.
    /// </summary>
    public sealed class DownloadClient : IDisposable
    {
        private readonly object syncRoot = new object();
        private Queue<DownloadTask> queue;
        private ICache cache;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the DownloadClient class.
        /// </summary>
        public DownloadClient()
        {
            this.cache = new MemoryCache();
            this.queue = new Queue<DownloadTask>();
        }

        /// <summary>
        /// Finalizes an instance of the DownloadClient class.
        /// </summary>
        ~DownloadClient()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Cancels a pending or in-progress download operation.
        /// </summary>
        /// <param name="result">The <see cref="DownloadResult"/> representing the operation to cancel.</param>
        /// <returns>True if the operation was found and cancelled, false otherwise.</returns>
        public bool Cancel(DownloadResult result)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Disposes of resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initiates a cached + queued download operation.
        /// </summary>
        /// <param name="url">The URL of the resource to download.</param>
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

        private void Dequeue()
        {
            throw new NotImplementedException();
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                }

                this.disposed = true;
            }
        }

        private void Enqueue(Uri url, Action<DownloadResult> callback, DownloadOptions options)
        {
            throw new NotImplementedException();
        }
    }
}