namespace Parlay.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using NUnit.Framework;

    [TestFixture]
    public sealed class DownloadClientConfigurationTests
    {
        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test method.")]
        public void Defaults()
        {
            DownloadClientConfiguration config = new DownloadClientConfiguration();
            Assert.IsFalse(string.IsNullOrEmpty(config.UserAgent));
        }
    }
}