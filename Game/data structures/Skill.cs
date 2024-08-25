using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class Skill : XMLProcessing
	{
        public static int BaseExperienceTreshold = 10;
		public static Skills All = new Skills();

		public Skill(SkillType type, int experience = 0)
		{
			this.SkillType = type;
			this.Experience = experience;
		}

		public SkillType SkillType { get; set; }

		public int Experience { get; set; }

		public int Level
		{
			get 
			{ 
				// TODO: tresholds for skills in xml
                if (this.Experience < Skill.BaseExperienceTreshold) // 10
                    return 1;
                else if (this.Experience < Skill.BaseExperienceTreshold * 2) // 20
                    return 2;
                else if (this.Experience < Skill.BaseExperienceTreshold * 2 * 2) // 40 
                    return 3;
                else if (this.Experience < Skill.BaseExperienceTreshold * 2 * 2 * 2) // 80
                    return 4;
                else
                    return 5;
			}
		}


        public bool Visible(Faction faction)
        {
            return true;
        }

        public override void LoadXml(XmlElement elSkill)
        {
            this.Experience = this.XMLAssignInteger(elSkill.GetAttribute("experience"), 0);
        }

        public override XmlElement SaveXml(XmlDocument doc)
        {
            this.xmlElement = doc.CreateElement("skill");

            this.xmlElement.SetAttribute("name", this.SkillType.Name);
            this.xmlElement.SetAttribute("experience", this.Experience.ToString());

            return this.xmlElement;
        }

    }
}
