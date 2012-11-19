//-----------------------------------------------------------------------------
// <copyright file="DiskCache.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;
    using System.Data.SQLite;
    using System.Globalization;
    using System.IO;
    using Dapper;

    /// <summary>
    /// Implements <see cref="ICache"/> with an on-disk cache database and content storage.
    /// </summary>
    public sealed class DiskCache : SqliteCache
    {
        private string path, databasePath, connectionString;

        /// <summary>
        /// Initializes a new instance of the DiskCache class.
        /// </summary>
        /// <param name="path">The path of the directory to store cached content in.</param>
        public DiskCache(string path)
            : this(path, 104857600)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DiskCache class.
        /// </summary>
        /// <param name="path">The path of the directory to store cached content in.</param>
        /// <param name="maxSize">The maximum size, in bytes, to allow the cache to grow to.</param>
        public DiskCache(string path, long maxSize)
            : base(maxSize)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path", "path must contain a value.");
            }

            this.path = path;
            this.databasePath = System.IO.Path.Combine(path, "Parlay.sqlite");
            this.connectionString = string.Format(CultureInfo.InvariantCulture, "Data Source={0};Journal Mode=Off;Synchronous=Off;Version=3", this.databasePath);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Gets the path of the directory this instance stores cached content in.
        /// </summary>
        internal string Path
        {
            get { return this.path; }
        }

        /// <summary>
        /// Creates an opens a <see cref="SQLiteConnection"/> to use for accessing cache information.
        /// </summary>
        /// <returns>A new <see cref="SQLiteConnection"/>.</returns>
        protected override SQLiteConnection CreateAndOpenConnection()
        {
            SQLiteConnection connection;

            if (!File.Exists(this.databasePath))
            {
                using (connection = new SQLiteConnection(this.connectionString))
                {
                    connection.Open();
                    connection.Execute(SqliteCache.GetSchema());
                }
            }

            connection = new SQLiteConnection(this.connectionString);
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

            string contentPath = System.IO.Path.Combine(this.path, key.Hash());

            if (File.Exists(contentPath))
            {
                File.Delete(contentPath);
            }
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

            string contentPath = System.IO.Path.Combine(this.path, key.Hash());

            if (File.Exists(contentPath))
            {
                return File.OpenRead(contentPath);
            }

            return null;
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

            string contentPath = System.IO.Path.Combine(this.path, key.Hash());

            using (FileStream stream = File.Create(contentPath))
            {
                content.CopyTo(stream);
            }
        }
    }
}