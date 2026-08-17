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
			this.attackers = this.collectAttackers(attacker, defender);
			this.defenders = this.collectDefenders(attacker, defender);

			this.participants = this.findParticipants();

			this.week = 1;
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


		private ModuleStacks collectAttackers(ModuleStack initiator, ModuleStack target)
		{
			ModuleStacks side = new ModuleStacks();
			if (initiator.Location == null)
			{
				if (initiator.IsArmed)
				{
					side.Add(initiator.Name, initiator);
				}
				return side;
			}

			foreach (ModuleStack stack in initiator.Location.ModuleStacks.Values)
			{
				if (!stack.IsRootModuleStack || !stack.IsArmed)
				{
					continue;
				}
				if (this.joinsAttack(stack.Owner, initiator.Owner, target.Owner))
				{
					side.Add(stack.Name, stack);
				}
			}
			if (!side.Contains(initiator.Name) && initiator.IsArmed)
			{
				side.Add(initiator.Name, initiator);
			}
			return side;
		}

		private ModuleStacks collectDefenders(ModuleStack initiator, ModuleStack target)
		{
			ModuleStacks side = new ModuleStacks();
			if (target.Location == null)
			{
				side.Add(target.Name, target);
				return side;
			}

			foreach (ModuleStack stack in target.Location.ModuleStacks.Values)
			{
				if (!stack.IsRootModuleStack)
				{
					continue;
				}
				if (this.joinsDefense(stack.Owner, initiator.Owner, target.Owner))
				{
					side.Add(stack.Name, stack);
				}
			}
			if (!side.Contains(target.Name))
			{
				side.Add(target.Name, target);
			}
			return side;
		}

		private bool joinsAttack(Faction owner, Faction attackerOwner, Faction defenderOwner)
		{
			if (owner == null)
			{
				return false;
			}
			if (this.sitsOutBothSides(owner, attackerOwner, defenderOwner))
			{
				return false;
			}
			if (owner == attackerOwner)
			{
				return true;
			}
			return owner.AttitudeToward(attackerOwner) == FactionAttitude.Ally;
		}

		private bool joinsDefense(Faction owner, Faction attackerOwner, Faction defenderOwner)
		{
			if (owner == null)
			{
				return false;
			}
			if (this.sitsOutBothSides(owner, attackerOwner, defenderOwner))
			{
				return false;
			}
			if (owner == defenderOwner)
			{
				return true;
			}
			FactionAttitude towardDefender = owner.AttitudeToward(defenderOwner);
			return towardDefender == FactionAttitude.Ally || towardDefender == FactionAttitude.Friendly;
		}

		private bool sitsOutBothSides(Faction owner, Faction attackerOwner, Faction defenderOwner)
		{
			if (owner == attackerOwner || owner == defenderOwner)
			{
				return false;
			}
			bool wouldAttack = owner.AttitudeToward(attackerOwner) == FactionAttitude.Ally;
			FactionAttitude towardDefender = owner.AttitudeToward(defenderOwner);
			bool wouldDefend = towardDefender == FactionAttitude.Ally || towardDefender == FactionAttitude.Friendly;
			return wouldAttack && wouldDefend;
		}

		public static string PairKey(ModuleStack a, ModuleStack b)
		{
			if (string.CompareOrdinal(a.Name, b.Name) < 0)
			{
				return string.Concat(a.Name, "|", b.Name);
			}
			return string.Concat(b.Name, "|", a.Name);
		}

		public static List<Battle> StartAtLocations(int week)
		{
			List<Battle> started = new List<Battle>();
			HashSet<string> paired = new HashSet<string>();
			foreach (ModuleStack stack in ModuleStack.All.Values)
			{
				if (!stack.IsRootModuleStack || !stack.IsArmed || stack.Location == null)
				{
					continue;
				}
				foreach (ModuleStack other in stack.Location.ModuleStacks.Values)
				{
					if (other == stack || !other.IsRootModuleStack)
					{
						continue;
					}
					if (stack.Owner.AttitudeTowardUnit(other) != FactionAttitude.Enemy)
					{
						continue;
					}
					string key = PairKey(stack, other);
					if (paired.Contains(key))
					{
						continue;
					}
					paired.Add(key);
					started.Add(new Battle(stack, other));
				}
			}
			return started;
		}

		public static int HpDamageFromShot(int weaponDamage)
		{
			return weaponDamage / 10;
		}

		public static int CaptureDamageFromShot(int weaponDamage)
		{
			return weaponDamage - HpDamageFromShot(weaponDamage);
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

		public const int MaxRounds = 10;

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

			string preferred = modulestack.PreferredTargetName;
			if (!string.IsNullOrEmpty(preferred) && preferred != "all")
			{
				foreach (ModuleStack target in targets.Values)
				{
					if (target.Name == preferred)
					{
						return target;
					}
				}
			}

			ModuleStack firstArmed = null;
			ModuleStack firstAny = null;
			foreach (ModuleStack target in targets.Values)
			{
				if (firstAny == null)
				{
					firstAny = target;
				}
				if (target.IsArmed && firstArmed == null)
				{
					firstArmed = target;
				}
			}
			return firstArmed != null ? firstArmed : firstAny;
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

		private Dictionary<ModuleStack, int> unhitRounds = new Dictionary<ModuleStack, int>();
		private List<ModuleStack> hitThisRound = new List<ModuleStack>();
		private List<Module> pendingCaptures = new List<Module>();

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
				if (target == null)
				{
					return;
				}
				ETactic firing = modulestack.FiringTactic;
				Modules militaryModules = modulestack.GetModules(EModuleTypesGroup.military);
				foreach (Module module in militaryModules)
				{
					line = string.Concat(
						modulestack.ReportName,
						" fires ",
						module.Parent.ReportName,
						" on ",
						target.ReportName);

					int chance = this.getChance(modulestack, target, firing);

					int dice = modulestack.Attack + modulestack.ModuleStacks.Attack() + target.Defense + target.ModuleStacks.Defense();
					line = string.Format("{0} (chance: {1}/{2}) and",
						line,
						chance,
						dice);

                    int roll = this.getRoll(dice, line);

                    if (roll <= chance)
					{
						this.markHit(target);
						Module targetModule = this.resolveHitLocation(target, firing, target.HasEvade);
						if (targetModule == null)
						{
							continue;
						}

						int weaponDamage = module.Parent.ModuleType.Damage;
						int hpDamage = weaponDamage;
						int captureDamage = 0;
						if (firing == ETactic.capture)
						{
							hpDamage = HpDamageFromShot(weaponDamage);
							captureDamage = CaptureDamageFromShot(weaponDamage);
						}
						targetModule.Damage += hpDamage;
						targetModule.CaptureDamage += captureDamage;

						line = string.Format("{0} hits {1} {2} doing {3} damage.",
							line,
							targetModule.ReportID,
							targetModule.Parent.ReportName,
							hpDamage);
						this.report(line);
						this.reportObserver(string.Format("{0} fires {1} on {2} and hits {3} {4}.",
							modulestack.ReportName,
							module.Parent.ReportName,
							target.ReportName,
							targetModule.ReportID,
							targetModule.Parent.ReportName));

						if (targetModule.IsWrecked)
						{
							this.pendingCaptures.Remove(targetModule);
							line = string.Format("  {0} is wrecked.", targetModule.ReportName);
							this.report(line);
							this.reportObserver(line);
						}
						else if (firing == ETactic.capture && targetModule.IsCaptureComplete)
						{
							targetModule.Online = false;
							string capturedName = targetModule.ReportName;
							line = string.Format("  {0} module captured by {1}.",
								capturedName,
								this.attacker.Owner.ReportName);
							this.report(line);
							this.reportObserver(line);
							this.transferCapturedModule(targetModule, this.week);
						}
						else
						{
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
								this.reportObserver(string.Format("  {0} is {1}.",
									targetModule.ReportName,
									targetModule.ReportActive));

								if (!target.IsActive)
								{
									this.removeFromBattle(target);
									switch (targetModule.Parent.ModuleType.Group)
									{
										case EModuleTypesGroup.command:
											line = string.Concat("  ", target.ReportName, " lost it's command module and disables.");
											this.report(line);
											this.reportObserver(line);
											break;
										case EModuleTypesGroup.energy:
											line = string.Concat("  ", target.ReportName, " lost it's power supply and disables.");
											this.report(line);
											this.reportObserver(line);
											break;
									}
								}
								if (!target.IsArmed & targetModule.Parent.ModuleType.Group == EModuleTypesGroup.military)
								{
									line = string.Concat("  ", target.ReportName, " lost it's military module and become unarmed.");
									this.report(line);
									this.reportObserver(line);
								}
								if (target.IsImmobile & targetModule.Parent.ModuleType.Group == EModuleTypesGroup.propulsion)
								{
									line = string.Concat("  ", target.ReportName, " lost it's propulsion module and become immobile.");
									this.report(line);
									this.reportObserver(line);
								}							
							}
						}						
					}
					else
					{
						line = string.Format("{0} misses.", line);
						this.report(line);
						this.reportObserver(string.Format("{0} fires {1} on {2} and misses.",
							modulestack.ReportName,
							module.Parent.ReportName,
							target.ReportName));
					}
				}
			}
		}

		private void markHit(ModuleStack target)
		{
			if (!this.hitThisRound.Contains(target))
			{
				this.hitThisRound.Add(target);
			}
		}

		private int getRoll(int dice, string description)
		{
			return Sequence.GenerateRandomInt(1, dice + 1, description);
		}

		private int getChance(ModuleStack modulestack, ModuleStack target, ETactic eTactic)
		{
			int chance = System.Convert.ToInt32((modulestack.Attack + modulestack.ModuleStacks.Attack()) / 2);
			if (target != null && target.HasEvade)
			{
				chance = chance / 2;
			}
			return chance;
		}

		private int hitWeight(ModuleStack moduleStack, ETactic firing, bool targetEvade)
		{
			int size = moduleStack.ModuleType.DamageCapacity * moduleStack.Modules.Count;
			bool commandOrPropulsion = moduleStack.ModuleType.Group == EModuleTypesGroup.command
				|| moduleStack.ModuleType.Group == EModuleTypesGroup.propulsion;
			if (firing == ETactic.capture && commandOrPropulsion)
			{
				size = size * 2;
			}
			if (targetEvade && commandOrPropulsion)
			{
				size = size / 2;
			}
			return size;
		}

		private int getDamageArea(ModuleStack moduleStack, ETactic firing, bool targetEvade)
		{
            int totalArea = this.hitWeight(moduleStack, firing, targetEvade);
            foreach (ModuleStack subModuleStack in moduleStack.ModuleStacks.Values)
            {
				totalArea += this.getDamageArea(subModuleStack, firing, targetEvade);
            }
			return totalArea;
        }

		private Module getModule(ModuleStack moduleStack, int target, ETactic firing, bool targetEvade)
		{
			Module targetModule = null;
			int perModule = 0;
			if (moduleStack.Modules.Count > 0)
			{
				perModule = this.hitWeight(moduleStack, firing, targetEvade) / moduleStack.Modules.Count;
			}
			foreach (Module module in moduleStack.Modules)
			{
				target -= perModule > 0 ? perModule : moduleStack.ModuleType.DamageCapacity;
				if (target <= 0)
				{
                    targetModule = module;
				}
			}
			if (targetModule == null)
			{
				foreach (ModuleStack subModuleStack in moduleStack.ModuleStacks.Values)
				{
					if (targetModule == null)
					{
						targetModule = this.getModule(subModuleStack, target, firing, targetEvade);
					}
					if (targetModule == null)
					{
						target -= getDamageArea(subModuleStack, firing, targetEvade);
					}
				}
			}
			return targetModule;
		}

        private Module resolveHitLocation(ModuleStack moduleStack, ETactic eTactic, bool targetEvade)
		{
			int damageArea = this.getDamageArea(moduleStack, eTactic, targetEvade);
			if (damageArea < 1)
			{
				damageArea = 1;
			}
            int roll = Sequence.GenerateRandomInt(0, damageArea, string.Concat("Hit location 0 to ", damageArea.ToString()));
			return this.getModule(moduleStack, roll, eTactic, targetEvade);
		}

		public void executeMovement()
		{

		}

		private bool concluded;

		public void Execute(int week)
		{
			this.concluded = false;
			this.week = week;

			this.prepareBattleReports();
			this.announceLocationEvent(week);

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
				this.hitThisRound.Clear();
				
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
				this.processEvadeLeaves();
				// execute orders from battle sequence - mainly move, attack, activate, deactivate, 
				//   but also use of tactical technologies or personal equipment
				// resolve all movements
				if (this.round >= Battle.MaxRounds | this.attackers.Count == 0 | this.defenders.Count == 0)
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
			this.ApplyCaptures(week);
		}

		private void processEvadeLeaves()
		{
			List<ModuleStack> combatants = new List<ModuleStack>();
			foreach (ModuleStack stack in this.attackers.Values)
			{
				combatants.Add(stack);
			}
			foreach (ModuleStack stack in this.defenders.Values)
			{
				combatants.Add(stack);
			}
			foreach (ModuleStack stack in combatants)
			{
				if (!stack.HasEvade)
				{
					continue;
				}
				if (this.hitThisRound.Contains(stack))
				{
					this.unhitRounds[stack] = 0;
					continue;
				}
				int count = 0;
				if (this.unhitRounds.ContainsKey(stack))
				{
					count = this.unhitRounds[stack];
				}
				count++;
				this.unhitRounds[stack] = count;
				if (count >= 2)
				{
					string line = string.Concat(stack.ReportName, " evades and leaves combat.");
					this.report(line);
					this.removeFromBattle(stack);
				}
			}
		}

		public void ApplyCaptures(int week)
		{
			List<Module> captures = new List<Module>(this.pendingCaptures);
			foreach (Module module in captures)
			{
				if (module.IsWrecked || !module.IsCaptureComplete)
				{
					continue;
				}
				this.transferCapturedModule(module, week);
			}
			this.pendingCaptures.Clear();
		}

		private void transferCapturedModule(Module module, int week)
		{
			ModuleStack source = module.Parent;
			int originalCount = source.Quantity;
			if (originalCount < 1)
			{
				return;
			}

			int index = source.Modules.IndexOf(module);
			if (index < 0)
			{
				return;
			}

			ModuleStack captured = new ModuleStack(source.Parent, this.attacker.Owner, source.ModuleType, this.capturedStackName(source));
			source.RemoveModule(index);
			captured.AddModule(module);

			this.transferProportionalItems(source, captured, 1, originalCount);
			this.transferProportionalNested(source, captured, 1, originalCount);
			this.transferProportionalPeople(source, captured, 1, originalCount);

			source.EventReports.Add(week, string.Format("lost {0} to capture.", module.ReportName));
			captured.EventReports.Add(week, string.Format("captured {0} from {1}.", captured.ModuleType.ReportName, source.ReportName));
		}

		private string capturedStackName(ModuleStack source)
		{
			string name = string.Concat("c", source.Name);
			int suffix = 0;
			while (ModuleStack.All.ContainsKey(name))
			{
				suffix++;
				name = string.Concat("c", suffix.ToString(), source.Name);
			}
			return name;
		}

		private void transferProportionalItems(ModuleStack source, ModuleStack dest, int taken, int originalCount)
		{
			List<ItemStack> snapshot = new List<ItemStack>();
			foreach (ItemStack itemStack in source.ItemStacks.Values)
			{
				snapshot.Add(itemStack);
			}
			foreach (ItemStack itemStack in snapshot)
			{
				int move = (itemStack.Quantity * taken) / originalCount;
				if (move <= 0)
				{
					continue;
				}
				source.ItemStacks.Remove(new ItemStack(itemStack.ItemType, move));
				dest.ItemStacks.Add(new ItemStack(itemStack.ItemType, move));
			}
		}

		private void transferProportionalNested(ModuleStack source, ModuleStack dest, int taken, int originalCount)
		{
			List<ModuleStack> nested = new List<ModuleStack>();
			foreach (ModuleStack child in source.ModuleStacks.Values)
			{
				nested.Add(child);
			}
			foreach (ModuleStack child in nested)
			{
				int moveCount = (child.Quantity * taken) / originalCount;
				if (moveCount <= 0)
				{
					continue;
				}
				ModuleStack moved = new ModuleStack(dest, this.attacker.Owner, child.ModuleType);
				for (int i = 0; i < moveCount; i++)
				{
					if (child.Quantity == 0)
					{
						break;
					}
					Module nestedModule = child.RemoveModule(0);
					moved.AddModule(nestedModule);
				}
			}
		}

		private void transferProportionalPeople(ModuleStack source, ModuleStack dest, int taken, int originalCount)
		{
			People people = source.People;
			int moveCount = (people.Count * taken) / originalCount;
			if (moveCount <= 0)
			{
				return;
			}

			List<Person> toMove = new List<Person>();
			foreach (Person person in people.Values)
			{
				if (toMove.Count >= moveCount)
				{
					break;
				}
				toMove.Add(person);
			}

			int killed = moveCount / 4;
			int wounded = moveCount / 2;
			Race woundedRace = Race.All.ContainsKey("wndtrn") ? Race.All["wndtrn"] : null;
			int index = 0;
			foreach (Person person in toMove)
			{
				if (index < killed)
				{
					Person.All.Remove(person);
				}
				else
				{
					person.Parent = dest;
					person.Owner = this.attacker.Owner;
					if (index < killed + wounded && woundedRace != null)
					{
						person.Race = woundedRace;
					}
				}
				index++;
			}
		}

		private void announceLocationEvent(int week)
		{
			if (this.attacker == null || this.attacker.Location == null)
			{
				return;
			}
			Location location = this.attacker.Location;
			string attackers = this.factionList(this.attackers);
			string defenders = this.factionList(this.defenders);
			string line = string.Format(
				"battle at {0}, attackers: {1}, defenders: {2}.",
				location.ReportName,
				attackers,
				defenders);
			foreach (Faction faction in Faction.All.Values)
			{
				if (location.HasPresence(faction))
				{
					faction.EventReports.Add(week, line);
				}
			}
		}

		private string factionList(ModuleStacks stacks)
		{
			List<string> names = new List<string>();
			foreach (Faction faction in stacks.Owners)
			{
				names.Add(faction.ReportName);
			}
			return string.Join(", ", names.ToArray());
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
		private Dictionary<Faction, ReportLines> observerReports = new Dictionary<Faction, ReportLines>();

		private void prepareBattleReports()
		{
			foreach (Faction faction in this.participants)
			{
				if (!this.battleReports.ContainsKey(faction))
				{
					this.battleReports.Add(faction, new ReportLines());
				}
			}
			if (this.attacker != null && this.attacker.Location != null)
			{
				foreach (Faction faction in Faction.All.Values)
				{
					if (this.participants.Contains(faction))
					{
						continue;
					}
					if (this.attacker.Location.HasPresence(faction) && !this.observerReports.ContainsKey(faction))
					{
						this.observerReports.Add(faction, new ReportLines());
					}
				}
			}
		}

		private void report(Faction faction, string line)
		{
			if (this.battleReports.ContainsKey(faction))
			{
				this.battleReports[faction].Add(line, 1);
			}
		}

		private void report(Faction faction, List<string> lines)
		{
			if (this.battleReports.ContainsKey(faction))
			{
				this.battleReports[faction].Add(lines, 1);
			}
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

		private void reportObserver(string line)
		{
			foreach (Faction faction in this.observerReports.Keys)
			{
				this.observerReports[faction].Add(line, 1);
			}
		}

		private void reportPublic(string line)
		{
			this.report(line);
			this.reportObserver(line);
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
			if (this.battleReports.ContainsKey(faction))
			{
				return this.battleReports[faction].IndentedLines;
			}
			if (this.observerReports.ContainsKey(faction))
			{
				return this.observerReports[faction].IndentedLines;
			}
			return new List<string>();
		}

		private List<string> reportHeader(Faction faction)
		{
			List<string> lines = new List<string>
            {
                string.Format("  Week {0}.", this.week),
                string.Format("  Battle has commenced at {0}.", this.attacker.Location.BattleReportName),
                "  ------------------------------------------------------------"
            };
			return lines;
		}

		#endregion
	}
}
