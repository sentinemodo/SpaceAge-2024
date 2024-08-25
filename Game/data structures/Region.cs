using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Region : Location, IResourcesHolder, IBattleReporting
	{
		public Region(IRegionHolder regionHolder, string name)
			: base(name) 
		{
			while (string.IsNullOrEmpty(this.name) || Region.All.ContainsKey(this.name))
				this.name = this.GenerateRandomIdentifier("R", Region.RegionNameLength);
			Region.All.Add(this.name, this);
			this.locationParent = regionHolder;
		}
		
		public const int RegionNameLength = 5;
		public static Regions All = new Regions();

		private RegionType regionType;
		public RegionType RegionType 
		{
			get { return this.regionType; }
			set { this.regionType = value; }
		}

        private Point2D coordinates = new Point2D();
		public Point2D Coordinates
		{
			get { return this.coordinates; }
		}

		public IRegionHolder RegionHolder
		{
            get { return (IRegionHolder)this.locationParent; }
            set { this.locationParent = value; }
		}

		private Capacities capacities = new Capacities();
		public Capacities Capacities
		{
			get { return this.capacities; }
		}

		private Resources resources = new Resources();
		public Resources Resources
		{
			get { return this.resources; }
		}

        public override ELocationType LocationType
        {
            get { return this.RegionType.LocationType; }
        }

		#region IReporting Members
		public override string  ReportName
		{
			get 
			{ 
				 return string.Format("{0} [{1}] ({2},{3})", this.FullName, this.Name, this.Coordinates.X, this.Coordinates.Y);
			}
		}

		public override string BattleReportName
		{
			get
			{
				return string.Format("{0} on {4}", this.ReportName, this.RegionHolder.BattleReportName);
			}
		}

		private string reportHeader()
		{
			string line;
			line = string.Format("{0}, {1}", this.ReportName, this.RegionType.FullName);
			foreach (Capacity capacity in this.Capacities)
			{
				int current = this.ModuleStacks.Quantity(capacity.Group);
				line = string.Format("{0}, {1} capacity {2}/{3}", line, capacity.Group, capacity.Quantity, current);
			}
			line = string.Concat(line, ".");
			return line;
		}



        public override List<string> Report(Faction faction)
		{
			return this.Report(faction, 0);
		}

		public List<string> Report(Faction faction, int level)
		{
			ReportLines reportLines = new ReportLines();
			reportLines.Add(this.reportHeader(), level);
			reportLines.Add(this.Exits.Report, level);
			if (this.resources.Count > 0)
			{
				reportLines.Add(this.Resources.Report, level);
			}

			// market report
			reportLines.Add(this.Market.Report(faction), level);

			// region modulestacks
			foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
			{
				if (!moduleStack.Visible(faction))
					continue;

				reportLines.Add(moduleStack.Report(faction), level);
			}
			return reportLines.IndentedLines;
		}

		#endregion

		public bool Visible(Faction faction)
		{
            if (faction == null)
            {
                return true;
            }
			if (faction.FullName == "NPC")
			{
				return true;
			}
			if (this.ModuleStacks.Contains(faction))
			{
				return true;
			}
			return false;
		}
	}
}
