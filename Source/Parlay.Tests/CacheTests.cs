namespace Parlay.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using NUnit.Framework;

    public abstract class CacheTests : IDisposable
    {
        private ICache cache;
        private bool disposed;

        ~CacheTests()
        {
            this.Dispose(false);
        }

        protected ICache Cache
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }

                return this.cache ?? (this.cache = this.CreateCache(104857600));
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Setup()
        {
            this.Cache.EvictToSize(0);
        }

        public virtual void Teardown()
        {
            this.Cache.EvictToSize(0);
        }

        protected void AddContent()
        {
            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Parlay.Tests.Content.Domo.png"))
            {
                this.Cache.AddContent("http://example.com/Domo.png", stream.ReadAllBytes());
            }

            Assert.AreEqual(1, this.Cache.ItemCount);
            Assert.AreEqual(6233, this.Cache.Size);

            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Parlay.Tests.Content.LetGo.png"))
            {
                this.Cache.AddContent("http://example.com/LetGo.png", stream.ReadAllBytes());
            }

            Assert.AreEqual(2, this.Cache.ItemCount);
            Assert.AreEqual(11400, this.Cache.Size);

            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Parlay.Tests.Content.Domo.png"))
            {
                this.Cache.AddContent("http://example.com/Domo.png", stream.ReadAllBytes());
            }

            Assert.AreEqual(2, this.Cache.ItemCount);
            Assert.AreEqual(11400, this.Cache.Size);
        }

        protected void AddContentEvict()
        {
            using (ICache cache = this.CreateCache(12000))
            {
                CacheTests.PopulateCache(cache);
                Assert.AreEqual(2, cache.ItemCount);
                Assert.AreEqual(11057, cache.Size);
            }
        }

        protected abstract ICache CreateCache(long maxSize);

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.cache != null)
                    {
                        this.cache.Dispose();
                        this.cache = null;
                    }
                }

                this.disposed = true;
            }
        }

        protected void EvictToSize()
        {
            this.PopulateCache();
            Assert.IsTrue(10000 < this.Cache.Size);
            this.Cache.EvictToSize(10000);
            Assert.IsTrue(10000 >= this.Cache.Size);
        }

        protected void Expire()
        {
            this.PopulateCache();
            this.Cache.RemoveContent("http://example.com/Domo.png");

            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Parlay.Tests.Content.Domo.png"))
            {
                this.Cache.AddContent("http://example.com/Domo.png", stream.ReadAllBytes(), DateTime.UtcNow.AddSeconds(1));
            }

            Assert.IsNotNull(this.Cache.GetContent("http://example.com/Domo.png"));
            Thread.Sleep(1000);

            Assert.IsNull(this.Cache.GetContent("http://example.com/Domo.png"));
            Assert.AreEqual(2, this.Cache.ItemCount);
        }

        protected void GetContent()
        {
            this.PopulateCache();

            byte[] content = this.Cache.GetContent("http://example.com/Domo.png");
            Assert.IsNotNull(content);
            Assert.AreEqual(6233, content.Length);
            Assert.IsNull(this.Cache.GetContent("http://example.com/NotAValidKey.png"));
        }

        protected void MultipleThreads()
        {
            this.PopulateCache();
            this.Cache.RemoveContent("http://example.com/Traindead.png");

            ManualResetEvent one = new ManualResetEvent(false);
            ManualResetEvent two = new ManualResetEvent(false);

            new Thread(
                () =>
                {
                    Assert.IsNotNull(this.Cache.GetContent("http://example.com/Domo.png"));

                    using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Parlay.Tests.Content.Traindead.png"))
                    {
                        this.Cache.AddContent("http://example.com/Traindead.png", stream.ReadAllBytes());
                    }

                    this.Cache.RemoveContent("http://example.com/LetGo.png");
                    one.Set();
                }).Start();

            new Thread(
                () =>
                {
                    Assert.IsNotNull(this.Cache.GetContent("http://example.com/Domo.png"));
                    one.WaitOne();
                    Assert.IsNotNull(this.Cache.GetContent("http://example.com/Traindead.png"));
                    Assert.IsNull(this.Cache.GetContent("http://example.com/LetGo.png"));
                    two.Set();
                }).Start();

            WaitHandle.WaitAll(new[] { two });
        }

        protected void RemoveContent()
        {
            this.PopulateCache();
            Assert.IsNotNull(this.Cache.GetContent("http://example.com/Domo.png"));
            this.Cache.RemoveContent("http://example.com/Domo.png");
            Assert.IsNull(this.Cache.GetContent("http://example.com/Domo.png"));
        }

        private static void PopulateCache(ICache cache)
        {
            using (Stream stream = typeof(CacheTests).Assembly.GetManifestResourceStream("Parlay.Tests.Content.Domo.png"))
            {
                cache.AddContent("http://example.com/Domo.png", stream.ReadAllBytes());
            }

            using (Stream stream = typeof(CacheTests).Assembly.GetManifestResourceStream("Parlay.Tests.Content.LetGo.png"))
            {
                cache.AddContent("http://example.com/LetGo.png", stream.ReadAllBytes());
            }

            using (Stream stream = typeof(CacheTests).Assembly.GetManifestResourceStream("Parlay.Tests.Content.Traindead.png"))
            {
                cache.AddContent("http://example.com/Traindead.png", stream.ReadAllBytes());
            }
        }

        private void PopulateCache()
        {
            CacheTests.PopulateCache(this.Cache);
        }
    }
}