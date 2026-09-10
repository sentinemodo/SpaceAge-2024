using System;
using System.Collections.Generic;

namespace SpaceAge
{
	public partial class ModuleStack
	{
		#region battle report

		public List<string> BattleReport(Faction faction)
		{
			ReportLines lines = new ReportLines
            {
                this.ReportHeader(faction),
                { this.battleReportDetails(faction), 1 }
            };

			// submodulestacks
			foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
			{
				lines.Add(moduleStack.BattleReport(faction), 1);
			}
			return lines.IndentedLines;
		}

		private List<string> battleReportDetails(Faction faction)
		{
			ReportLines lines = new ReportLines();

			string line;
			line = string.Empty;
			line = this.reportSize(line);
			line = this.reportMass(faction, line);
			line = this.reportEnergyUsage(faction, line, false);
			line = this.reportCrew(faction, line);
			lines.Add(string.Concat(line, "."));

			line = string.Empty;
			line = this.reportHitPoints(faction, line);
			line = this.reportAttack(faction, line);
			line = this.reportDefense(faction, line);
			line = this.reportInitiative(line);
			lines.Add(string.Concat(line, "."));
			
			if (this.owner == faction & this.tactics.Count > 0)
			{
				lines.Add(string.Format("tactics: {0}.", this.Tactics.ReportList));
			}

			if (this.owner == faction & this.technologies.CountBattleTechnologies > 0)
			{
				line = string.Empty;
				line = this.reportBattleTechnologies(line);
				lines.Add(string.Concat(line, "."));
			}

			ModuleStack parentStack = this.parent as ModuleStack;
			if (this.owner == faction && this.itemStacks.Count > 0
				&& parentStack != null && parentStack.Owner != faction)
			{
				lines.Add(string.Concat("items: ", this.ItemStacks.ReportList, "."));
			}

			//if (this.owner == faction && this.effects.Count > 0)
			//{
			//    reportLines.Add(this.effects.Report(faction, level + 1));
			//}
			lines.Add(this.modules.BattleReport(faction), 1);

			if (this.People.CountBattleSkilled > 0)
			{
				foreach (Person person in this.People.Values)
				{
					if (person.IsBattleSkilled)
					{
						lines.Add(person.BattleReport(faction));
					}
				}
			}
			return lines.IndentedLines;
		}

		private string reportBattleTechnologies(string line)
		{
			//TODO: change as method, pass modulestack
			// change to calculate the effect of a technology use against enemy modulestack it as to be ... er ... used

			line = string.Format("{0}technologies: ",
			   (line == string.Empty) ? string.Empty : string.Concat(line, ", "));

			bool firstAdded = false;
			foreach (Technology technology in this.technologies)
			{
				if (technology.IsBattleTechnology)
				{
					line = string.Format("{0}{1}", line, (firstAdded) ? ", " : "");
					firstAdded = true;
					line = string.Concat(line, technology.ReportDetails());
				}
			}
			return line;
		}

		private string reportInitiative(string line)
		{
			if (this.Initiative != 0)
			{
				line = string.Format("{0}initiative: {1}",
				   (line == string.Empty) ? string.Empty : string.Concat(line, ", "),
				   this.Initiative);
				if (this.Initiative != (this.InitiativeBonus - this.People.CombatInitiative(this.RootModuleStack)))
				{
					line = string.Format("{0} ({1})", 
						line,	
						this.InitiativeBonus - this.People.CombatInitiative(this.RootModuleStack));
				}
			}
			return line;
		}

		private string reportOwnHitPoints(string line)
		{
			line = string.Format("{0}hit points: {1}/{2}",
				(line == string.Empty) ? string.Empty : string.Concat(line, ", "),
				this.HitPoints,
				this.HitPoints - this.Damage);
			int capture = 0;
			foreach (Module module in this.modules)
			{
				capture += module.CaptureDamage;
			}
			if (capture > 0)
			{
				line = string.Format("{0}, capture: {1}", line, capture);
			}
			return line;
		}

		private string reportHitPoints(Faction faction, string line)
		{
			int totalHitPoints = this.HitPoints + this.ModuleStacks.HitPoints();
			line = string.Format("{0}hit points: {1}/{2}",
				(line == string.Empty) ? string.Empty : string.Concat(line, ", "),
				totalHitPoints,
				totalHitPoints - this.Damage - this.ModuleStacks.Damage());
			if (totalHitPoints > this.HitPoints)
			{
				line = string.Format("{0} ({1}/{2})",
					line,
					this.HitPoints,
					this.HitPoints - this.Damage);
			}
			int capture = this.CaptureDamage + this.ModuleStacks.CaptureDamage();
			if (capture > 0)
			{
				line = string.Format("{0}, capture: {1}", line, capture);
			}
			return line;
		}

		private string reportDefense(Faction faction, string line)
		{
			if (this.Defense + this.ModuleStacks.Defense() > 0)
			{
				line = string.Format("{0}defense: {1}",
				   (line == string.Empty) ? string.Empty : string.Concat(line, ", "),
				   this.Defense + this.ModuleStacks.Defense());
			}
			if (this.ModuleStacks.Defense() > 0)
			{
				line = string.Format("{0} ({1})", line, this.Defense);
			}
			return line;
		}

		private string reportAttack(Faction faction, string line)
		{
			if (this.Attack + this.ModuleStacks.Attack() > 0)
			{
				line = string.Format("{0}attack: {1}",
					(line == string.Empty) ? string.Empty : string.Concat(line, ", "),
					this.Attack + this.ModuleStacks.Attack());
			}
			if (this.ModuleStacks.Attack() > 0)
			{
				line = string.Format("{0} ({1})", line, this.Attack);
			}
			return line;
		}

		private string reportSize(string line)
		{
			return string.Format("{0}size: {1}", line, this.Size);
		}

		private string reportMass(Faction faction, string line)
		{
			if (this.owner == faction)
			{                
                if ( this.MassCapacity + this.ModuleStacks.MassCapacity > 0)
                {
                    line = string.Format("{0}, mass: {1}/{2}",
                        line,
                        this.MassCapacity + this.ModuleStacks.MassCapacity,
                        this.Mass);
                } else
                {
                    line = string.Format("{0}, mass: {1}",
                        line,
                        this.Mass);
                }

                if (this.Mass > this.ModuleStacks.Mass)
                {
                    line = string.Format("{0} ({1})", line, this.MassNetto);
                }
            } 
			return line;
		}

		private string reportEnergyUsage(Faction faction, string line, bool showConsume = true)
		{
			if (this.owner == faction || (this.ModuleStacks.Contains(faction)))
			{
				if (this.EnergyProduction + this.ModuleStacks.EnergyProduction() > 0)
				{
					line = string.Format("{0}, energy: {1}/{2}{3}",
						line,
						this.EnergyProduction + this.ModuleStacks.EnergyProduction(),
						this.EnergyRequired + this.ModuleStacks.EnergyRequired(),
                        (showConsume & this.ProduceEnergyConsume.Count > 0) ? string.Format(" (consume: {0} for {1} weeks)",
                            this.ProduceEnergyConsume.ReportList,
                            this.ModuleType.ProduceDuration) : string.Empty);
				}
				else if (this.EnergyRequired + this.ModuleStacks.EnergyRequired() > 0)
				{
					line = string.Format("{0}, energy: {1}",
						line,
						this.EnergyRequired + this.ModuleStacks.EnergyRequired());
				}

				if (this.ModuleStacks.EnergyRequired() > 0)
				{
					line = string.Format("{0} ({1})", line, this.EnergyRequired);
				}
			}
			return line;
		}

		private string reportCrew(Faction faction, string line)
		{
			if (this.owner == faction)
			{
				if (this.CrewRequired + this.ModuleStacks.CrewRequired() > 0)
				{
					line = string.Format("{0}, crew: {1}/{2}",
						line,
						this.CrewRequired + this.ModuleStacks.CrewRequired(),
						this.CrewCurrent + this.ModuleStacks.CrewCurrent());
				}
				if (this.CrewRequired + this.CrewCurrent > 0 & this.ModuleStacks.CrewRequired() > 0)
				{
					line = string.Format("{0} ({1}/{2})", line, this.CrewRequired, this.CrewCurrent);
				}
			}
			return line;
		}

		private string reportActive(string line)
		{
			if (line != string.Empty)
				line = string.Concat(line, ", ");
			if (!this.online)
			{
				line = string.Concat(line, "deactivated");
			}
			else if (this.IsActive)
			{
				line = string.Concat(line, "active");
			}
			else if (this.IsPartiallyDisabled)
			{
				line = string.Concat(line, "partially disabled");
			}
			else
			{
				line = string.Concat(line, "disabled");
			}
			return line;
		}

		public List<string> ReportOrdersTemplateHeader(Faction faction)
		{
			ReportLines reportLines = new ReportLines
            {
                this.ReportHeader(faction)
            };

			if (this.Technologies.Count > 0)
			{
				reportLines.Add(string.Concat("technologies: ", this.Technologies.ReportList, "."));
			}
			if (this.ItemStacks.Count > 0)
			{
				reportLines.Add(string.Concat("items: ", this.ItemStacks.ReportList, "."));
			}
			if (this.Effects.Count > 0)
			{
				foreach (string line in this.Effects.Report(faction))
				{
					reportLines.Add(line);
				}
			}

			return reportLines.IndentedLines;
		}

		# endregion
	}
}
