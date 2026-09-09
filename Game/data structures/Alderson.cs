using System;
using System.Collections.Generic;

namespace SpaceAge
{
	public class Alderson : SpaceSystemObject, IOrbitHolder
	{
		public const int AldersonNameLength = 5;
		public static Aldersons All = new Aldersons();

		public Alderson(SpaceSystem spaceSystem, string name)
			: base(name)
		{
			while (string.IsNullOrEmpty(this.name) || Alderson.All.ContainsKey(this.name))
			{
				this.name = this.GenerateRandomIdentifier("P", Alderson.AldersonNameLength);
			}
			Alderson.All.Add(this.name, this);
			this.spaceSystem = spaceSystem;
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

		private string pairName = string.Empty;
		public string PairName
		{
			get { return this.pairName; }
			set { this.pairName = value ?? string.Empty; }
		}

		public Alderson PairAlderson
		{
			get
			{
				if (string.IsNullOrEmpty(this.pairName) || !Alderson.All.ContainsKey(this.pairName))
				{
					return null;
				}
				return Alderson.All[this.pairName];
			}
		}

		public Orbit Orbit
		{
			get { return Orbit.All[this]; }
		}

		public override string ReportName
		{
			get
			{
				return string.Format("{0} [{1}] at AU {2}, Alderson Gate", this.FullName, this.Name, this.au);
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
			List<string> lines = new List<string>
			{
				"",
				string.Format("  * {0}.", this.ReportName),
				"------------------------------------------------------------"
			};
			if (this.Orbit != null)
			{
				lines.AddRange(this.Orbit.Report(faction, 1));
			}
			return lines;
		}

		public override bool Visible(Faction faction)
		{
			return true;
		}
	}
}
