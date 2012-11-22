//-----------------------------------------------------------------------------
// <copyright file="CacheType.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;

    /// <summary>
    /// Defines the possible <see cref="ICache"/> types.
    /// </summary>
    public enum CacheType
    {
        /// <summary>
        /// Identifies an on-disk cache.
        /// </summary>
        Disk = 0,

        /// <summary>
        /// Identifies an in-memory cache.
        /// </summary>
        Memory
    }
}