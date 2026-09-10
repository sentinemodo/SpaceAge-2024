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

		public int CombatAttack(ModuleStack host, ModuleStack root)
		{
			int attackBonus = 0;
			foreach (Skill skill in this.Values)
			{
				if (skill.Experience < 1)
				{
					continue;
				}
				attackBonus += skill.SkillType.CombatAttack(host, root);
			}
			return attackBonus;
		}

		public int CombatDefense(ModuleStack host, ModuleStack root)
		{
			int defenseBonus = 0;
			foreach (Skill skill in this.Values)
			{
				if (skill.Experience < 1)
				{
					continue;
				}
				defenseBonus += skill.SkillType.CombatDefense(host, root);
			}
			return defenseBonus;
		}

		public int CombatInitiative(ModuleStack host, ModuleStack root)
		{
			int initiativeBonus = 0;
			foreach (Skill skill in this.Values)
			{
				if (skill.Experience < 1)
				{
					continue;
				}
				initiativeBonus += skill.SkillType.CombatInitiative(host, root);
			}
			return initiativeBonus;
		}

		public int CureChance(ModuleStack host, ModuleStack root)
		{
			int max = 0;
			foreach (Skill skill in this.Values)
			{
				if (skill.Experience < 1)
				{
					continue;
				}
				int chance = skill.SkillType.EvaluateCureChance(host, root);
				if (chance > max)
				{
					max = chance;
				}
			}
			return max;
		}

		public int ResearchOutputBonus(ModuleStack host, ModuleStack root)
		{
			int bonus = 0;
			foreach (Skill skill in this.Values)
			{
				if (skill.Experience < 1)
				{
					continue;
				}
				bonus += skill.SkillType.ResearchOutputBonus(host, root);
			}
			return bonus;
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
