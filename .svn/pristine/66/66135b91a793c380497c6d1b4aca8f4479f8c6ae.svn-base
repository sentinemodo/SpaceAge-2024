using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class SkillType : NamedObject
	{

		public static SkillTypes All = new SkillTypes();

		public SkillType(string name)
			: base(name)
		{
			if (SkillType.All.ContainsKey(name))
			{
				throw new Exception("SkillType with name " + name + " already exists");
			}

			SkillType.All.Add(this.name, this);
			this.FullName = FullName;
		}

		private int trainingDuration;
		public int TrainingDuration
		{
			get { return this.trainingDuration; }
			set { this.trainingDuration = value; }
		}

		public bool IsBattleSkill
		{
			get
			{
				if (this.attack != 0 | this.defense != 0 | this.initiative != 0)
				{
					return true;
				}
				return false;
			}
		}

		private int attack;
		public int Attack
		{
			get { return this.attack; }
			set { this.attack = value; }
		}

		private int defense;
		public int Defense
		{
			get { return this.defense; }
			set { this.defense = value; }
		}

		private int initiative;
		public int Initiative
		{
			get { return this.initiative; }
			set { this.initiative = value; }
		}

		public string ReportDetails(int Experience)
		{
			string line = string.Empty;
			line = this.ReportName;
			if (this.IsBattleSkill)
			{
				line = string.Concat(line, " (");
				//if (this.Level > 1)
				//{
				//	line = string.Concat(line, "level: ", this.level);
				//}
				//if (this.Level > 1 & this.IsBattleSkill)
				//{
				//	line = string.Concat(line, ", ");
				//}
				if (this.IsBattleSkill)
				{
					bool firstAdded = false;
					if (this.Attack != 0)
					{
						line = string.Format("{0}{1}attack: {2}",
							line,
							(firstAdded) ? ", " : "",
							this.Attack);
						firstAdded = true;
					}
					if (this.Defense != 0)
					{
						line = string.Format("{0}{1}defense: {2}",
							line,
							(firstAdded) ? ", " : "",
							this.Defense);
						firstAdded = true;
					}
					if (this.Initiative != 0)
					{
						line = string.Format("{0}{1}initiative: {2}",
							line,
							(firstAdded) ? ", " : "",
							this.Initiative);
						firstAdded = true;
					}
				}
				line = string.Concat(line, ")");
			}
			return line;

		}

	}
}
