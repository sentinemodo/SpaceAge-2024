using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Modules : List<Module>
	{
		public List<string> BattleReport(Faction faction)
		{
			return this.BattleReport(faction, 0);
		}

		public List<string> BattleReport(Faction faction, int level)
		{
			ReportLines lines = new ReportLines();
			string line;
			bool firstAdded;

			int index = 0;
			foreach (Module module in this)
			{
				index ++;
				line = string.Format("#{0} hit points: {1}/{2}",
					index,
					module.Parent.ModuleType.HitPoints,
					module.Parent.ModuleType.HitPoints - module.Damage);
				firstAdded = false;
				if (module.Effects.Count > 0 | module.IsActive == false)
				{
					line = string.Concat(line, ", effects: ");
					if (module.Effects.Count > 0)
					{
						foreach (Effect effect in module.Effects)
						{
							line = string.Format("{0}{1}{2}",
								line,
								(firstAdded == true) ? ", " : "",
								effect.Description);
							firstAdded = true;
						}
					}
					if (module.IsActive == false)
					{
						line = string.Format("{0}{1}{2}",
							line,
							(firstAdded == true) ? ", " : "",
							module.ReportActive);
					}
				}
				lines.Add(string.Concat(line, "."), level);
			}
			return lines.IndentedLines;
		}
	}
}
