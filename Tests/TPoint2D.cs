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
            Assert.That(true);
		}



		[Test]
		public void Distance()
		{
            Assert.That(point.Distance(point), Is.EqualTo(0));

			this.point.X = 1;
			
			Point2D point2 = new Point2D();
            Assert.That(point.Distance(point2), Is.EqualTo(1));

			this.point.Y = 1;
            Assert.That(point.Distance(point2), Is.EqualTo(Math.Sqrt(2)));		
		}	 
	}
}
