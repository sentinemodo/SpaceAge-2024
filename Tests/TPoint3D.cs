using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TPoint3D
	{
		public Point3D point;

		public TPoint3D()
		{
		}

		[SetUp]
		public void setupDataFile()
		{
			this.point = new Point3D();
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
			
			Point3D point2 = new Point3D();
			ClassicAssert.AreEqual(1, point.Distance(point2));

			this.point.Y = 1;
			ClassicAssert.AreEqual(Math.Sqrt(2), point.Distance(point2));

			this.point.Z = 1;
			ClassicAssert.AreEqual(Math.Sqrt(3), point.Distance(point2));		

		}	 
	}
}
