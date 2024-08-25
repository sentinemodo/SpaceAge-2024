using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Point2D
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

		public double Radius()
		{
			return Math.Sqrt(Math.Abs(this.x) + Math.Abs(this.y));
		}


		public double Distance(Point2D point)
		{
			return Math.Sqrt((point.X - this.x) * (point.X - this.x) + (point.Y - this.y) * (point.Y - this.y));
		}

		public double RelativeX(Point2D point)
		{
			return point.X - this.x;
		}

		public double RelativeY(Point2D point)
		{
			return point.Y - this.y;
		}
	}
}
