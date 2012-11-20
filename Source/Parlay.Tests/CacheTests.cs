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

                return this.cache ?? (this.cache = this.CreateCache());
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void AddContent()
        {
            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Parlay.Tests.Content.Domo.png"))
            {
                this.Cache.AddContent("http://example.com/Domo.png", stream);
            }

            Assert.AreEqual(1, this.Cache.ItemCount);
            Assert.AreEqual(6233, this.Cache.Size);

            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Parlay.Tests.Content.LetGo.png"))
            {
                this.Cache.AddContent("http://example.com/LetGo.png", stream);
            }

            Assert.AreEqual(2, this.Cache.ItemCount);
            Assert.AreEqual(11400, this.Cache.Size);

            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Parlay.Tests.Content.Domo.png"))
            {
                this.Cache.AddContent("http://example.com/Domo.png", stream);
            }

            Assert.AreEqual(2, this.Cache.ItemCount);
            Assert.AreEqual(11400, this.Cache.Size);
        }

        protected abstract ICache CreateCache();

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
                this.Cache.AddContent("http://example.com/Domo.png", stream, DateTime.UtcNow.AddSeconds(1));
            }

            using (Stream stream = this.Cache.GetContent("http://example.com/Domo.png"))
            {
                Assert.IsNotNull(stream);
            }

            Thread.Sleep(1000);

            using (Stream stream = this.Cache.GetContent("http://example.com/Domo.png"))
            {
                Assert.IsNull(stream);
            }

            Assert.AreEqual(2, this.Cache.ItemCount);
        }

        protected void GetContent()
        {
            this.PopulateCache();

            using (Stream stream = this.Cache.GetContent("http://example.com/Domo.png"))
            {
                Assert.IsNotNull(stream);
                Assert.AreEqual(6233, stream.Length);
            }

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
                    using (Stream stream = this.Cache.GetContent("http://example.com/Domo.png"))
                    {
                        Assert.IsNotNull(stream);
                    }

                    using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Parlay.Tests.Content.Traindead.png"))
                    {
                        this.Cache.AddContent("http://example.com/Traindead.png", stream);
                    }

                    this.Cache.RemoveContent("http://example.com/LetGo.png");
                    one.Set();
                }).Start();

            new Thread(
                () =>
                {
                    using (Stream stream = this.Cache.GetContent("http://example.com/Domo.png"))
                    {
                        Assert.IsNotNull(stream);
                    }

                    WaitHandle.WaitAll(new[] { one });

                    using (Stream stream = this.Cache.GetContent("http://example.com/Traindead.png"))
                    {
                        Assert.IsNotNull(stream);
                    }

                    Assert.IsNull(this.Cache.GetContent("http://example.com/LetGo.png"));
                    two.Set();
                }).Start();

            WaitHandle.WaitAll(new[] { two });
        }

        protected void Teardown()
        {
            this.Cache.EvictToSize(0);
        }

        protected void RemoveContent()
        {
            this.PopulateCache();

            using (Stream stream = this.Cache.GetContent("http://example.com/Domo.png"))
            {
                Assert.IsNotNull(stream);
            }

            this.Cache.RemoveContent("http://example.com/Domo.png");
            Assert.IsNull(this.Cache.GetContent("http://example.com/Domo.png"));
        }

        private void PopulateCache()
        {
            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Parlay.Tests.Content.Domo.png"))
            {
                this.Cache.AddContent("http://example.com/Domo.png", stream);
            }

            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Parlay.Tests.Content.LetGo.png"))
            {
                this.Cache.AddContent("http://example.com/LetGo.png", stream);
            }

            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Parlay.Tests.Content.Traindead.png"))
            {
                this.Cache.AddContent("http://example.com/Traindead.png", stream);
            }
        }
    }
}