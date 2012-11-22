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

    internal sealed class SqliteCache : ICache
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
        private ISqliteCacheStorage storage;
        private long maxSize;
        private long? itemCount, size;
        private bool disposed;

        internal SqliteCache(ISqliteCacheStorage storage, long maxSize)
        {
            if (storage == null)
            {
                throw new ArgumentNullException("storage", "storage cannot be null.");
            }

            this.storage = storage;
            this.maxSize = maxSize > 0 ? maxSize : 0;
        }

        ~SqliteCache()
        {
            this.Dispose(false);
        }

        public long ItemCount
        {
            get
            {
                this.EnsureStatistics();
                return this.itemCount.Value;
            }
        }

        public long Size
        {
            get
            {
                this.EnsureStatistics();
                return this.size.Value;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Performance.")]
        public static string GetSchema()
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

        public void AddContent(string key, byte[] content)
        {
            this.AddContentImpl(key, content, null);
        }

        public void AddContent(string key, byte[] content, DateTime expires)
        {
            this.AddContentImpl(key, content, expires);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void EvictToSize(long maxSize)
        {
            if (maxSize < 0)
            {
                maxSize = 0;
            }

            if (this.Size > maxSize)
            {
                using (IDbConnection connection = this.storage.CreateAndOpenConnection())
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

        public byte[] GetContent(string key)
        {
            byte[] content = null;
            CacheItem item = this.GetItem(key, null);

            if (item != null)
            {
                if (item.ExpireDate == null || item.ExpireDate > DateTime.UtcNow)
                {
                    content = this.storage.GetStoredContent(item.Key);

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

        public void RemoveContent(string key)
        {
            using (IDbConnection connection = this.storage.CreateAndOpenConnection())
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

        private void AddContentImpl(string key, byte[] content, DateTime? expires)
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

            using (IDbConnection connection = this.storage.CreateAndOpenConnection())
            {
                using (IDbTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        lock (this.syncRoot)
                        {
                            this.RemoveContent(item.Key, connection, transaction);

                            CacheStatistics stats = connection.Query<CacheStatistics>(Sql, item, transaction).First();
                            this.storage.StoreContent(item.Key, content);

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

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                }

                this.storage = null;
                this.disposed = true;
            }
        }

        private void EnsureStatistics()
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
                        using (IDbConnection connection = this.storage.CreateAndOpenConnection())
                        {
                            CacheStatistics stats = connection.Query<CacheStatistics>(Sql).First();
                            this.itemCount = stats.ItemCount;
                            this.size = stats.Size;
                        }
                    }
                }
            }
        }

        private void EvictExpired(IDbConnection connection, IDbTransaction transaction)
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

                    this.storage.DeleteStoredContent(item.Key);
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

        private void EvictToSize(long maxSize, IDbConnection connection, IDbTransaction transaction)
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

                        this.storage.DeleteStoredContent(item.Key);

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

        private CacheItem GetItem(string key, IDbTransaction transaction)
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

            using (IDbConnection connection = this.storage.CreateAndOpenConnection())
            {
                return connection.Query<CacheItem>(
                    Sql,
                    new { Key = key.ToUpperInvariant() },
                    transaction).FirstOrDefault();
            }
        }

        private void RemoveContent(string key, IDbConnection connection, IDbTransaction transaction)
        {
            CacheItem item = this.GetItem(key, null);

            if (item != null)
            {
                CacheStatistics stats = connection.Query<CacheStatistics>(
                        SqliteCache.RemoveSql,
                        new { Key = item.Key, Size = item.Size },
                        transaction).First();

                this.storage.DeleteStoredContent(item.Key);

                this.itemCount = stats.ItemCount;
                this.size = stats.Size;
            }
        }
    }
}