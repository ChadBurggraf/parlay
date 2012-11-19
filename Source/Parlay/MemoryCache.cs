//-----------------------------------------------------------------------------
// <copyright file="MemoryCache.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;
    using System.Collections.Specialized;
    using System.Data.SQLite;
    using System.IO;
    using Dapper;
    
    /// <summary>
    /// Implements <see cref="ICache"/> with an in-memory cache database and content storage.
    /// </summary>
    public sealed class MemoryCache : SqliteCache
    {
        private const string ConnectionString = "FullUri=file::memory:?cache=shared;Journal Mode=Off;Synchronous=Off;Version=3";
        private static readonly SQLiteConnection DefaultConnection = MemoryCache.CreateAndOpenDefaultConnection();
        private static readonly HybridDictionary ContentStore = new HybridDictionary();

        /// <summary>
        /// Initializes a new instance of the MemoryCache class.
        /// </summary>
        public MemoryCache()
            : this(104857600)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MemoryCache class.
        /// </summary>
        /// <param name="maxSize">The maximum size, in bytes, to allow the cache to grow to.</param>
        public MemoryCache(long maxSize)
            : base(maxSize)
        {
        }

        /// <summary>
        /// Creates an opens a <see cref="SQLiteConnection"/> to use for accessing cache information.
        /// </summary>
        /// <returns>A new <see cref="SQLiteConnection"/>.</returns>
        protected override SQLiteConnection CreateAndOpenConnection()
        {
            SQLiteConnection connection = new SQLiteConnection(MemoryCache.ConnectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Deletes the stored content identified by the given key.
        /// </summary>
        /// <param name="key">The key identifying the content to delete.</param>
        protected override void DeleteContent(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key", "key must contain a value.");
            }

            MemoryCache.ContentStore.Remove(key);
        }

        /// <summary>
        /// Gets the stored content for the given key
        /// </summary>
        /// <param name="key">The key identifying the stored content to get.</param>
        /// <returns>The stored content for the given key.</returns>
        protected override Stream GetContent(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key", "key must contain a value.");
            }

            Stream result = null;
            byte[] content = MemoryCache.ContentStore[key] as byte[];

            if (content != null)
            {
                result = new MemoryStream(content);
            }

            return result;
        }

        /// <summary>
        /// Stores content identified by the given key in the cache.
        /// </summary>
        /// <param name="key">The key identifying the content to store.</param>
        /// <param name="content">The content to store.</param>
        protected override void StoreContent(string key, Stream content)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key", "key must contain a value.");
            }

            if (content == null)
            {
                throw new ArgumentNullException("content", "content cannot be null.");
            }

            long length = content.Length;
            byte[] buffer = new byte[length];
            int offset = 0;

            while (offset < length)
            {
                offset += content.Read(buffer, offset, buffer.Length);
            }

            MemoryCache.ContentStore[key] = buffer;
        }

        private static SQLiteConnection CreateAndOpenDefaultConnection()
        {
            SQLiteConnection connection = new SQLiteConnection(MemoryCache.ConnectionString);
            connection.Open();
            connection.Execute(SqliteCache.GetSchema());
            return connection;
        }
    }
}