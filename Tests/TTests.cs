using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Core;
using NUnit.Framework.Legacy;

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
            ClassicAssert.IsTrue(true);
        }
    }
}
