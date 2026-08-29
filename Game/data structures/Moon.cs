using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Moon : SpaceSystemObject, IOrbitHolder, IRegionHolder
	{
		public Moon(SpaceSystem spaceSystem, Planet planet, string name)
			: base(name)
		{		
			while (string.IsNullOrEmpty(this.name) || Moon.All.ContainsKey(this.name))
				this.name = this.GenerateRandomIdentifier("M", Moon.MoonNameLength);
			Moon.All.Add(this.name, this);
			this.planet = planet;
			this.spaceSystem = spaceSystem;
		}

		public const int MoonNameLength = 5;
		public static Moons All = new Moons();

		private Planet planet;
		public Planet Planet
		{
			get { return this.planet; }
			set { this.planet = value; }
		}

		private MoonType moonType;
		public MoonType MoonType
		{
			get { return this.moonType; }
			set { this.moonType = value; }
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

		// surface temperature (legacy unused kelvin leftover)
		private double temperature = 0;
		public double Temperature
		{
			get { return this.temperature; }
			set { this.temperature = value; }
		}

		private EGravityBand gravityBand = EGravityBand.normal;
		public EGravityBand GravityBand
		{
			get { return this.gravityBand; }
			set { this.gravityBand = value; }
		}

		private ETemperatureBand temperatureBand = ETemperatureBand.cold;
		public ETemperatureBand TemperatureBand
		{
			get { return this.temperatureBand; }
			set { this.temperatureBand = value; }
		}

		private EAtmosphereBand atmosphereBand = EAtmosphereBand.none;
		public EAtmosphereBand AtmosphereBand
		{
			get { return this.atmosphereBand; }
			set { this.atmosphereBand = value; }
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

		#region IReporting Members

		public override string ReportName
		{
			get
			{
                return string.Format("{0} [{1}], {2} {3} AU from the planet", 
					this.FullName, 
					this.Name,
					this.MoonType.ReportName,
                    this.au);
			}
		}

		public override string BattleReportName
		{
			get
			{
				return string.Format("{0} of planet {1}", this.ReportName, this.planet.BattleReportName);
			}
		}

		public override List<string> Report(Faction faction)
		{
			List<string> reportLines = new List<string>();

			reportLines.AddRange(this.reportHeader(faction));			
			reportLines.AddRange(this.reportOrbit(faction));
			reportLines.AddRange(this.reportRegions(faction));
			return reportLines;
		}

		private List<string> reportHeader(Faction faction)
		{
			List<string> lines = new List<string>
            {
                // lines.Add(string.Format("    * {0}.", this.ReportName));
                "  * Luna [P00003] (1, 0, 0), moon, unexplored.",
                "------------------------------------------------------------"
            };
			return lines;
		}

		private List<string> reportOrbit(Faction faction)
		{
			List<string> lines = new List<string>();
			if (this.Orbit != null)
			{
				lines.AddRange(this.Orbit.Report(faction, 2));
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
					lines.AddRange(region.Report(faction, 2));
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
