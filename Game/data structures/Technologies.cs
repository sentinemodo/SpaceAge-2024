using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class Technologies : List<Technology>, IReporting
	{
		public Technology this[string name]
		{
			get
			{
                Technology found = null;
                foreach (Technology technology in this)
                {
                    if (technology.Name == name)
                    {
                        found = technology;
                    }
                }
                return found;
            }
		}

        public bool Contains(string name)
        {
            if (this[name] != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

		public int MaxTechnologyLevel
		{
			get
			{
                int maxTechnologyLevel = 0;
                foreach (Technology technology in this)
                {
					if (technology.Level > maxTechnologyLevel)
					{
						maxTechnologyLevel = technology.Level;
					}
                }
                return maxTechnologyLevel;
            }
        }


		public int Attack
		{
			get
			{
				int attackBonus = 0;
				foreach (Technology technology in this)
				{
					attackBonus += technology.Attack;
				}
				return attackBonus;
			}
		}

		public int Defense
		{
			get
			{
				int defenseBonus = 0;
				foreach (Technology technology in this)
				{
					defenseBonus += technology.Defense;
				}
				return defenseBonus;
			}
		}

		public int Initiative
		{
			get
			{
				int initiativeBonus = 0;
				foreach (Technology technology in this)
				{
					initiativeBonus += technology.Initiative;
				}
				return initiativeBonus;
			}
		}

		public string ReportList
		{
			get
			{
				string line = string.Empty;
				bool firstAdded = false;
				foreach (Technology technology in this)
				{
					line = string.Format("{0}{1}", line, (firstAdded) ? ", " : "");
					firstAdded = true;
					line = string.Concat(line, technology.ReportName);
				}
				return line;
			}
		}

		public string ReportBattleTechnologiesList
		{
			get
			{
				string line = string.Empty;
				bool firstAdded = false;
				foreach (Technology technology in this)
				{
					if (technology.IsBattleTechnology)
					{
						line = string.Format("{0}{1}", line, (firstAdded) ? ", " : "");
						firstAdded = true;
						line = string.Concat(line, technology.ReportName);
					}
				}
				return line;
			}
		}
		public int CountBattleTechnologies
		{
			get
			{
				int count = 0;
				foreach (Technology technology in this)
				{
					if (technology.IsBattleTechnology)
					{
						count++;
					}
				}
				return count;
			}
		}

		#region IReporting Members

		public List<string> Report(Faction faction)
		{
			return this.Report(faction, 0);
		}

		public List<string> Report(Faction faction, int level)
		{
			string line = string.Format("technologies: {0}.", this.ReportList);
			ReportLine reportLine = new ReportLine(line, level);
			return reportLine.IndentedLines;
		}

        public List<string> ReportDescriptions(Faction faction, int level)
        {
            ReportLines reportLines = new ReportLines();
			foreach (Technology technology in this)
			{
                reportLines.Add(string.Format("{0}: {1}.", technology.ReportName, technology.Description), level);
                reportLines.Add(string.Empty, level);
            }
            return reportLines.IndentedLines;
        }

        #endregion

        public void LoadXml(XmlElement elHolder, ModuleStack holder)
        {
            Technology technology;

            foreach (XmlElement elTechnology in elHolder.SelectNodes("technology"))
            {
                technology = Technology.All[elTechnology.GetAttribute("name")];
                holder.Technologies.Add(technology);
            }
        }
        
        public XmlElement SaveXml(XmlDocument doc, XmlElement elHolder)
        {
            XmlElement elTechnology;

            foreach (Technology technology in this)
            {
                elTechnology = technology.SaveXml(doc);
                elHolder.AppendChild(elTechnology);
            }

            return elHolder;
        }
	}
}
