using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Planet : SpaceSystemObject, IOrbitHolder, IRegionHolder
	{
		public Planet(SpaceSystem spaceSystem, string name)
			: base(name)
		{
			while (string.IsNullOrEmpty(this.name) || Planet.All.ContainsKey(this.name))
				this.name = this.GenerateRandomIdentifier("P", Planet.PlanetNameLength);
			Planet.All.Add(this.name, this);
			this.spaceSystem = spaceSystem; 
		}

		public const int PlanetNameLength = 5;
		public static Planets All = new Planets();

		private PlanetType planetType;
		public PlanetType PlanetType
		{
			get { return this.planetType; }
			set { this.planetType = value; }
		}

		// Earth = 1
		private double mass = 0;
		public double Mass
		{
			get { return this.mass; }
			set { this.mass = value; }
		}

		// Earth = 1
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

		private int surfaceSizeX = 0;
		public int SurfaceSizeX
		{
			get { return this.surfaceSizeX; }
			set { this.surfaceSizeX = value; }
		}

		private int surfaceSizeY = 0;
		public int SurfaceSizeY
		{
			get { return this.surfaceSizeY; }
			set { this.surfaceSizeY = value; }
		}

		public Regions Regions
		{
			get { return Region.All[this]; }
		}

		public Orbit Orbit
		{
			get { return Orbit.All[this]; }
		}

		public Moons Moons
		{
			get { return Moon.All[this]; }
		}

		#region IReporting Members

		public override string ReportName
		{
			get
			{
				return string.Format("{0} [{1}] at AU {2}, {3}", 
					this.FullName, 
					this.Name,
					this.au, 
					this.PlanetType.ReportName);
			}
		}

		public override string BattleReportName
		{
			get
			{
				return string.Format("{0} in {1}", this.ReportName, this.spaceSystem.ReportName);
			}
		}

		public override List<string> Report(Faction faction)
		{
			List<string> reportLines = new List<string>();

			reportLines.AddRange(this.reportHeader(faction));
			reportLines.AddRange(this.reportOrbit(faction));
			reportLines.AddRange(this.reportRegions(faction));
			reportLines.AddRange(this.reportMoons(faction));
			return reportLines;
		}

		private List<string> reportHeader(Faction faction)
		{
			List<string> lines = new List<string>
            {
                "",
                string.Format("  * {0}.", this.ReportName),
                "------------------------------------------------------------"
            };
			return lines;
		}

		private List<string> reportMoons(Faction faction)
		{
			List<string> lines = new List<string>();
			foreach (Moon moon in this.Moons.Values)
			{
				if (moon.Visible(faction))
				{
					lines.Add("------------------------------------------------------------");
					lines.AddRange(moon.Report(faction));
				}
			}

			return lines;
		}

		private List<string> reportOrbit(Faction faction)
		{
			List<string> lines = new List<string>();
			if (this.Orbit != null)
			{
				lines.AddRange(this.Orbit.Report(faction, 1));
			}
			return lines;
		}

		private List<string> reportRegions(Faction faction)
		{
			List<string> lines = new List<string>();
			foreach (Region region in this.Regions.Values)
			{
				if (region.Visible(faction))
				{
					lines.Add("");
					lines.AddRange(region.Report(faction, 1));
				}
			}

			return lines;
		}

		#endregion

		public override bool Visible(Faction faction)
		{
			return true;
		}
	}
}


