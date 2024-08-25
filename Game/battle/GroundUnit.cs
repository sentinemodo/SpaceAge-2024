using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class GroundUnit : Unit
	{
		private Point2D coordinates;
		public Point2D Coordinates
		{
			get { return this.coordinates; }
			set { this.coordinates = value; }
		}

		public GroundUnit(ModuleStack moduleStack, double x, double y) : base(moduleStack)
		{
			this.coordinates.X = x;
			this.coordinates.Y = y;
		}
	}
}
