//-----------------------------------------------------------------------------
// <copyright file="ISqliteCacheStorage.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------------

namespace Parlay
{
    using System;
    using System.Data.SQLite;

    internal interface ISqliteCacheStorage
    {
        SQLiteConnection CreateAndOpenConnection();

        void DeleteStoredContent(string key);

        byte[] GetStoredContent(string key);

        void StoreContent(string key, byte[] content);
    }
}