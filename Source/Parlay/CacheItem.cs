//-----------------------------------------------------------------------------
// <copyright file="CacheItem.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;

    /// <summary>
    /// Represents an item in an <see cref="ICache"/>.
    /// </summary>
    public sealed class CacheItem
    {
        /// <summary>
        /// Gets or sets the date the item was first accessed.
        /// </summary>
        public DateTime FirstAccessDate { get; set; }

        /// <summary>
        /// Gets or sets the item's key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the date the item was last accessed.
        /// </summary>
        public DateTime LastAccessDate { get; set; }

        /// <summary>
        /// Gets or sets the size of the item, in bytes.
        /// </summary>
        public long Size { get; set; }
    }
}