//-----------------------------------------------------------------------------
// <copyright file="ICache.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;
    using System.IO;

    /// <summary>
    /// Defines the interface for <see cref="DownloadClient"/> cache implementations.
    /// </summary>
    public interface ICache : IDisposable
    {
        /// <summary>
        /// Gets the number of items in the cache.
        /// </summary>
        long ItemCount { get; }

        /// <summary>
        /// Gets the size of the cache, in bytes.
        /// </summary>
        long Size { get; }

        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="content">The content of the item to add.</param>
        void Add(string key, Stream content);

        /// <summary>
        /// Evicts items from the cache until the total cache size is smaller
        /// than or equal to the given maximum size, in bytes.
        /// </summary>
        /// <param name="maxSize">The maximum size of the cache, in bytes.</param>
        void EvictToSize(long maxSize);

        /// <summary>
        /// Gets a <see cref="Stream"/> of content for the item with the given
        /// key. Returns null if the item is not found.
        /// </summary>
        /// <param name="key">The key of the item to get.</param>
        /// <returns>A <see cref="Stream"/> of item content, or null if none is found.</returns>
        Stream Get(string key);

        /// <summary>
        /// Removes an item from the cache.
        /// </summary>
        /// <param name="key">The item's network identifier.</param>
        void Remove(string key);
    }
}