namespace Parlay.Tests
{
    using System;
    using System.IO;
    using NUnit.Framework;

    [TestFixture]
    public sealed class DiskCacheTests : CacheTests
    {
        public DiskCacheTests()
            : base(new DiskCache(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Replace(".", string.Empty))))
        {
        }

        [Test]
        public override void Add()
        {
            base.Add();
        }

        [Test]
        public override void EvictToSize()
        {
            base.EvictToSize();
        }

        [Test]
        public override void Get()
        {
            base.Get();
        }

        [Test]
        public override void MultipleThreads()
        {
            base.MultipleThreads();
        }

        [Test]
        public override void Remove()
        {
            base.Remove();
        }

        [TearDown]
        public override void Teardown()
        {
            base.Teardown();
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

        protected override void Dispose(bool disposing)
        {
            this.TeardownFixture();
            base.Dispose(disposing);
        }
    }
}