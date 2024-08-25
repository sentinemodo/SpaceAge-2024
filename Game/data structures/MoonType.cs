using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class MoonType : NamedObject
	{
		public MoonType(string name) : base(name) { }

		private string color;
		public string Color
		{
			get { return this.color; }
			set { this.color = value; }
		}
	}
}
