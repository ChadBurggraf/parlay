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
    /// Serves as the result and tracking token of a <see cref="DownloadClient"/> operation.
    /// </summary>
    public sealed class DownloadResult
    {
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

        /// <summary>
        /// Gets a value indicating whether the operation was cancelled.
        /// </summary>
        public bool WasCancelled { get; private set; }

        /// <summary>
        /// Gets a <see cref="Stream"/> referring to the downloaded content.
        /// </summary>
        /// <returns>A <see cref="Stream"/> referring to the downloaded content.</returns>
        public Stream GetContent()
        {
            if (this.Error != null)
            {
                if (this.IsComplete)
                {
                    throw new NotImplementedException();
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
}