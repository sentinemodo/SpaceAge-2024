using System;

using System.Collections.Generic;

using System.Text;



namespace SpaceAge

{

	public class SkillType : NamedObject

	{

		private readonly List<SkillUsableIn> usableIn = new List<SkillUsableIn>();

		private readonly SkillPercentProduces percentProduces = new SkillPercentProduces();

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

			this.AttackFormula = SkillBonusFormula.Zero;

			this.DefenseFormula = SkillBonusFormula.Zero;

			this.InitiativeFormula = SkillBonusFormula.Zero;

			this.CureChanceFormula = SkillBonusFormula.Zero;

			this.ProduceFormula = SkillBonusFormula.Zero;

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

				if (this.AttackFormula.IsNonZero

					| this.DefenseFormula.IsNonZero

					| this.InitiativeFormula.IsNonZero)

				{

					return true;

				}

				return false;

			}

		}



		public SkillBonusFormula AttackFormula { get; set; }



		public SkillBonusFormula DefenseFormula { get; set; }



		public SkillBonusFormula InitiativeFormula { get; set; }



		public SkillBonusFormula CureChanceFormula { get; set; }



		public IList<SkillUsableIn> UsableIn

		{

			get { return this.usableIn; }

		}



		public string ProduceEffect { get; set; }



		public string ProduceTarget { get; set; }



		public SkillBonusFormula ProduceFormula { get; set; }

		public SkillPercentProduces PercentProduces
		{
			get { return this.percentProduces; }
		}

		public bool AppliesTo(ModuleStack host, ModuleStack root)

		{

			if (this.usableIn.Count == 0)

			{

				return true;

			}



			if (host != null && host.ModuleType != null)

			{

				foreach (SkillUsableIn usable in this.usableIn)

				{

					if (usable.Matches(host))

					{

						return true;

					}

				}

			}



			if (root != null && root.ModuleType != null)

			{

				foreach (SkillUsableIn usable in this.usableIn)

				{

					if (usable.Matches(root))

					{

						return true;

					}

				}

			}



			return false;

		}



		public int CombatAttack(ModuleStack host, ModuleStack root)

		{

			if (!this.AppliesTo(host, root))

			{

				return 0;

			}



			int bonus = this.AttackFormula.Evaluate(host, root);

			if (this.ProduceEffect == "effective attack" && this.ProduceTarget == "units")

			{

				bonus += this.ProduceFormula.Evaluate(host, root) * root.QuantityActive;

			}



			return bonus;

		}



		public int CombatDefense(ModuleStack host, ModuleStack root)

		{

			if (!this.AppliesTo(host, root))

			{

				return 0;

			}



			int bonus = this.DefenseFormula.Evaluate(host, root);

			if (this.ProduceEffect == "effective defence" && this.ProduceTarget == "units")

			{

				bonus += this.ProduceFormula.Evaluate(host, root) * root.QuantityActive;

			}



			return bonus;

		}



		public int CombatInitiative(ModuleStack host, ModuleStack root)

		{

			if (!this.AppliesTo(host, root))

			{

				return 0;

			}



			return this.InitiativeFormula.Evaluate(host, root);

		}



		public int EvaluateCureChance(ModuleStack host, ModuleStack root)

		{

			if (!this.AppliesTo(host, root))

			{

				return 0;

			}



			return this.CureChanceFormula.Evaluate(host, root);

		}



		public int ResearchOutputBonus(ModuleStack host, ModuleStack root)

		{

			if (!this.AppliesTo(host, root))

			{

				return 0;

			}



			if (this.ProduceEffect == "research output")

			{

				return this.ProduceFormula.Evaluate(host, root);

			}



			return 0;

		}



		public string ReportDetails(int Experience, ModuleStack host, ModuleStack root)
		{
			string line = this.ReportName;

			if (this.IsBattleSkill)
			{
				line = string.Concat(line, " (");
				bool firstAdded = false;

				if (this.AttackFormula.IsNonZero)
				{
					line = string.Format("{0}{1}attack: {2}",
						line,
						firstAdded ? ", " : "",
						this.CombatAttack(host, root));
					firstAdded = true;
				}
				if (this.DefenseFormula.IsNonZero)
				{
					line = string.Format("{0}{1}defense: {2}",
						line,
						firstAdded ? ", " : "",
						this.CombatDefense(host, root));
					firstAdded = true;
				}
				if (this.InitiativeFormula.IsNonZero)
				{
					line = string.Format("{0}{1}initiative: {2}",
						line,
						firstAdded ? ", " : "",
						this.CombatInitiative(host, root));
					firstAdded = true;
				}

				line = string.Concat(line, ")");
			}

			return line;
		}



	}

}


