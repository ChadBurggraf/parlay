//-----------------------------------------------------------------------------
// <copyright file="DownloadTask.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;
    using System.Net;
    using System.Net.Cache;

    internal sealed class DownloadTask : IDisposable
    {
        private readonly object syncRoot = new object();
        private bool disposed, finished, started;
        private Downloader downloader;
        private WebClient webClient;
        
        public DownloadTask(Downloader downloader, Uri url, Action<DownloadResult> callback, DownloadOptions options, string key, DownloadResult result)
        {
            this.downloader = downloader;
            this.Url = url;
            this.Callback = callback;
            this.Options = options;
            this.Key = key;
            this.Result = result;
        }

        ~DownloadTask()
        {
            this.Dispose(false);
        }

        public event EventHandler Completed;

        public Action<DownloadResult> Callback { get; private set; }

        public byte[] Content { get; private set; }

        public Exception Error { get; private set; }

        public string Key { get; private set; }

        public DownloadOptions Options { get; private set; }

        public DownloadResult Result { get; private set; }

        public Uri Url { get; private set; }

        public bool WasCancelled { get; private set; }

        public void Abort()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            lock (this.syncRoot)
            {
                if (!this.finished)
                {
                    this.webClient.DownloadDataCompleted -= this.WebClientDownloadDataCompleted;
                    this.webClient.CancelAsync();
                    this.Finish(null, null, true);
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Start()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            lock (this.syncRoot)
            {
                if (!this.started && !this.finished)
                {
                    this.webClient = new WebClient();
                    this.webClient.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Reload);
                    this.webClient.Credentials = this.Options.Credentials;
                    this.webClient.Headers = this.Options.Headers;
                    this.webClient.DownloadDataCompleted += this.WebClientDownloadDataCompleted;

                    this.started = true;
                    this.webClient.DownloadDataAsync(this.Url);
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
                        this.started = false;
                        this.finished = true;

                        if (this.webClient != null)
                        {
                            this.webClient.Dispose();
                            this.webClient = null;
                        }
                    }
                }

                this.downloader = null;
                this.disposed = true;
            }
        }

        private void Finish(byte[] content, Exception error, bool wasCancelled)
        {
            if (!this.disposed)
            {
                lock (this.syncRoot)
                {
                    if (!this.finished)
                    {
                        this.started = false;
                        this.finished = true;
                        this.Content = content;
                        this.Error = error;
                        this.WasCancelled = wasCancelled;

                        EventHandler completed = this.Completed;

                        if (completed != null)
                        {
                            completed(this, EventArgs.Empty);
                        }
                    }
                }
            }
        }

        private void WebClientDownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            this.Finish(e.Result, e.Error, e.Cancelled);
        }
    }
}