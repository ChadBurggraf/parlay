//-----------------------------------------------------------------------------
// <copyright file="DownloadResult.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;
    using System.IO;

    /// <summary>
    /// Serves as the result and tracking token of a <see cref="Downloader"/> operation.
    /// </summary>
    public sealed class DownloadResult
    {
        private byte[] content;

        internal DownloadResult()
        {
        }

        internal DownloadResult(byte[] content, bool wasCached)
        {
            this.content = content;
            this.WasCached = wasCached;
            this.IsComplete = true;
        }

        internal DownloadResult(Exception error)
        {
            if (error == null)
            {
                throw new ArgumentNullException("error", "error cannot be null.");
            }

            this.Error = error;
        }

        /// <summary>
        /// Gets the downloaded content
        /// </summary>
        public byte[] Content
        {
            get
            {
                if (this.Error == null)
                {
                    if (this.IsComplete)
                    {
                        return this.content;
                    }
                    else
                    {
                        throw new InvalidOperationException("Cannot get the content stream when the operation is not complete.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot get the content stream when there was an error. See the Error property for details.");
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="Exception"/> representing the error that
        /// occurred, if applicable.
        /// </summary>
        public Exception Error { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the operation is complete.
        /// </summary>
        public bool IsComplete { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the cache was used.
        /// </summary>
        public bool WasCached { get; private set; }
    }
}