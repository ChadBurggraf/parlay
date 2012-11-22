//-----------------------------------------------------------------------------
// <copyright file="SqliteCache.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SQLite;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using Dapper;

    /// <summary>
    /// Provides a base <see cref="ICache"/> implementation for SQLite caches.
    /// </summary>
    public abstract class SqliteCache : ICache
    {
        private const string RemoveSql =
@"UPDATE [ParlayStatistics]
SET
    [ItemCount] = [ItemCount] - 1,
    [Size] = [Size] - @Size;

DELETE FROM [ParlayItem]
WHERE
    [Key] = @Key;

SELECT *
FROM [ParlayStatistics];";

        private readonly object syncRoot = new object();
        private long maxSize;
        private long? itemCount, size;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the SqliteCache class.
        /// </summary>
        /// <param name="maxSize">The maximum size, in bytes, to allow the cache to grow to.</param>
        protected SqliteCache(long maxSize)
        {
            this.maxSize = maxSize > 0 ? maxSize : 0;
        }

        /// <summary>
        /// Finalizes an instance of the SqliteCache class.
        /// </summary>
        ~SqliteCache()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the number of items in the cache.
        /// </summary>
        public long ItemCount
        {
            get
            {
                this.EnsureStatistics();
                return this.itemCount.Value;
            }
        }

        /// <summary>
        /// Gets the size of the cache, in bytes.
        /// </summary>
        public long Size
        {
            get
            {
                this.EnsureStatistics();
                return this.size.Value;
            }
        }

        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="content">The content of the item to add.</param>
        public void AddContent(string key, byte[] content)
        {
            this.AddContentImpl(key, content, null);
        }

        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="content">The content of the item to add.</param>
        /// <param name="expires">The date the content expires.</param>
        public void AddContent(string key, byte[] content, DateTime expires)
        {
            this.AddContentImpl(key, content, expires);
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
            if (maxSize < 0)
            {
                maxSize = 0;
            }

            if (this.Size > maxSize)
            {
                using (IDbConnection connection = this.CreateAndOpenConnection())
                {
                    using (IDbTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            lock (this.syncRoot)
                            {
                                this.EvictExpired(connection, transaction);
                                this.EvictToSize(maxSize, connection, transaction);
                                transaction.Commit();
                            }
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the content for the item with the given
        /// key. Returns null if the item is not found.
        /// </summary>
        /// <param name="key">The key of the item to get.</param>
        /// <returns>The content, or null if none is found.</returns>
        public byte[] GetContent(string key)
        {
            byte[] content = null;
            CacheItem item = this.GetItem(key, null);

            if (item != null)
            {
                if (item.ExpireDate == null || item.ExpireDate > DateTime.UtcNow)
                {
                    content = this.GetStoredContent(item.Key);

                    if (content == null)
                    {
                        this.RemoveContent(key);
                    }
                }
                else
                {
                    this.RemoveContent(key);
                }
            }

            return content;
        }

        /// <summary>
        /// Removes an item from the cache.
        /// </summary>
        /// <param name="key">The item's network identifier.</param>
        public void RemoveContent(string key)
        {
            using (IDbConnection connection = this.CreateAndOpenConnection())
            {
                using (IDbTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        lock (this.syncRoot)
                        {
                            this.RemoveContent(key, connection, transaction);
                            transaction.Commit();
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the schema to use for a SQLite cache.
        /// </summary>
        /// <returns>The schema definition.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Performance.")]
        protected static string GetSchema()
        {
            Stream stream = null;

            try
            {
                stream = typeof(SqliteCache).Assembly.GetManifestResourceStream("Parlay.SqliteCache.Schema.sql");

                using (StreamReader reader = new StreamReader(stream))
                {
                    stream = null;
                    return reader.ReadToEnd();
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
        }

        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="content">The content of the item to add.</param>
        /// <param name="expires">The date the content expires, if applicable.</param>
        protected void AddContentImpl(string key, byte[] content, DateTime? expires)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key", "key must contain a value.");
            }

            if (content == null)
            {
                throw new ArgumentNullException("content", "content cannot be null.");
            }

            const string Sql =
@"INSERT INTO [ParlayItem]([ExpireDate],[FirstAccessDate],[Key],[LastAccessDate],[Size])
VALUES(@ExpireDate,@FirstAccessDate,@Key,@LastAccessDate,@Size);

UPDATE [ParlayStatistics]
SET
    [ItemCount] = [ItemCount] + 1,
    [Size] = [Size] + @Size;

SELECT *
FROM [ParlayStatistics];";

            DateTime now = DateTime.UtcNow;

            CacheItem item = new CacheItem()
            {
                ExpireDate = expires,
                FirstAccessDate = now,
                Key = key.ToUpperInvariant(),
                LastAccessDate = now,
                Size = content.Length
            };

            using (IDbConnection connection = this.CreateAndOpenConnection())
            {
                using (IDbTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        lock (this.syncRoot)
                        {
                            this.RemoveContent(item.Key, connection, transaction);

                            CacheStatistics stats = connection.Query<CacheStatistics>(Sql, item, transaction).First();
                            this.StoreContent(item.Key, content);

                            this.itemCount = stats.ItemCount;
                            this.size = stats.Size;

                            if (this.maxSize > 0)
                            {
                                this.EvictToSize(this.maxSize, connection, transaction);
                            }

                            transaction.Commit();
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Creates an opens a <see cref="SQLiteConnection"/> to use for accessing cache information.
        /// </summary>
        /// <returns>A new <see cref="SQLiteConnection"/>.</returns>
        protected abstract SQLiteConnection CreateAndOpenConnection();

        /// <summary>
        /// Deletes the stored content identified by the given key.
        /// </summary>
        /// <param name="key">The key identifying the content to delete.</param>
        protected abstract void DeleteStoredContent(string key);

        /// <summary>
        /// Gets the stored content for the given key
        /// </summary>
        /// <param name="key">The key identifying the stored content to get.</param>
        /// <returns>The stored content for the given key.</returns>
        protected abstract byte[] GetStoredContent(string key);

        /// <summary>
        /// Removes an item from the cache.
        /// </summary>
        /// <param name="key">The item's network identifier.</param>
        /// <param name="connection">The <see cref="IDbConnection"/> to use.</param>
        /// <param name="transaction">The <see cref="IDbTransaction"/> to use.</param>
        protected void RemoveContent(string key, IDbConnection connection, IDbTransaction transaction)
        {
            CacheItem item = this.GetItem(key, null);

            if (item != null)
            {
                CacheStatistics stats = connection.Query<CacheStatistics>(
                        SqliteCache.RemoveSql,
                        new { Key = item.Key, Size = item.Size },
                        transaction).First();

                this.DeleteStoredContent(item.Key);

                this.itemCount = stats.ItemCount;
                this.size = stats.Size;
            }
        }

        /// <summary>
        /// Stores content identified by the given key in the cache.
        /// </summary>
        /// <param name="key">The key identifying the content to store.</param>
        /// <param name="content">The content to store.</param>
        protected abstract void StoreContent(string key, byte[] content);

        /// <summary>
        /// Disposes of resources used by this instance.
        /// </summary>
        /// <param name="disposing">A value indicating whether to dispose of managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Ensures that this instance's statistics have been initialized.
        /// </summary>
        protected void EnsureStatistics()
        {
            const string Sql =
@"SELECT *
FROM [ParlayStatistics];";

            if (this.itemCount == null || this.size == null)
            {
                lock (this.syncRoot)
                {
                    if (this.itemCount == null || this.size == null)
                    {
                        using (IDbConnection connection = this.CreateAndOpenConnection())
                        {
                            CacheStatistics stats = connection.Query<CacheStatistics>(Sql).First();
                            this.itemCount = stats.ItemCount;
                            this.size = stats.Size;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Evicts all expired items from the cache.
        /// </summary>
        /// <param name="connection">The <see cref="IDbConnection"/> to use.</param>
        /// <param name="transaction">The <see cref="IDbTransaction"/> to use.</param>
        protected void EvictExpired(IDbConnection connection, IDbTransaction transaction)
        {
            const string Sql =
@"SELECT
    [Key],
    [Size]
FROM [ParlayItem]
WHERE
    [ExpireDate] IS NOT NULL
    AND [ExpireDate] < @Now
LIMIT 100;";

            CacheStatistics stats = null;

            while (true)
            {
                CacheItem[] items = connection.Query<CacheItem>(Sql, new { Now = DateTime.UtcNow }, transaction).ToArray();

                foreach (CacheItem item in items)
                {
                    stats = connection.Query<CacheStatistics>(
                        SqliteCache.RemoveSql,
                        new { Key = item.Key, Size = item.Size },
                        transaction).First();

                    this.DeleteStoredContent(item.Key);
                }

                if (items.Length == 0)
                {
                    break;
                }
            }

            if (stats != null)
            {
                this.itemCount = stats.ItemCount;
                this.size = stats.Size;
            }
        }

        /// <summary>
        /// Evicts items from the cache until the total cache size is smaller
        /// than or equal to the given maximum size, in bytes.
        /// </summary>
        /// <param name="maxSize">The maximum size of the cache, in bytes.</param>
        /// <param name="connection">The <see cref="IDbConnection"/> to use.</param>
        /// <param name="transaction">The <see cref="IDbTransaction"/> to use.</param>
        protected void EvictToSize(long maxSize, IDbConnection connection, IDbTransaction transaction)
        {
            const string Sql =
@"SELECT
    [Key],
    [Size]
FROM [ParlayItem]
ORDER BY [LastAccessDate]
LIMIT 100;";

            if (maxSize < 0)
            {
                maxSize = 0;
            }

            if (this.Size > maxSize)
            {
                CacheStatistics stats = null;

                while (true)
                {
                    foreach (CacheItem item in connection.Query<CacheItem>(Sql, transaction))
                    {
                        stats = connection.Query<CacheStatistics>(
                            SqliteCache.RemoveSql,
                            new { Key = item.Key, Size = item.Size },
                            transaction).First();

                        this.DeleteStoredContent(item.Key);

                        if (stats.Size <= maxSize)
                        {
                            break;
                        }
                    }

                    if (stats == null || stats.Size <= maxSize)
                    {
                        break;
                    }
                }

                if (stats != null)
                {
                    this.itemCount = stats.ItemCount;
                    this.size = stats.Size;
                }
            }
        }

        /// <summary>
        /// Gets the item with the given key from the cache.
        /// Returns null if the item is not found.
        /// </summary>
        /// <param name="key">The key of the item to get.</param>
        /// <param name="transaction">The transaction to use, if applicable.</param>
        /// <returns>A cache item, or null if none is found.</returns>
        protected CacheItem GetItem(string key, IDbTransaction transaction)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key", "key must contain a value.");
            }

            const string Sql =
@"SELECT *
FROM [ParlayItem]
WHERE
    [Key] = @Key;";

            using (IDbConnection connection = this.CreateAndOpenConnection())
            {
                return connection.Query<CacheItem>(
                    Sql,
                    new { Key = key.ToUpperInvariant() },
                    transaction).FirstOrDefault();
            }
        }
    }
}