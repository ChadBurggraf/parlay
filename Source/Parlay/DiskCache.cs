//-----------------------------------------------------------------------------
// <copyright file="DiskCache.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;
    using System.Data;
    using System.Data.SQLite;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// Implements <see cref="ICache"/> with an on-disk cache database and content storage.
    /// </summary>
    public sealed class DiskCache : ICache, ISqliteCacheStorage
    {
        private string localPath, databasePath, connectionString;
        private SqliteCache cache;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the DiskCache class.
        /// </summary>
        /// <param name="localPath">The path of the directory to store cached content in.</param>
        public DiskCache(string localPath)
            : this(localPath, 104857600)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DiskCache class.
        /// </summary>
        /// <param name="localPath">The path of the directory to store cached content in.</param>
        /// <param name="maxSize">The maximum size, in bytes, to allow the cache to grow to.</param>
        public DiskCache(string localPath, long maxSize)
        {
            if (string.IsNullOrEmpty(localPath))
            {
                throw new ArgumentNullException("path", "path must contain a value.");
            }

            this.localPath = localPath;
            this.databasePath = System.IO.Path.Combine(localPath, "Parlay.sqlite");
            this.connectionString = string.Format(CultureInfo.InvariantCulture, "Data Source={0};DateTimeKind=Utc;Journal Mode=Off;Synchronous=Off;Version=3", this.databasePath);
            this.cache = new SqliteCache(this, maxSize);

            if (!Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }
        }

        /// <summary>
        /// Finalizes an instance of the DiskCache class.
        /// </summary>
        ~DiskCache()
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

        internal string LocalPath
        {
            get { return this.localPath; }
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

            if (!File.Exists(this.databasePath))
            {
                using (connection = new SQLiteConnection(this.connectionString))
                {
                    connection.Open();

                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = SqliteCache.GetSchema();
                        command.CommandType = CommandType.Text;
                        command.ExecuteNonQuery();
                    }
                }

                connection = null;
            }

            try
            {
                connection = new SQLiteConnection(this.connectionString);
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

            string contentPath = System.IO.Path.Combine(this.localPath, key.Hash());

            if (File.Exists(contentPath))
            {
                File.Delete(contentPath);
            }
        }

        byte[] ISqliteCacheStorage.GetStoredContent(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key", "key must contain a value.");
            }

            string contentPath = System.IO.Path.Combine(this.localPath, key.Hash());

            if (File.Exists(contentPath))
            {
                return File.ReadAllBytes(contentPath);
            }

            return null;
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

            string contentPath = System.IO.Path.Combine(this.localPath, key.Hash());
            File.WriteAllBytes(contentPath, content);
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