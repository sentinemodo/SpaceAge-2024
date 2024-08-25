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
			List<string> reportLines = new List<string>();
			reportLines.Add("Battles report:");
			reportLines.Add("");
			foreach (Battle battle in this)
			{
				reportLines.AddRange(battle.Report(faction));
			}
			reportLines.Add("");

			return reportLines;
		}

		#endregion
	}
}
