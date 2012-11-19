//-----------------------------------------------------------------------------
// <copyright file="CacheStatistics.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;

    /// <summary>
    /// Represents <see cref="ICache"/> statistics.
    /// </summary>
    public sealed class CacheStatistics
    {
        /// <summary>
        /// Gets or sets the number of items in the cache.
        /// </summary>
        public long ItemCount { get; set; }

        /// <summary>
        /// Gets or sets the size of the cache, in bytes.
        /// </summary>
        public long Size { get; set; }
    }
}