namespace Parlay.Tests
{
    using System;
    using System.IO;
    using System.Threading;
    using NUnit.Framework;

    [TestFixture]
    public sealed class DownloadClientTests
    {
        [Test]
        public void DownloadClientDownload()
        {
            using (ManualResetEvent handle = new ManualResetEvent(false))
            {
                using (DownloadClient client = new DownloadClient())
                {
                    client.Download(
                        new Uri("http://www.gravatar.com/avatar/aeb98450c2ecffb91d20afc296f70238.png"),
                        result =>
                        {
                            Assert.IsNotNull(result);
                            Assert.IsTrue(result.IsComplete);
                            Assert.IsFalse(result.WasCached);
                            Assert.IsFalse(result.WasCancelled);

                            using (Stream content = result.GetContent())
                            {
                                Assert.IsNotNull(content);
                            }

                            handle.Set();
                        });
                }

                handle.WaitOne();
            }
        }

        [Test]
        public void DownloadClientDownloadCached()
        {
            using (ManualResetEvent handle = new ManualResetEvent(false))
            {
                using (DownloadClient client = new DownloadClient())
                {
                    client.Download(
                        new Uri("http://www.gravatar.com/avatar/aeb98450c2ecffb91d20afc296f70238.png"),
                        result =>
                        {
                            Assert.IsNotNull(result);
                            Assert.IsTrue(result.IsComplete);
                            Assert.IsFalse(result.WasCached);
                            Assert.IsFalse(result.WasCancelled);

                            using (Stream content = result.GetContent())
                            {
                                Assert.IsNotNull(content);
                            }

                            handle.Set();
                        });
                }

                handle.WaitOne();

                using (DownloadClient client = new DownloadClient())
                {
                    client.Download(
                        new Uri("http://www.gravatar.com/avatar/aeb98450c2ecffb91d20afc296f70238.png"),
                        result =>
                        {
                            Assert.IsNotNull(result);
                            Assert.IsTrue(result.IsComplete);
                            Assert.IsTrue(result.WasCached);
                            Assert.IsFalse(result.WasCancelled);

                            using (Stream content = result.GetContent())
                            {
                                Assert.IsNotNull(content);
                            }

                            handle.Set();
                        });
                }

                handle.WaitOne();
            }
        }
    }
}