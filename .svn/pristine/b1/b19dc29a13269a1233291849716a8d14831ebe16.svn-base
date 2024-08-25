using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class Skills : Dictionary<SkillType, Skill>, IReporting
	{
		public void Add(SkillType skillType, int experience = 1)
		{
			if (this.ContainsKey(skillType))
			{
				this[skillType].Experience += experience;
			}
			else
			{
                base.Add(skillType, new Skill(skillType, experience));
			}
		}

		public void Remove(Skill skill)
		{
			// you can't remove skill you don't have
			if (!this.ContainsKey(skill.SkillType) || this[skill.SkillType].Experience < skill.Experience)
			{
				throw new Exception("No skill or not enough experience of " + skill.SkillType.ReportName);
			}
			else
			{
				this[skill.SkillType].Experience -= skill.Experience;
				if (this[skill.SkillType].Experience == 0)
				{
					base.Remove(skill.SkillType);
				}
			}
		}

		public new void Remove(SkillType skillType)
		{
			// you can't remove skill you don't have
			if (!this.ContainsKey(skillType))
			{
				throw new Exception("No skill " + skillType.ReportName);
			}
			else
			{
				base.Remove(skillType);
			}
		}

		public bool Has(SkillType skillType)
		{
			if (!this.ContainsKey(skillType) || this[skillType].Experience == 0)
			{
				return false;
			}
			return true;
		}

		public int Attack
		{
			get
			{
				int attackBonus = 0;
				foreach (Skill skill in this.Values)
				{
					attackBonus += skill.SkillType.Attack;
				}
				return attackBonus;
			}
		}

		public int Defense
		{
			get
			{
				int defenseBonus = 0;
				foreach (Skill skill in this.Values)
				{
					defenseBonus += skill.SkillType.Defense;
				}
				return defenseBonus;
			}
		}

		public int Initiative
		{
			get
			{
				int initiativeBonus = 0;
				foreach (Skill skill in this.Values)
				{
					initiativeBonus += skill.SkillType.Initiative;
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
				foreach (Skill skill in this.Values)
				{
					line = string.Format("{0}{1}", line, (firstAdded) ? ", " : "");
					firstAdded = true;
					line = string.Concat(line, skill.SkillType.ReportName);
				}
				return line;
			}
		}

		public string ReportBattleSkillList
		{
			get
			{
				string line = string.Empty;
				bool firstAdded = false;
				foreach (Skill skill in this.Values)
				{
					if (skill.SkillType.IsBattleSkill)
					{
						line = string.Format("{0}{1}", line, (firstAdded) ? ", " : "");
						firstAdded = true;
						line = string.Concat(line, skill.SkillType.ReportName);
					}
				}
				return line;
			}
		}
		public int CountBattleSkills
		{
			get
			{
				int count = 0;
				foreach (Skill skill in this.Values)
				{
					if (skill.SkillType.IsBattleSkill)
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
			string line = string.Format("skills: {0}.", this.ReportList);
			ReportLine reportLine = new ReportLine(line, level);
			return reportLine.IndentedLines;
		}

		#endregion

        public void LoadXml(XmlElement elHolder, Person holder)
        {
            Skill skill;
            SkillType skillType;

            foreach (XmlElement elSkill in elHolder.SelectNodes("skill"))
            {
                skillType = SkillType.All[elSkill.GetAttribute("name")];
                skill = new Skill(skillType);
                skill.LoadXml(elSkill);
                holder.Skills.Add(skillType, skill);
            }
        }

        public XmlElement SaveXml(XmlDocument doc, XmlElement elHolder, Faction faction = null)
        {
            XmlElement elSkill;

            foreach (Skill skill in this.Values)
            {
                if (!skill.Visible(faction))
                    continue;

                elSkill = skill.SaveXml(doc);
                elHolder.AppendChild(elSkill);
            }

            return elHolder;
        }
	}
}
