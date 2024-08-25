using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Battle : IReporting
	{
		private int week = 0;
		public int Week
		{
			get { return this.week; }
			set { this.week = value; }
		}

		public static Battles All = new Battles();

		private Battle()
		{
			Battle.All.Add(this);
		}

		public Battle(ModuleStack attacker, ModuleStack defender)
			: this()
		{
			this.attacker = attacker;
			this.attackers = this.findAllies(attacker); 
			this.defenders = this.findAllies(defender);

			this.participants = this.findParticipants();

			this.week = 1;
			// mark all ATTACK orders executed
		}

		public Battle(ModuleStack attacker, Faction defenders)
			: this()
		{
			this.defenders = this.findAllies(defenders);
			this.attackers = this.findAllies(attacker);
			
			this.round = 1;
			this.week = 1;

			this.participants = this.findParticipants();
		}


		private List<Faction> findParticipants()
		{
			List<Faction> factions = new List<Faction>();
			foreach (Faction faction in this.attackers.Owners)
			{
				if (!factions.Contains(faction))
				{
					factions.Add(faction);
				}
			}
			foreach (Faction faction in this.defenders.Owners)
			{
				if (!factions.Contains(faction))
				{
					factions.Add(faction);
				}
			}
			return factions;
		}


 		private ModuleStacks findAllies(ModuleStack moduleStack)
		{
			ModuleStacks allies = new ModuleStacks();
			allies = moduleStack.Location.ModuleStacks[moduleStack.Owner];
			allies.RemoveAvoiding();
			return allies;
		}

		private ModuleStacks findAllies(ModuleStacks moduleStacks)
		{
			ModuleStacks allies = new ModuleStacks();
			foreach (ModuleStack moduleStack in moduleStacks.Values)
			{
				ModuleStacks allies2 = this.findAllies(moduleStack);
				foreach (ModuleStack moduleStack2 in allies2.Values)
				{
					if (this.attackers.Contains(moduleStack2.Name))
						continue;
					if (this.defenders.Contains(moduleStack2.Name))
						continue;
					if (allies.Contains(moduleStack2.Name))
						continue;
					allies.Add(moduleStack2.Name, moduleStack2);
				}
			}
			return allies;
		}

		private ModuleStacks findAllies(Faction faction)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		private int round = 1;
		public int Round
		{
			get { return this.round; }
			set { this.round = value; }
		}

		private ModuleStack attacker = null;
		public ModuleStack Attacker
		{
			get { return this.attacker; }
			set { this.attacker = value; }
		}

		private ModuleStacks attackers = null;
		public ModuleStacks Attackers
		{
			get { return this.attackers; }
			set { this.attackers = value; }
		}

		private ModuleStacks defenders = null;
		public ModuleStacks Defenders
		{
			get { return this.defenders; }
			set { this.defenders = value; }
		}

		private List<Faction> participants = new List<Faction>();
		
		private bool canAttack()
		{
			return true;
		}

		private ModuleStack findTarget(ModuleStack modulestack)
		{
			ModuleStacks targets;
			if (this.attackers.Contains(modulestack.Name))
			{
				targets = this.availableTargets(this.defenders);
			}
			else
			{
				targets = this.availableTargets(this.attackers);			
			}			

			ModuleStack bestTarget = null;
			foreach (ModuleStack target in targets.Values)
			{							
				bestTarget = target;
				break;
			}
			return bestTarget;
		}

		private void removeFromBattle(ModuleStack moduleStack)
		{
			if (this.attackers.Contains(moduleStack.Name))
			{
				this.attackers.Remove(moduleStack);
			}
			else
			{
				this.defenders.Remove(moduleStack);
			}			
		}

		private ModuleStacks availableTargets(ModuleStacks allTargets)
		{
			return allTargets;
		}

		private void executeAttack(ModuleStack modulestack)
		{
			string line;
			if (!modulestack.IsArmed)
			{
				line = string.Concat(modulestack.ReportName, " is unarmed and cannot attack.");
				this.report(line);		
			}
			else
			{
				ModuleStack target = this.findTarget(modulestack);
				Modules militaryModules = modulestack.GetModules(EModuleTypesGroup.military);
				foreach (Module module in militaryModules)
				{
					line = string.Concat(
						modulestack.ReportName,
						" fires ",
						module.Parent.ReportName,
						" on ",
						target.ReportName);

					// make an attack and report it
					int chance = this.getChance(modulestack, ETactic.disable);

					int dice = modulestack.Attack + modulestack.ModuleStacks.Attack() + target.Defense + target.ModuleStacks.Defense();
					int roll = this.getRoll(dice);
					line = string.Format("{0} (chance: {1}/{2}) and",
						line,
						chance,
						dice);

					if (roll <= chance)
					{
						// hit
						Module targetModule = this.getModule(target, roll, ETactic.disable);

						targetModule.Damage += module.Parent.ModuleType.Damage;
						line = string.Format("{0} hits {1} {2} doing {3} damage.",
							line,
							targetModule.ReportID,
							targetModule.Parent.ReportName,
							module.Parent.ModuleType.Damage);
						this.report(line);
						if (targetModule.DamageStatus != EDamageStatus.undamaged)
						{
							line = string.Format("  {0} is {1}.",
								targetModule.ReportName, 
								targetModule.ReportDamage);
							this.report(line);
						}
						if (!targetModule.IsActive)
						{
							line = string.Format("  {0} is {1}.",
								targetModule.ReportName, 
								targetModule.ReportActive);
							this.report(line);

							if (!target.IsActive)
							{
								this.removeFromBattle(target);
								switch (targetModule.Parent.ModuleType.Group)
								{
									case EModuleTypesGroup.command:
										line = string.Concat("  ", target.ReportName, " lost it's command module and disables.");
										this.report(line);
										break;
									case EModuleTypesGroup.energy:
										line = string.Concat("  ", target.ReportName, " lost it's power supply and disables.");
										this.report(line);
										break;
								}
							}
							if (!target.IsArmed & targetModule.Parent.ModuleType.Group == EModuleTypesGroup.military)
							{
								line = string.Concat("  ", target.ReportName, " lost it's military module and become unarmed.");
								this.report(line);
							}
							if (target.IsImmobile & targetModule.Parent.ModuleType.Group == EModuleTypesGroup.propulsion)
							{
								line = string.Concat("  ", target.ReportName, " lost it's military module and become immobile.");
								this.report(line);
							}							
						}						
					}
					else
					{
						// miss
						line = string.Format("{0} misses.", line);
						this.report(line);
					}
				}
			}
		}

		private int getRoll(int dice)
		{
			// TODO: seed for turn reruns;
			int roll = 0;
			if (Sequence.Ints.Count == 0)
			{

				Random random = new Random();
				roll = random.Next(dice) + 1;
			}
			else
			{
				roll = Sequence.Ints.Pop();
			}
			return roll;
		}

		private int getChance(ModuleStack modulestack, ETactic eTactic)
		{
			return System.Convert.ToInt32((modulestack.Attack + modulestack.ModuleStacks.Attack()) / 2);
		}

		private Module getModule(ModuleStack moduleStack, int location, ETactic eTactic)
		{
			return moduleStack.ModuleStacks["100023"].Modules[0];
		}

		public void executeMovement()
		{

		}

		private bool concluded;

		public void Execute(int week)
		{
			this.concluded = false;

			this.prepareBattleReports();

			this.attacker.EventReports.Add(week, string.Format("initiated battle against {0}", defenders.ReportList));
			this.attackers.AddEvent(week, "engaged in battle");
			this.defenders.AddEvent(week, "engaged in battle");

			this.round = 0;

			while (!this.concluded)
			{
				this.round++;

				this.report(string.Format ("Round {0}:", this.round));
				this.report("------------------------------------------------------------");
				
				foreach (Faction faction in this.participants) 
				{
					// attackers list
					this.report(faction, "Attackers:");
					foreach (ModuleStack modulestack in this.attackers.Values)
					{
						this.report(faction, modulestack.BattleReport(faction));
					}
					// defenders list
					this.report(faction, "Defenders:");
					foreach (ModuleStack modulestack in this.defenders.Values)
					{
						this.report(faction, modulestack.BattleReport(faction));
					}
				}
				this.report("------------------------------------------------------------");
				
				
				// decide superiorities and bonuses
				// validate participating modulestacks (remove destroyed, disabled)
				SortedList<int, List<ModuleStack>> sorted = new SortedList<int, List<ModuleStack>>();
				foreach (ModuleStack moduleStack in this.attackers.Values)
				{
					if (!sorted.ContainsKey(moduleStack.Initiative)) 
					{
						sorted.Add(moduleStack.Initiative, new List<ModuleStack>());
					}
					sorted[moduleStack.Initiative].Add(moduleStack);
				}
				foreach (ModuleStack moduleStack in this.defenders.Values)
				{
					if (!sorted.ContainsKey(moduleStack.Initiative))
					{
						sorted.Add(moduleStack.Initiative, new List<ModuleStack>());
					}
					sorted[moduleStack.Initiative].Add(moduleStack);
				}
				foreach (List<ModuleStack> modulestacks in sorted.Values)
				{
					// resolve individual modulestacks initiative
					foreach (ModuleStack modulestack in modulestacks)
					{
						if (modulestack.IsActive)
						{
							this.executeAttack(modulestack);
							this.considerRetreat(modulestack);
						}
					}
				}
				this.report("------------------------------------------------------------");
				// execute orders from battle sequence - mainly move, attack, activate, deactivate, 
				//   but also use of tactical technologies or personal equipment
				// resolve all movements
				if (this.round > 10 | this.attackers.Count == 0 | this.defenders.Count == 0)
				{
					this.concluded = true;
					if (this.attackers.Count > 0 & this.defenders.Count == 0)
					{
						this.report("Battle won by attackers.");
					}
					else if (this.attackers.Count == 0 & this.defenders.Count > 0)
					{
						this.report("Battle won by defenders.");
					}
					else
					{
						this.report("Battle ended indecisively.");					
					}
				}
			}
			// add experience
		}

		private void considerRetreat(ModuleStack modulestack)
		{
			string line;
			if (!modulestack.IsArmed | modulestack.IsAvoiding)
			{
				if (modulestack.IsImmobile)
				{
					line = string.Concat(modulestack.ReportName, " is immobile and cannot escape.");
				}
				else
				{
					line = string.Concat(modulestack.ReportName, " tries to escape but failed.");
				}
				this.report(line);
			}
		}

		#region IReporting Members

		private Dictionary<Faction, ReportLines> battleReports = new Dictionary<Faction, ReportLines>();

		private void prepareBattleReports()
		{
			foreach (Faction faction in this.participants)
			{
				if (!this.battleReports.ContainsKey(faction))
				{
					this.battleReports.Add(faction, new ReportLines());
				}
			}
		}

		private void report(Faction faction, string line)
		{
			this.battleReports[faction].Add(line, 1);
		}

		private void report(Faction faction, List<string> lines)
		{
			this.battleReports[faction].Add(lines, 1);
		}

		private void report(ModuleStacks moduleStacks, string line)
		{
			foreach (Faction faction in moduleStacks.Owners)
			{
				this.report(faction, line);
			}
		}

		private void report(string line)
		{
			foreach (Faction faction in this.battleReports.Keys)
			{
				this.report(faction, line);
			}
		}

		public List<string>  Report(Faction faction)
		{
			List<string> reportLines = new List<string>();

			reportLines.AddRange(this.reportHeader(faction));
			reportLines.AddRange(this.reportDetails(faction));

			return reportLines;
		}

		private List<string> reportDetails(Faction faction)
		{
			return this.battleReports[faction].IndentedLines;
		}

		private List<string> reportHeader(Faction faction)
		{
			List<string> lines = new List<string>();
			lines.Add("  Week 1.");
			lines.Add(string.Format("  Battle has commenced at {0}.", this.attacker.Location.BattleReportName));
			lines.Add("  ------------------------------------------------------------");
			return lines;
		}

		#endregion
	}
}
