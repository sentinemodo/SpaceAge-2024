using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Core;

namespace UnitTests
{
    [TestFixture]
    public class TTests
    {

        public TTests()
        {
        }

        [Test]
        public void SetupTeardown()
        {
            Assert.IsTrue(true);
        }
    }
}
