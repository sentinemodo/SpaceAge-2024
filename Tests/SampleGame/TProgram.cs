using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace IntegrationTests
{
	[TestFixture]
	public class TProgram
	{


		public TProgram()
		{
		}

		[Test]
		public void SetupTeardown()
		{
			ClassicAssert.IsTrue(true);
		}
	}
}
