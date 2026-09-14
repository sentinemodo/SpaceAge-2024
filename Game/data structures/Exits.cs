using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Exits : List<Exit>
	{

        public Exit this[IHolder index]
		{
			get 
			{
				foreach (Exit e in this)
				{
					if (e.To.Name == index.Name)
					{
						return e;
					}
				}
				throw new KeyNotFoundException("Tried to lookup [" + index + "]");
			}
		}

        public bool Contains(IHolder index)
		{
			foreach (Exit e in this)
			{
				if (e.To.Name == index.Name)
				{
					return true;
				}
			}
			return false;
		}

		public List<string> Report(Faction faction, Region fromRegion)
		{
			string line;
			List<string> lines = new List<string>
			{
				"Exits:"
			};
			foreach (Exit exit in this)
			{
				Region region = exit.To as Region;
				if (region != null)
				{
					line = string.Format("  {0}, {1}", region.ReportName, region.RegionType.FullName);
				}
				else
				{
					line = string.Format("  {0}", exit.To.ReportName);
				}
				foreach (ExitMode exitMode in exit.ExitModes.Values)
				{
					line = string.Format("{0}, {1}", line, exitMode.ReportName);
				}
				if (region != null
					&& region.HasAnomaly
					&& (faction == null || !region.Anomaly.IsResolved(faction))
					&& fromRegion != null
					&& fromRegion.Visible(faction))
				{
					line = string.Format("{0}, anomaly detected", line);
				}
				if (region != null
					&& region.HasDeepPocket
					&& fromRegion != null
					&& fromRegion.Visible(faction)
					&& fromRegion.HasCdrillTechnologyFor(faction))
				{
					line = string.Format("{0}, deep pocket of resources detected", line);
				}
				if (region != null
					&& region.HasSettlement
					&& fromRegion != null
					&& fromRegion.Visible(faction))
				{
					line = string.Format("{0}, settlement detected", line);
				}
				lines.Add(string.Concat(line, "."));
			}
			return lines;
		}

	}
}
