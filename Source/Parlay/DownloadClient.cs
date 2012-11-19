//-----------------------------------------------------------------------------
// <copyright file="DownloadClient.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;

    /// <summary>
    /// Provides the primary interface for downloading cached + queued HTTP content.
    /// </summary>
    public sealed class DownloadClient
    {
        private DownloadClientConfiguration config;

        /// <summary>
        /// Initializes a new instance of the DownloadClient class.
        /// </summary>
        public DownloadClient()
            : this(new DownloadClientConfiguration())
        {
        }

        /// <summary>
        /// Initializes a new instance of the DownloadClient class.
        /// </summary>
        /// <param name="config">The configuration to use.</param>
        public DownloadClient(DownloadClientConfiguration config)
        {
            this.config = config ?? new DownloadClientConfiguration();
        }
    }
}