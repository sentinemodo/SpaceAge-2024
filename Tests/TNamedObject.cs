using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TNamedObject
	{
		class MockNamedObject : NamedObject
		{
			public MockNamedObject(string name)
				: base(name)
			{
			}
		}

		private MockNamedObject named;

		public TNamedObject()
		{
		}

		[SetUp]
		public void setupDataFile()
		{
			this.named = new MockNamedObject("testName");
		}

		[Test]
		public void SetupTeardown()
		{
            Assert.That(true);
		}

		[Test]
		public void RandomId()
		{
			string name1 = this.named.GenerateRandomIdentifier();
			string name2 = this.named.GenerateRandomIdentifier();
            Assert.That(name1, Is.Not.EqualTo(name2));
		}

    }
}
