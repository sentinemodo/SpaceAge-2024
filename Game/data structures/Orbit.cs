using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Orbit : Location, IBattleReporting, IResourcesHolder
	{
		public Orbit(IOrbitHolder orbitHolder, string name)
			: base(name) 
		{
			while (string.IsNullOrEmpty(this.name) || Orbit.All.ContainsKey(this.name))
				this.name = this.GenerateRandomIdentifier("O", Orbit.OrbitNameLength);
			Orbit.All.Add(this.name, this);
			this.locationParent = orbitHolder;
		}
		
		public const int OrbitNameLength = 5;
		public static Orbits All = new Orbits();

		public IOrbitHolder OrbitHolder
		{
			get { return (IOrbitHolder)this.locationParent; }
			set { this.locationParent = value; }
		}

		private Resources resources = new Resources();
		public Resources Resources
		{
			get { return this.resources; }
		}

		private Races races = new Races();
		public Races Races
		{
			get { return this.races; }
		}

		public bool HasAtmosphere
		{
			get
			{
				if (this.resources.Count > 0 || this.races.Count > 0)
				{
					return true;
				}
				Planet planet = this.OrbitHolder as Planet;
				if (planet != null)
				{
					return planet.Races.Count > 0 || planet.AtmosphereBand != EAtmosphereBand.none;
				}
				Moon moon = this.OrbitHolder as Moon;
				if (moon != null)
				{
					return moon.Races.Count > 0 || moon.AtmosphereBand != EAtmosphereBand.none;
				}
				return false;
			}
		}

    public override ELocationType LocationType
    {
        get { return ELocationType.orbit; }
    }

		#region IReporting Members

		public override string ReportName
		{
			get
			{
				return string.Format("orbit [{0}]", this.name);
			}
		}

    public override string BattleReportName
    {
        get
        {
            return string.Format("{0} of {1}", this.ReportName, this.OrbitHolder.BattleReportName);
        }
    }

        public override List<string> Report(Faction faction)
		{
			return this.Report(faction, 0);
		}

		public List<string> Report(Faction faction, int level)
		{
			ReportLines reportLines = new ReportLines
            {
                { this.reportHeader(), level }
            };

			if (this.resources.Count > 0)
			{
				reportLines.Add(this.Resources.Report, level);
			}
			// region modulestacks
			foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
			{
				if (!moduleStack.Visible(faction))
					continue;

				reportLines.Add(moduleStack.Report(faction), level);
			}
			return reportLines.IndentedLines;
		}

		private string reportHeader()
		{
			string line = this.ReportName;
			if (this.HasAtmosphere) 
			{
				line = string.Concat(line, ", has atmosphere");
				List<Race> listed = this.AtmosphereRaces();
				if (listed.Count > 0)
				{
					line = string.Concat(line, " suitable for");
					bool firstAdded = false;
					foreach (Race race in listed)
					{
						line = string.Format("{0} {1}",
							firstAdded ? string.Concat(line, ",") : line,
							race.ReportName);
						firstAdded = true;
					}
				}
			} 
			else 
			{ 
				line = string.Concat(line, ", has no atmosphere"); 
			}

			return string.Concat(line, ".");
		}

		private List<Race> AtmosphereRaces()
		{
			List<Race> listed = new List<Race>();
			foreach (Race race in this.races.Values)
			{
				listed.Add(race);
			}
			Races bodyRaces = null;
			Planet planet = this.OrbitHolder as Planet;
			if (planet != null)
			{
				bodyRaces = planet.Races;
			}
			else
			{
				Moon moon = this.OrbitHolder as Moon;
				if (moon != null)
				{
					bodyRaces = moon.Races;
				}
			}
			if (bodyRaces != null)
			{
				foreach (Race race in bodyRaces.Values)
				{
					if (!this.races.ContainsKey(race.Name))
					{
						listed.Add(race);
					}
				}
			}
			return listed;
		}

		#endregion


		public bool Visible(Faction faction)
		{
			return true;
		}
	}
}
