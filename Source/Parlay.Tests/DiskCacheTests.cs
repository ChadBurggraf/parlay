namespace Parlay.Tests
{
    using System;
    using System.IO;
    using NUnit.Framework;

    [TestFixture]
    public sealed class DiskCacheTests : CacheTests
    {
        [Test]
        public void DiskCacheAddContent()
        {
            this.AddContent();
        }

        [Test]
        public void DiskCacheEvictToSize()
        {
            this.EvictToSize();
        }

        [Test]
        public void DiskCacheExpire()
        {
            this.Expire();
        }

        [Test]
        public void DiskCacheGetContent()
        {
            this.GetContent();
        }

        [Test]
        public void DiskCacheMultipleThreads()
        {
            this.MultipleThreads();
        }

        [Test]
        public void DiskCacheRemoveContent()
        {
            this.RemoveContent();
        }

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