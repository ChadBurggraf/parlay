namespace Parlay.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using NUnit.Framework;

    [TestFixture]
    public abstract class CacheTests : IDisposable
    {
        private ICache cache;
        private bool disposed;

        protected CacheTests(ICache cache)
        {
            if (cache == null)
            {
                throw new ArgumentNullException("cache", "cache cannot be null.");
            }

            this.cache = cache;
        }

        ~CacheTests()
        {
            this.Dispose(false);
        }

        protected ICache Cache
        {
            get { return this.cache; }
        }

        public virtual void Add()
        {
            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Parlay.Tests.Content.Domo.png"))
            {
                this.Cache.Add("http://example.com/Domo.png", stream);
            }

            Assert.AreEqual(1, this.Cache.ItemCount);
            Assert.AreEqual(6233, this.Cache.Size);

            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Parlay.Tests.Content.LetGo.png"))
            {
                this.Cache.Add("http://example.com/LetGo.png", stream);
            }

            Assert.AreEqual(2, this.Cache.ItemCount);
            Assert.AreEqual(11400, this.Cache.Size);

            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Parlay.Tests.Content.Domo.png"))
            {
                this.Cache.Add("http://example.com/Domo.png", stream);
            }

            Assert.AreEqual(2, this.Cache.ItemCount);
            Assert.AreEqual(11400, this.Cache.Size);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void EvictToSize()
        {
            this.PopulateCache();
            Assert.IsTrue(10000 < this.Cache.Size);
            this.Cache.EvictToSize(10000);
            Assert.IsTrue(10000 >= this.Cache.Size);
        }

        public virtual void Get()
        {
            this.PopulateCache();

            using (Stream stream = this.Cache.Get("http://example.com/Domo.png"))
            {
                Assert.IsNotNull(stream);
                Assert.AreEqual(6233, stream.Length);
            }

            Assert.IsNull(this.Cache.Get("http://example.com/NotAValidKey.png"));
        }

        public virtual void MultipleThreads()
        {
            this.PopulateCache();
            this.Cache.Remove("http://example.com/Traindead.png");

            ManualResetEvent one = new ManualResetEvent(false);
            ManualResetEvent two = new ManualResetEvent(false);

            new Thread(
                () =>
                {
                    using (Stream stream = this.Cache.Get("http://example.com/Domo.png"))
                    {
                        Assert.IsNotNull(stream);
                    }

                    using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Parlay.Tests.Content.Traindead.png"))
                    {
                        this.Cache.Add("http://example.com/Traindead.png", stream);
                    }

                    this.Cache.Remove("http://example.com/LetGo.png");
                    one.Set();
                }).Start();

            new Thread(
                () =>
                {
                    using (Stream stream = this.Cache.Get("http://example.com/Domo.png"))
                    {
                        Assert.IsNotNull(stream);
                    }

                    WaitHandle.WaitAll(new[] { one });

                    using (Stream stream = this.Cache.Get("http://example.com/Traindead.png"))
                    {
                        Assert.IsNotNull(stream);
                    }

                    Assert.IsNull(this.Cache.Get("http://example.com/LetGo.png"));
                    two.Set();
                }).Start();

            WaitHandle.WaitAll(new[] { two });
        }

        public virtual void Teardown()
        {
            this.Cache.EvictToSize(0);
        }

        public virtual void Remove()
        {
            this.PopulateCache();

            using (Stream stream = this.Cache.Get("http://example.com/Domo.png"))
            {
                Assert.IsNotNull(stream);
            }

            this.Cache.Remove("http://example.com/Domo.png");
            Assert.IsNull(this.Cache.Get("http://example.com/Domo.png"));
        }

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

        private void PopulateCache()
        {
            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Parlay.Tests.Content.Domo.png"))
            {
                this.Cache.Add("http://example.com/Domo.png", stream);
            }

            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Parlay.Tests.Content.LetGo.png"))
            {
                this.Cache.Add("http://example.com/LetGo.png", stream);
            }

            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Parlay.Tests.Content.Traindead.png"))
            {
                this.Cache.Add("http://example.com/Traindead.png", stream);
            }
        }
    }
}