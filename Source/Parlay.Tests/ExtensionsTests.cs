﻿namespace Parlay.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public sealed class ExtensionsTests
    {
        [Test]
        public void ExtensionsHash()
        {
            Assert.AreEqual("943a702d06f34599aee1f8da8ef9f7296031d699", "Hello, world!".Hash());
        }
    }
}