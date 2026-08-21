using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Battles : List<Battle>, IReporting
	{
		#region IReporting Members

		public List<string> Report(Faction faction)
		{
			List<string> reportLines = new List<string>
            {
                "Battles report:",
                ""
            };
			List<Battle> sorted = new List<Battle>(this);
			sorted.Sort(delegate(Battle a, Battle b) { return a.Week.CompareTo(b.Week); });
			foreach (Battle battle in sorted)
			{
				if (reportLines.Count > 2)
				{
					reportLines.Add("");
				}
				reportLines.AddRange(battle.Report(faction));
			}
			reportLines.Add("");

			return reportLines;
		}

		#endregion
	}
}
