using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Star : SpaceSystemObject, IOrbitHolder 
	{
		public Star(SpaceSystem spaceSystem, string name)
			: base(name)
		{
			while (string.IsNullOrEmpty(this.name) || Star.All.ContainsKey(this.name))
				this.name = this.GenerateRandomIdentifier("S", Star.StarNameLength);
			Star.All.Add(this.name, this);
			this.spaceSystem = spaceSystem;
		}

		public const int StarNameLength = 5;
		public static Stars All = new Stars();

		private StarType starType;
		public StarType StarType
		{
			get { return this.starType; }
			set { this.starType = value; }
		}

		private string color;
		public string Color
		{
			get { return this.color; }
			set { this.color = value; }
		}
	
		// Sun = 1
		private double mass = 0;
		public double Mass
		{
			get { return this.mass; }
			set { this.mass = value; }
		}

		// Sun = 1
		private double luminosity = 0;
		public double Luminosity
		{
			get { return this.luminosity; }
			set { this.luminosity = value; }
		}

		// Sun = 1
		private double radius = 0;
		public double Radius
		{
			get { return this.radius; }
			set { this.radius = value; }
		}

		// surface temperature
		private double temperature = 0;
		public double Temperature
		{
			get { return this.temperature; }
			set { this.temperature = value; }
		}

		public Orbit Orbit
		{
			get { return Orbit.All[this]; }
		}

		#region IReporting Members

		public override List<string> Report(Faction faction)
		{
			List<string> reportLines = new List<string>
            {
                "",
                "  * Sol [S00001] (0, 0, 0), M4 star, unexplored.",
                "------------------------------------------------------------"
            };

			return reportLines;
		}

		public override string BattleReportName
		{
			get
			{
				return "Sol [S00001] (0, 0, 0)";
			}
		}

		#endregion

		public override bool Visible(Faction faction)
		{
			return true;
		}
	}
}
