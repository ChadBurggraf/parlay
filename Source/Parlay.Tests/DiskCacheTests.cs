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
        public void DiskCacheAddContentEvict()
        {
            this.AddContentEvict();
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

        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

        [TearDown]
        public override void Teardown()
        {
            base.Teardown();
        }

        [TestFixtureTearDown]
        public void TeardownFixture()
        {
            string path = ((DiskCache)this.Cache).LocalPath;

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        protected override ICache CreateCache(long maxSize)
        {
            return new DiskCache(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Replace(".", string.Empty)), maxSize);
        }

        protected override void Dispose(bool disposing)
        {
            this.TeardownFixture();
            base.Dispose(disposing);
        }
    }
}