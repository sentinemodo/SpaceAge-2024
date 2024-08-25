using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Point3D
	{
		private double x = 0;
		public double X
		{
			get { return this.x; }
			set { this.x = value; }
		}

		private double y = 0;
		public double Y
		{
			get { return this.y; }
			set { this.y = value; }
		}

		private double z = 0;
		public double Z
		{
			get { return this.z; }
			set { this.z = value; }
		}

		public double Radius()
		{
			return Math.Sqrt(Math.Abs(this.x) + Math.Abs(this.y) + Math.Abs(this.z));
		}

		public double Distance(Point3D point)
		{
			return Math.Sqrt((point.X - this.x) * (point.X - this.x) + (point.Y - this.y) * (point.Y - this.y) + (point.Z - this.z) * (point.Z - this.z));
		}

		public double RelativeX(Point3D point)
		{
			return point.X - this.x;
		}

		public double RelativeY(Point3D point)
		{
			return point.Y - this.y;
		}

		public double RelativeZ(Point3D point)
		{
			return point.Z - this.z;
		}

	}
}
