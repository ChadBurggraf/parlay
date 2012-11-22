//-----------------------------------------------------------------------------
// <copyright file="CacheProfile.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;

    /// <summary>
    /// Describes the caching profile a <see cref="Downloader"/> instance should use.
    /// </summary>
    public sealed class CacheProfile
    {
        private CacheProfile()
        {
        }

        /// <summary>
        /// Gets the cache type to use.
        /// </summary>
        public CacheType CacheType { get; private set; }

        /// <summary>
        /// Gets the path to a local directory to use for cache storage
        /// when using an on-disk cache.
        /// </summary>
        public string LocalPath { get; private set; }

        /// <summary>
        /// Gets the maximum size, in bytes, to allow the cache to grow to.
        /// </summary>
        public long MaximumSize { get; private set; }

        /// <summary>
        /// Creates a new <see cref="CacheProfile"/> that identifies an on-disk
        /// cache pointing to the given local path with the given maximum size.
        /// </summary>
        /// <param name="localPath">The path to a local directory to use for cache storage.</param>
        /// <param name="maxSize">The maximum size, in bytes, to allow the cache to grow to.</param>
        /// <returns>A new <see cref="CacheProfile"/>.</returns>
        public static CacheProfile Disk(string localPath, long maxSize)
        {
            return new CacheProfile()
            {
                CacheType = CacheType.Disk,
                LocalPath = localPath,
                MaximumSize = maxSize
            };
        }

        /// <summary>
        /// Creates a new <see cref="CacheProfile"/> that identifies an in-memory
        /// cache with the given maximum size.
        /// </summary>
        /// <param name="maxSize">The maximum size, in bytes, to allow the cache to grow to.</param>
        /// <returns>A new <see cref="CacheProfile"/>.</returns>
        public static CacheProfile Memory(long maxSize)
        {
            return new CacheProfile()
            {
                CacheType = CacheType.Memory,
                MaximumSize = maxSize
            };
        }
    }
}