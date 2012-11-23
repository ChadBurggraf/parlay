//-----------------------------------------------------------------------------
// <copyright file="Downloader.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;

    /// <summary>
    /// Provides the primary interface for downloading cached + queued HTTP content.
    /// </summary>
    public sealed class Downloader : IDisposable
    {
        private readonly object syncRoot = new object();
        private List<DownloadTask> processing, queued;
        private ICache cache;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the Downloader class.
        /// </summary>
        public Downloader()
            : this(CacheProfile.Memory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the Downloader class.
        /// </summary>
        /// <param name="cacheProfile">The <see cref="CacheProfile"/> to use.</param>
        public Downloader(CacheProfile cacheProfile)
            : this(4, cacheProfile)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Downloader class.
        /// </summary>
        /// <param name="maxConcurrentDownloads">The maximum number of concurrent downloads allowed.</param>
        /// <param name="cacheProfile">The <see cref="CacheProfile"/> to use.</param>
        public Downloader(int maxConcurrentDownloads, CacheProfile cacheProfile)
        {
            this.MaxConcurrentDownloads = maxConcurrentDownloads < 0 ? 0 : maxConcurrentDownloads;
            this.cache = Downloader.CreateCache(cacheProfile);
            this.processing = new List<DownloadTask>();
            this.queued = new List<DownloadTask>();
        }

        /// <summary>
        /// Finalizes an instance of the Downloader class.
        /// </summary>
        ~Downloader()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the maximum number of concurrent downloads allowed.
        /// </summary>
        public int MaxConcurrentDownloads { get; private set; }

        internal int ProcessingCount
        {
            get { return this.processing.Count; }
        }

        internal int QueuedCount
        {
            get { return this.queued.Count; }
        }

        /// <summary>
        /// Cancels a pending or in-progress download operation.
        /// </summary>
        /// <param name="result">The <see cref="DownloadResult"/> representing the operation to cancel.</param>
        /// <returns>True if the operation was found and cancelled, false otherwise.</returns>
        public bool Cancel(DownloadResult result)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            if (result == null)
            {
                throw new ArgumentNullException("result", "result cannot be null.");
            }

            bool cancelled = false;
            DownloadTask task = null;

            lock (this.syncRoot)
            {
                task = this.processing.Where(t => t.Result == result).FirstOrDefault();

                if (task == null)
                {
                    task = this.queued.Where(t => t.Result == result).FirstOrDefault();

                    if (task != null)
                    {
                        this.queued.Remove(task);
                    }
                }

                if (task != null)
                {
                    task.Abort();
                    cancelled = true;
                }
            }

            return cancelled;
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
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            if (url == null)
            {
                throw new ArgumentNullException("url", "url cannot be null.");
            }

            DownloadResult result;
            string key = url.ToString().ToUpperInvariant();
            byte[] content = this.cache.GetContent(key);

            if (content != null)
            {
                result = new DownloadResult(content, true);
            }
            else
            {
                result = new DownloadResult();
                options = new DownloadOptions(options);
                this.Enqueue(new DownloadTask(this, url, callback, options, key, result));
            }

            return result;
        }

        private static ICache CreateCache(CacheProfile cacheProfile)
        {
            cacheProfile = cacheProfile ?? CacheProfile.Memory();

            switch (cacheProfile.CacheType)
            {
                case CacheType.Disk:
                    return new DiskCache(cacheProfile.LocalPath, cacheProfile.MaximumSize);
                case CacheType.Memory:
                    return new MemoryCache(cacheProfile.MaximumSize);
                default:
                    throw new NotImplementedException();
            }
        }

        private void Dequeue()
        {
            lock (this.syncRoot)
            {
                if (this.processing.Count < this.MaxConcurrentDownloads && this.queued.Count > 0)
                {
                    DownloadTask task = this.queued[0];
                    this.queued.RemoveAt(0);
                    this.processing.Add(task);
                    task.Completed += this.TaskCompleted;
                    task.Start();
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    lock (this.syncRoot)
                    {
                        foreach (DownloadTask task in this.processing)
                        {
                            task.Dispose();
                        }

                        this.processing.Clear();

                        foreach (DownloadTask task in this.queued)
                        {
                            task.Dispose();
                        }

                        this.queued.Clear();
                    }
                }

                this.disposed = true;
            }
        }

        private void Enqueue(DownloadTask task)
        {
            lock (this.syncRoot)
            {
                if (this.MaxConcurrentDownloads > 0 && this.processing.Count >= this.MaxConcurrentDownloads)
                {
                    this.queued.Add(task);
                }
                else
                {
                    this.processing.Add(task);
                    task.Completed += this.TaskCompleted;
                    task.Start();
                }
            }
        }

        private void TaskCompleted(object sender, EventArgs e)
        {
            DownloadTask task = sender as DownloadTask;

            lock (this.syncRoot)
            {
                this.processing.Remove(task);
                this.queued.Remove(task);

                try
                {
                    if (task.Error != null)
                    {
                        if (task.Callback != null)
                        {
                            task.Callback(new DownloadResult(task.Error));
                        }
                    }
                    else
                    {
                        if (!task.WasCancelled && task.Content != null)
                        {
                            this.cache.AddContent(task.Key, task.Content);

                            if (task.Callback != null)
                            {
                                task.Callback(new DownloadResult(task.Content, false));
                            }
                        }
                    }
                }
                finally
                {
                    task.Dispose();
                }
            }

            this.Dequeue();
        }
    }
}