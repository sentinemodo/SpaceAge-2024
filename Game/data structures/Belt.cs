using System;
using System.Collections.Generic;

namespace SpaceAge
{
	public class CompositionEntry
	{
		public ItemType ItemType { get; set; }
		public int Quantity { get; set; }
		public double Probability { get; set; }
	}

	public class Belt : Location, IBattleReporting, IResourcesHolder
	{
		public const int BeltNameLength = 5;
		public static Belts All = new Belts();

		public Belt(SpaceSystem spaceSystem, Planet planet, string name)
			: base(name)
		{
			while (string.IsNullOrEmpty(this.name) || Belt.All.ContainsKey(this.name))
			{
				this.name = this.GenerateRandomIdentifier("B", Belt.BeltNameLength);
			}
			Belt.All.Add(this.name, this);
			this.spaceSystem = spaceSystem;
			this.planet = planet;
		}

		private SpaceSystem spaceSystem;
		public SpaceSystem SpaceSystem
		{
			get { return this.spaceSystem; }
			set { this.spaceSystem = value; }
		}

		private Planet planet;
		public Planet Planet
		{
			get { return this.planet; }
			set { this.planet = value; }
		}

		private double au;
		public double AU
		{
			get { return this.au; }
			set { this.au = value; }
		}

		private List<CompositionEntry> composition = new List<CompositionEntry>();
		public List<CompositionEntry> Composition
		{
			get { return this.composition; }
		}

		public Resources Resources
		{
			get
			{
				Resources resources = new Resources();
				foreach (CompositionEntry entry in this.composition)
				{
					Resource resource = new Resource();
					resource.ItemType = entry.ItemType;
					resource.Quantity = entry.Quantity;
					resources.Add(resource);
				}
				return resources;
			}
		}

		public override ELocationType LocationType
		{
			get { return ELocationType.space; }
		}

		public override double Capacity
		{
			get { return double.MaxValue; }
		}

		public override double CapacityUsed
		{
			get { return this.ModuleStacks.Size(); }
		}

		public override string ReportName
		{
			get
			{
				return string.Format("{0} [{1}] at AU {2}, belt", this.FullName, this.Name, this.AU);
			}
		}

		public override string BattleReportName
		{
			get
			{
				if (this.planet != null)
				{
					return string.Format("{0} of planet {1}", this.ReportName, this.planet.BattleReportName);
				}
				return string.Format("{0} in {1}", this.ReportName, this.spaceSystem.ReportName);
			}
		}

		public override List<string> Report(Faction faction)
		{
			List<string> lines = new List<string>
			{
				"",
				string.Format("  * {0}.", this.ReportName),
				"------------------------------------------------------------"
			};
			return lines;
		}

		public bool Visible(Faction faction)
		{
			return true;
		}
	}
}
