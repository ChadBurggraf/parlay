namespace Parlay.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using NUnit.Framework;

    [TestFixture]
    public sealed class DownloadOptionsTests
    {
        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test method.")]
        public void DownloadOptionsnDefaults()
        {
            DownloadOptions options = new DownloadOptions();
            Assert.IsFalse(string.IsNullOrEmpty(options.UserAgent));
        }
    }
}