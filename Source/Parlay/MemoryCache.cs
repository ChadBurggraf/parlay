//-----------------------------------------------------------------------------
// <copyright file="MemoryCache.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;
    using System.Collections.Specialized;
    using System.Data;
    using System.Data.SQLite;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    
    /// <summary>
    /// Implements <see cref="ICache"/> with an in-memory cache database and content storage.
    /// </summary>
    public sealed class MemoryCache : ICache, ISqliteCacheStorage
    {
        private const string ConnectionString = "FullUri=file::memory:?cache=shared;DateTimeKind=Utc;Journal Mode=Off;Synchronous=Off;Version=3";
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Keeps the shared in-memory database alive.")]
        private static readonly SQLiteConnection DefaultConnection = MemoryCache.CreateAndOpenDefaultConnection();
        private static readonly HybridDictionary ContentStore = new HybridDictionary();
        private SqliteCache cache;
        private bool disposed;

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
        {
            this.cache = new SqliteCache(this, maxSize);
        }

        /// <summary>
        /// Finalizes an instance of the MemoryCache class.
        /// </summary>
        ~MemoryCache()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the number of items in the cache.
        /// </summary>
        public long ItemCount
        {
            get { return this.cache.ItemCount; }
        }

        /// <summary>
        /// Gets the size of the cache, in bytes.
        /// </summary>
        public long Size
        {
            get { return this.cache.Size; }
        }

        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="content">The content of the item to add.</param>
        public void AddContent(string key, byte[] content)
        {
            this.cache.AddContent(key, content);
        }

        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="content">The content of the item to add.</param>
        /// <param name="expires">The date the content expires.</param>
        public void AddContent(string key, byte[] content, DateTime expires)
        {
            this.cache.AddContent(key, content, expires);
        }

        /// <summary>
        /// Disposes of resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Evicts items from the cache until the total cache size is smaller
        /// than or equal to the given maximum size, in bytes.
        /// </summary>
        /// <param name="maxSize">The maximum size of the cache, in bytes.</param>
        public void EvictToSize(long maxSize)
        {
            this.cache.EvictToSize(maxSize);
        }

        /// <summary>
        /// Gets the content for the item with the given
        /// key. Returns null if the item is not found.
        /// </summary>
        /// <param name="key">The key of the item to get.</param>
        /// <returns>The content, or null if none is found.</returns>
        public byte[] GetContent(string key)
        {
            return this.cache.GetContent(key);
        }

        /// <summary>
        /// Removes an item from the cache.
        /// </summary>
        /// <param name="key">The item's network identifier.</param>
        public void RemoveContent(string key)
        {
            this.cache.RemoveContent(key);
        }

        SQLiteConnection ISqliteCacheStorage.CreateAndOpenConnection()
        {
            SQLiteConnection connection = null;

            try
            {
                connection = new SQLiteConnection(MemoryCache.ConnectionString);
                connection.Open();
                return connection;
            }
            catch
            {
                if (connection != null)
                {
                    connection.Dispose();
                }

                throw;
            }
        }

        void ISqliteCacheStorage.DeleteStoredContent(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key", "key must contain a value.");
            }

            MemoryCache.ContentStore.Remove(key);
        }

        byte[] ISqliteCacheStorage.GetStoredContent(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key", "key must contain a value.");
            }

            return MemoryCache.ContentStore[key] as byte[];
        }

        void ISqliteCacheStorage.StoreContent(string key, byte[] content)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key", "key must contain a value.");
            }

            if (content == null)
            {
                throw new ArgumentNullException("content", "content cannot be null.");
            }

            MemoryCache.ContentStore[key] = content;
        }

        private static SQLiteConnection CreateAndOpenDefaultConnection()
        {
            SQLiteConnection connection = null;

            try
            {
                connection = new SQLiteConnection(MemoryCache.ConnectionString);
                connection.Open();

                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = SqliteCache.GetSchema();
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }

                return connection;
            }
            catch
            {
                if (connection != null)
                {
                    connection.Dispose();
                }

                throw;
            }
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.cache != null)
                    {
                        this.cache.Dispose();
                    }
                }

                this.cache = null;
                this.disposed = true;
            }
        }
    }
}