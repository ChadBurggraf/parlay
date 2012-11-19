namespace Parlay.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public sealed class MemoryCacheTests : CacheTests
    {
        public MemoryCacheTests()
            : base(new MemoryCache())
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
    }
}