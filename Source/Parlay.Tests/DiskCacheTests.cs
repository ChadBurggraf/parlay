namespace Parlay.Tests
{
    using System;
    using System.IO;
    using NUnit.Framework;

    [TestFixture]
    public sealed class DiskCacheTests : CacheTests
    {
        [TestFixtureTearDown]
        public void TeardownFixture()
        {
            string path = ((DiskCache)this.Cache).Path;

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        protected override ICache CreateCache()
        {
            return new DiskCache(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Replace(".", string.Empty)));
        }

        protected override void Dispose(bool disposing)
        {
            this.TeardownFixture();
            base.Dispose(disposing);
        }
    }
}