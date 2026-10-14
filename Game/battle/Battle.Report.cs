using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public partial class Battle
	{
		#region IReporting Members

		private void prepareBattleReports()
		{
			foreach (Faction faction in this.participants)
			{
				if (!this.battleReports.ContainsKey(faction))
				{
					this.battleReports.Add(faction, new ReportLines());
				}
			}
			if (this.attacker != null && this.attacker.Location != null)
			{
				foreach (Faction faction in Faction.All.Values)
				{
					if (this.participants.Contains(faction))
					{
						continue;
					}
					if (this.attacker.Location.HasPresence(faction) && !this.observerReports.ContainsKey(faction))
					{
						this.observerReports.Add(faction, new ReportLines());
					}
				}
			}
		}

		private void report(Faction faction, string line)
		{
			if (this.battleReports.ContainsKey(faction))
			{
				this.battleReports[faction].Add(line, 1);
			}
		}

		private void report(Faction faction, List<string> lines)
		{
			if (this.battleReports.ContainsKey(faction))
			{
				this.battleReports[faction].Add(lines, 1);
			}
		}

		private void report(ModuleStacks moduleStacks, string line)
		{
			foreach (Faction faction in moduleStacks.Owners)
			{
				this.report(faction, line);
			}
		}

		private void report(string line)
		{
			foreach (Faction faction in this.battleReports.Keys)
			{
				this.report(faction, line);
			}
		}

		private void reportObserver(string line)
		{
			foreach (Faction faction in this.observerReports.Keys)
			{
				this.observerReports[faction].Add(line, 1);
			}
		}

		private void reportPublic(string line)
		{
			this.report(line);
			this.reportObserver(line);
		}

		public List<string>  Report(Faction faction)
		{
			List<string> reportLines = new List<string>();

			if (!this.battleReports.ContainsKey(faction) && !this.observerReports.ContainsKey(faction))
			{
				return reportLines;
			}

			reportLines.AddRange(this.reportHeader(faction));
			reportLines.AddRange(this.reportDetails(faction));

			return reportLines;
		}

		private List<string> reportDetails(Faction faction)
		{
			if (this.battleReports.ContainsKey(faction))
			{
				return this.battleReports[faction].IndentedLines;
			}
			if (this.observerReports.ContainsKey(faction))
			{
				return this.observerReports[faction].IndentedLines;
			}
			return new List<string>();
		}

		private List<string> reportHeader(Faction faction)
		{
			List<string> lines = new List<string>
            {
                string.Format("  Week {0}.", this.week),
                string.Format("  Battle has commenced at {0}.", this.attacker.Location.BattleReportName),
                "  ------------------------------------------------------------"
            };
			return lines;
		}

		#endregion
	}
}
