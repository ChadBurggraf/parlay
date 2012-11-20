namespace Parlay.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public sealed class MemoryCacheTests : CacheTests
    {
        [Test]
        public void MemoryCacheAddContent()
        {
            this.AddContent();
        }

        [Test]
        public void MemoryCacheEvictToSize()
        {
            this.EvictToSize();
        }

        [Test]
        public void MemoryCacheExpire()
        {
            this.Expire();
        }

        [Test]
        public void MemoryCacheGetContent()
        {
            this.GetContent();
        }

        [Test]
        public void MemoryCacheMultipleThreads()
        {
            this.MultipleThreads();
        }

        [Test]
        public void MemoryCacheRemoveContent()
        {
            this.RemoveContent();
        }

        protected override ICache CreateCache()
        {
            return new MemoryCache();
        }
    }
}