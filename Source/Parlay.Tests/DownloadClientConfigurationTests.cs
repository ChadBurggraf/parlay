namespace Parlay.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public sealed class DownloadClientConfigurationTests
    {
        [Test]
        public void Defaults()
        {
            DownloadClientConfiguration config = new DownloadClientConfiguration();
            Assert.IsFalse(string.IsNullOrEmpty(config.UserAgent));
        }
    }
}