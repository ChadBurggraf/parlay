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
    using System.Globalization;
    using System.IO;

    internal sealed class SqliteCache : ICache
    {
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
                using (SQLiteConnection connection = this.storage.CreateAndOpenConnection())
                {
                    using (SQLiteTransaction transaction = connection.BeginTransaction())
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
            CacheItem item;

            using (SQLiteConnection connection = this.storage.CreateAndOpenConnection())
            {
                item = SqliteCache.GetItem(key, connection, null);
            }

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
            using (SQLiteConnection connection = this.storage.CreateAndOpenConnection())
            {
                using (SQLiteTransaction transaction = connection.BeginTransaction())
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

        private static CacheStatistics DeleteItem(string key, long size, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            const string Sql =
@"UPDATE [ParlayStatistics]
SET
    [ItemCount] = [ItemCount] - 1,
    [Size] = [Size] - @Size;

DELETE FROM [ParlayItem]
WHERE
    [Key] = @Key;

SELECT *
FROM [ParlayStatistics];";

            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = Sql;
                command.CommandType = CommandType.Text;
                command.Transaction = transaction;
                command.Parameters.AddWithValue("@Key", key);
                command.Parameters.AddWithValue("@Size", size);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    reader.Read();

                    return new CacheStatistics()
                    {
                        ItemCount = Convert.ToInt64(reader["ItemCount"], CultureInfo.InvariantCulture),
                        Size = Convert.ToInt64(reader["Size"], CultureInfo.InvariantCulture)
                    };
                }
            }
        }

        private static CacheItem[] GetEvictionItems(int limit, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            const string Sql =
@"SELECT
    [Key],
    [Size]
FROM [ParlayItem]
ORDER BY [LastAccessDate]
LIMIT @Limit;";

            List<CacheItem> items = new List<CacheItem>();

            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = Sql;
                command.CommandType = CommandType.Text;
                command.Transaction = transaction;
                command.Parameters.AddWithValue("@Limit", limit);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(
                            new CacheItem()
                            {
                                Key = Convert.ToString(reader["Key"], CultureInfo.InvariantCulture),
                                Size = Convert.ToInt64(reader["Size"], CultureInfo.InvariantCulture)
                            });
                    }
                }
            }

            return items.ToArray();
        }

        private static CacheItem[] GetExpiredEvictionItems(int limit, SQLiteConnection connection, SQLiteTransaction transaction)
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

            List<CacheItem> items = new List<CacheItem>();

            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = Sql;
                command.CommandType = CommandType.Text;
                command.Transaction = transaction;
                command.Parameters.AddWithValue("@Limit", limit);
                command.Parameters.AddWithValue("@Now", DateTime.UtcNow);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(
                            new CacheItem()
                            {
                                Key = Convert.ToString(reader["Key"], CultureInfo.InvariantCulture),
                                Size = Convert.ToInt64(reader["Size"], CultureInfo.InvariantCulture)
                            });
                    }
                }
            }

            return items.ToArray();
        }

        private static CacheItem GetItem(string key, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            const string Sql =
@"SELECT *
FROM [ParlayItem]
WHERE
    [Key] = @Key;";

            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = Sql;
                command.Transaction = transaction;
                command.Parameters.AddWithValue("@Key", key.ToUpperInvariant());

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new CacheItem()
                        {
                            ExpireDate = reader["ExpireDate"] != DBNull.Value ? (DateTime?)reader["ExpireDate"] : null,
                            FirstAccessDate = (DateTime)reader["FirstAccessDate"],
                            Key = Convert.ToString(reader["Key"], CultureInfo.InvariantCulture),
                            LastAccessDate = (DateTime)reader["LastAccessDate"],
                            Size = Convert.ToInt64(reader["Size"], CultureInfo.InvariantCulture)
                        };
                    }
                }
            }

            return null;
        }

        private static CacheStatistics GetStatistics(SQLiteConnection connection, SQLiteTransaction transaction)
        {
            const string Sql =
@"SELECT *
FROM [ParlayStatistics];";

            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = Sql;
                command.CommandType = CommandType.Text;
                command.Transaction = transaction;

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    reader.Read();

                    return new CacheStatistics()
                    {
                        ItemCount = Convert.ToInt64(reader["ItemCount"], CultureInfo.InvariantCulture),
                        Size = Convert.ToInt64(reader["Size"], CultureInfo.InvariantCulture)
                    };
                }
            }
        }

        private static CacheStatistics InsertItem(CacheItem item, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            const string Sql =
@"INSERT INTO [ParlayItem]([ExpireDate],[FirstAccessDate],[Key],[LastAccessDate],[Size])
VALUES(@ExpireDate,@FirstAccessDate,@Key,@LastAccessDate,@Size);

UPDATE [ParlayStatistics]
SET
    [ItemCount] = [ItemCount] + 1,
    [Size] = [Size] + @Size;

SELECT *
FROM [ParlayStatistics];";

            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = Sql;
                command.Transaction = transaction;
                command.Parameters.AddWithValue("@ExpireDate", item.ExpireDate);
                command.Parameters.AddWithValue("@FirstAccessDate", item.FirstAccessDate);
                command.Parameters.AddWithValue("@Key", item.Key);
                command.Parameters.AddWithValue("@LastAccessDate", item.LastAccessDate);
                command.Parameters.AddWithValue("@Size", item.Size);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    reader.Read();

                    return new CacheStatistics()
                    {
                        ItemCount = Convert.ToInt64(reader["ItemCount"], CultureInfo.InvariantCulture),
                        Size = Convert.ToInt64(reader["Size"], CultureInfo.InvariantCulture)
                    };
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

            DateTime now = DateTime.UtcNow;

            CacheItem item = new CacheItem()
            {
                ExpireDate = expires,
                FirstAccessDate = now,
                Key = key.ToUpperInvariant(),
                LastAccessDate = now,
                Size = content.Length
            };

            using (SQLiteConnection connection = this.storage.CreateAndOpenConnection())
            {
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        lock (this.syncRoot)
                        {
                            this.RemoveContent(item.Key, connection, transaction);
                            CacheStatistics stats = SqliteCache.InsertItem(item, connection, transaction);
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
            if (this.itemCount == null || this.size == null)
            {
                lock (this.syncRoot)
                {
                    if (this.itemCount == null || this.size == null)
                    {
                        using (SQLiteConnection connection = this.storage.CreateAndOpenConnection())
                        {
                            CacheStatistics stats = SqliteCache.GetStatistics(connection, null);
                            this.itemCount = stats.ItemCount;
                            this.size = stats.Size;
                        }
                    }
                }
            }
        }

        private void EvictExpired(SQLiteConnection connection, SQLiteTransaction transaction)
        {
            CacheStatistics stats = null;

            while (true)
            {
                CacheItem[] items = SqliteCache.GetExpiredEvictionItems(100, connection, transaction);

                foreach (CacheItem item in items)
                {
                    stats = SqliteCache.DeleteItem(item.Key, item.Size, connection, transaction);
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

        private void EvictToSize(long maxSize, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            if (maxSize < 0)
            {
                maxSize = 0;
            }

            if (this.Size > maxSize)
            {
                CacheStatistics stats = null;

                while (true)
                {
                    foreach (CacheItem item in SqliteCache.GetEvictionItems(100, connection, transaction))
                    {
                        stats = SqliteCache.DeleteItem(item.Key, item.Size, connection, transaction);
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

        private void RemoveContent(string key, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            CacheItem item = SqliteCache.GetItem(key, connection, transaction);

            if (item != null)
            {
                CacheStatistics stats = SqliteCache.DeleteItem(item.Key, item.Size, connection, transaction);
                this.storage.DeleteStoredContent(item.Key);

                this.itemCount = stats.ItemCount;
                this.size = stats.Size;
            }
        }
    }
}