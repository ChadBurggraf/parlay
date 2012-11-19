namespace Parlay.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public sealed class MemoryCacheTests : CacheTests
    {
        protected override ICache CreateCache()
        {
            return new MemoryCache();
        }
    }
}