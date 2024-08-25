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

		public List<string> Report
		{
			get
			{
				string line;
				List<string> lines = new List<string>
                {
                    "Exits:"
                };
				foreach (Exit exit in this)
				{
					Region destination = (Region)exit.To;
					line = string.Format("  {0}, {1}", destination.ReportName, destination.RegionType.FullName);
					foreach (ExitMode exitMode in exit.ExitModes.Values)
					{
						line = string.Format("{0}, {1}", line, exitMode.ReportName);
					}
					lines.Add(string.Concat(line, "."));
				}
				return lines;
			}
		}

	}
}
