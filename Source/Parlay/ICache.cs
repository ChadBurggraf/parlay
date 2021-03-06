﻿//-----------------------------------------------------------------------------
// <copyright file="ICache.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;
    using System.IO;

    /// <summary>
    /// Defines the interface for <see cref="Downloader"/> cache implementations.
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
        void AddContent(string key, byte[] content);

        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="content">The content of the item to add.</param>
        /// <param name="expires">The date the content expires.</param>
        void AddContent(string key, byte[] content, DateTime expires);

        /// <summary>
        /// Evicts items from the cache until the total cache size is smaller
        /// than or equal to the given maximum size, in bytes.
        /// </summary>
        /// <param name="maxSize">The maximum size of the cache, in bytes.</param>
        void EvictToSize(long maxSize);

        /// <summary>
        /// Gets the content for the item with the given
        /// key. Returns null if the item is not found.
        /// </summary>
        /// <param name="key">The key of the item to get.</param>
        /// <returns>The content, or null if none is found.</returns>
        byte[] GetContent(string key);

        /// <summary>
        /// Removes an item from the cache.
        /// </summary>
        /// <param name="key">The item's network identifier.</param>
        void RemoveContent(string key);
    }
}