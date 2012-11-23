namespace Parlay.Tests
{
    using System;
    using System.IO;
    using System.Threading;
    using NUnit.Framework;

    [TestFixture]
    public sealed class DownloaderTests
    {
        [Test]
        public void DownloaderCancel()
        {
            using (ManualResetEvent handle = new ManualResetEvent(false))
            {
                using (Downloader downloader = new Downloader())
                {
                    DownloadResult result = downloader.Download(
                        new Uri("http://www.gravatar.com/avatar/aeb98450c2ecffb91d20afc296f70238.png"),
                        r =>
                        {
                            Assert.Fail();
                            handle.Set();
                        });

                    Assert.IsTrue(downloader.Cancel(result));
                    handle.WaitOne(1000);
                    Assert.AreEqual(0, downloader.ProcessingCount);
                    Assert.AreEqual(0, downloader.QueuedCount);
                }
            }
        }

        [Test]
        public void DownloaderDownload()
        {
            bool finished = false;

            using (ManualResetEvent handle = new ManualResetEvent(false))
            {
                using (Downloader downloader = new Downloader())
                {
                    downloader.Download(
                        new Uri("http://www.gravatar.com/avatar/aeb98450c2ecffb91d20afc296f70238.png"),
                        result =>
                        {
                            Assert.IsNotNull(result);
                            Assert.IsNull(result.Error);
                            Assert.IsTrue(result.IsComplete);
                            Assert.IsFalse(result.WasCached);
                            Assert.IsNotNull(result.Content);
                            finished = true;
                            handle.Set();
                        });

                    handle.WaitOne(10000);
                    Assert.IsTrue(finished);
                }
            }
        }

        [Test]
        public void DownloaderDownloadCached()
        {
            bool finished = false;

            using (ManualResetEvent handle = new ManualResetEvent(false))
            {
                using (Downloader client = new Downloader())
                {
                    client.Download(
                        new Uri("http://www.gravatar.com/avatar/aeb98450c2ecffb91d20afc296f70238.png"),
                        result =>
                        {
                            Assert.IsNotNull(result);
                            Assert.IsNull(result.Error);
                            Assert.IsTrue(result.IsComplete);
                            Assert.IsFalse(result.WasCached);
                            Assert.IsNotNull(result.Content);
                            finished = true;
                            handle.Set();
                        });

                    handle.WaitOne(10000);
                }

                Assert.IsTrue(finished);
                
                using (Downloader client = new Downloader())
                {
                    DownloadResult result = client.Download(new Uri("http://www.gravatar.com/avatar/aeb98450c2ecffb91d20afc296f70238.png"));
                    Assert.IsNotNull(result);
                    Assert.IsNull(result.Error);
                    Assert.IsTrue(result.IsComplete);
                    Assert.IsTrue(result.WasCached);
                    Assert.IsNotNull(result.Content);
                }
            }
        }

        [Test]
        public void DownloaderQueue()
        {
            using (Downloader downloader = new Downloader(2, CacheProfile.Memory()))
            {
                downloader.Download(new Uri("http://www.gravatar.com/avatar/aeb98450c2ecffb91d20afc296f70238.png?s=1024"));
                downloader.Download(new Uri("http://www.gravatar.com/avatar/aeb98450c2ecffb91d20afc296f70238.png?s=1025"));
                downloader.Download(new Uri("http://www.gravatar.com/avatar/aeb98450c2ecffb91d20afc296f70238.png?s=1026"));
                downloader.Download(new Uri("http://www.gravatar.com/avatar/aeb98450c2ecffb91d20afc296f70238.png?s=1027"));
                Assert.AreEqual(2, downloader.ProcessingCount);
                Assert.AreEqual(2, downloader.QueuedCount);
            }
        }

        [SetUp]
        public void Setup()
        {
            using (MemoryCache cache = new MemoryCache())
            {
                cache.EvictToSize(0);
            }
        }
    }
}