using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class Modules : List<Module>
	{
		public bool HasPersistedState
		{
			get
			{
				foreach (Module module in this)
				{
					if (module.HasPersistedState)
					{
						return true;
					}
				}
				return false;
			}
		}

		public List<string> Report(Faction faction)
		{
			return this.Report(faction, 0);
		}

		public List<string> Report(Faction faction, int level)
		{
			ReportLines lines = new ReportLines();
			int index = 0;
			foreach (Module module in this)
			{
				index++;
				string line = string.Format("#{0} hit points: {1}/{2}",
					index,
					module.HitPoints,
					module.HitPoints - module.Damage);
				if (module.CaptureDamage > 0)
				{
					line = string.Format("{0}, capture: {1}", line, module.CaptureDamage);
				}
				if (module.DamageStatus != EDamageStatus.undamaged)
				{
					line = string.Format("{0}, {1}", line, module.ReportDamage);
				}
				if (!module.IsActive)
				{
					line = string.Format("{0}, {1}", line, module.ReportActive);
				}
				else if (!module.Parent.IsModuleOperational(module))
				{
					line = string.Format("{0}, inactive", line);
				}
				lines.Add(string.Concat(line, "."), level);
			}
			return lines.IndentedLines;
		}

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
					module.HitPoints,
					module.HitPoints - module.Damage);
				if (module.CaptureDamage > 0)
				{
					line = string.Format("{0}, capture: {1}", line, module.CaptureDamage);
				}
				if (module.DamageStatus != EDamageStatus.undamaged)
				{
					line = string.Format("{0}, {1}", line, module.ReportDamage);
				}
				firstAdded = false;
				if (module.Effects.Count > 0 | module.IsActive == false || !module.Parent.IsModuleOperational(module))
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
						firstAdded = true;
					}
					else if (!module.Parent.IsModuleOperational(module))
					{
						line = string.Format("{0}{1}{2}",
							line,
							(firstAdded == true) ? ", " : "",
							"inactive");
					}
				}
				lines.Add(string.Concat(line, "."), level);
			}
			return lines.IndentedLines;
		}

		public void LoadXml(XmlElement elHolder, ModuleStack holder)
		{
			XmlNodeList nodes = elHolder.SelectNodes("module");
			if (nodes.Count == 0)
			{
				return;
			}
			foreach (XmlElement elModule in nodes)
			{
				holder.AddModule(holder.XMLAssignInteger(elModule.GetAttribute("damage"), 0));
				Module loaded = holder.Modules[holder.Modules.Count - 1];
				loaded.CaptureDamage = holder.XMLAssignInteger(elModule.GetAttribute("capture"), 0);
				loaded.Online = holder.XMLAssignBoolean(elModule.GetAttribute("online"), true);
			}
		}

		public XmlElement SaveXml(XmlDocument doc, XmlElement elHolder)
		{
			if (!this.HasPersistedState)
			{
				return elHolder;
			}
			foreach (Module module in this)
			{
				XmlElement elModule = doc.CreateElement("module");
				if (module.Damage > 0)
				{
					elModule.SetAttribute("damage", module.Damage.ToString());
				}
				if (module.CaptureDamage > 0)
				{
					elModule.SetAttribute("capture", module.CaptureDamage.ToString());
				}
				if (!module.Online)
				{
					elModule.SetAttribute("online", "false");
				}
				elHolder.AppendChild(elModule);
			}
			return elHolder;
		}
	}
}
