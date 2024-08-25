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
			ClassicAssert.IsTrue(true);
		}

		[Test]
		public void RandomId()
		{
			ClassicAssert.AreNotEqual(this.named.GenerateRandomIdentifier(), this.named.GenerateRandomIdentifier());
		}

	}
}
