using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

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
			Assert.IsTrue(true);
		}

		[Test]
		public void RandomId()
		{
			Assert.AreNotEqual(this.named.GenerateRandomIdentifier(), this.named.GenerateRandomIdentifier());
		}

	}
}
