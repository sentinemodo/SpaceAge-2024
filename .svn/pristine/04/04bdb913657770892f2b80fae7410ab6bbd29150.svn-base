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
				if (resources.Count > 0 | races.Count > 0)
				{
					return true;
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
			ReportLines reportLines = new ReportLines();
			reportLines.Add(this.reportHeader(), level);

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
				if (this.races.Count > 0)
				{
					line = string.Concat(line, " suitable for"); 					
					bool firstAdded = false;
					foreach (Race race in this.races.Values) 
					{
						line = string.Format("{0} {1}",
							(firstAdded == true) ? string.Concat(line, ",") : line, 
							race.ReportName);
					}
				}
			} 
			else 
			{ 
				line = string.Concat(line, ", has no atmosphere"); 
			}

			return string.Concat(line, ".");
		}

		#endregion


		public bool Visible(Faction faction)
		{
			return true;
		}
	}
}
