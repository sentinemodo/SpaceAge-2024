using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class KnownSkills : List<SkillType>
	{
		public bool Contains(string name)
		{
			foreach (SkillType skill in this)
			{
				if (skill.Name == name)
				{
					return true;
				}
			}
			return false;
		}

		public SkillType this[string name]
		{
			get
			{
				foreach (SkillType skill in this)
				{
					if (skill.Name == name)
					{
						return skill;
					}
				}
				return null;
			}
		}

		public string ReportList
		{
			get
			{
				StringBuilder line = new StringBuilder();
				bool first = true;
				foreach (SkillType skill in this)
				{
					if (!first)
					{
						line.Append(", ");
					}
					first = false;
					line.Append(skill.ReportName);
				}
				return line.ToString();
			}
		}
	}
}
