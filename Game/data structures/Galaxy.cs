using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Galaxy : IReporting
	{
		private SpaceSystems spaceSystems = new SpaceSystems();
		public SpaceSystems SpaceSystems
		{
			get { return this.spaceSystems; }
			set { this.spaceSystems = value; }
		}

		#region IReporting Members

		public List<string> Report(Faction faction)
		{
			List<string> reportLines = new List<string>();

			reportLines.AddRange(this.reportHeader(faction));
			// deepSpace moduleStacks
			reportLines.AddRange(this.reportSpaceSystems(faction));
			reportLines.Add("");

			return reportLines;
		}

		private List<string> reportHeader(Faction faction)
		{
			List<string> lines = new List<string>
            {
                "Galaxy report:",
                "------------------------------------------------------------"
            };
			return lines;
		}

		private List<string> reportSpaceSystems(Faction faction)
		{
			List<string> lines = new List<string>();
			foreach (SpaceSystem spaceSystem in this.SpaceSystems)
			{
				if (spaceSystem.Visible(faction))
				{
					lines.AddRange(spaceSystem.Report(faction));
				}
			}

			return lines;
		}

		#endregion
	}
}
