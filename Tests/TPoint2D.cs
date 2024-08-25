using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TPoint2D
	{
		public Point2D point;

		public TPoint2D()
		{
		}

		[SetUp]
		public void setupDataFile()
		{
			this.point = new Point2D();
		}

		[Test]
		public void SetupTeardown()
		{
			ClassicAssert.IsTrue(true);
		}



		[Test]
		public void Distance()
		{
			ClassicAssert.AreEqual(0, point.Distance(point));

			this.point.X = 1;
			
			Point2D point2 = new Point2D();
			ClassicAssert.AreEqual(1, point.Distance(point2));

			this.point.Y = 1;
			ClassicAssert.AreEqual(Math.Sqrt(2), point.Distance(point2));		
		}	 
	}
}
