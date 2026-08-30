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
			this.battleDefender = defender;
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


		private bool canJoinAsAttacker(ModuleStack stack)
		{
			return stack != null && stack.IsArmed && stack.HasOperationalModules;
		}

		private ModuleStacks collectAttackers(ModuleStack initiator, ModuleStack target)
		{
			ModuleStacks side = new ModuleStacks();
			if (initiator.Location == null)
			{
				if (this.canJoinAsAttacker(initiator))
				{
					side.Add(initiator.Name, initiator);
				}
				return side;
			}

			foreach (ModuleStack stack in stacksAtLocation(initiator.Location))
			{
				if (!this.canJoinAsAttacker(stack))
				{
					continue;
				}
				if (!stack.IsRootModuleStack && stack != initiator)
				{
					continue;
				}
				if (this.joinsAttack(stack.Owner, initiator.Owner, target.Owner))
				{
					side.Add(stack.Name, stack);
				}
			}
			if (!side.Contains(initiator.Name) && this.canJoinAsAttacker(initiator))
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
				if (target.HasIntactModules())
				{
					side.Add(target.Name, target);
				}
				return side;
			}

			foreach (ModuleStack stack in stacksAtLocation(target.Location))
			{
				if (!stack.HasIntactModules())
				{
					continue;
				}
				if (stack.IsRootModuleStack)
				{
					if (this.joinsDefense(stack.Owner, initiator.Owner, target.Owner))
					{
						side.Add(stack.Name, stack);
					}
				}
				else if (this.isDefenderStack(stack, target, initiator))
				{
					ModuleStack root = stack.RootModuleStack;
					if (root != null
						&& root != stack
						&& root.HasIntactModules()
						&& this.joinsDefense(root.Owner, initiator.Owner, target.Owner))
					{
						continue;
					}
					if (this.joinsDefense(stack.Owner, initiator.Owner, target.Owner))
					{
						side.Add(stack.Name, stack);
					}
				}
			}
			if (!side.Contains(target.Name) && target.HasIntactModules())
			{
				side.Add(target.Name, target);
			}
			return side;
		}

		private bool isDefenderStack(ModuleStack stack, ModuleStack target, ModuleStack initiator)
		{
			if (stack == target)
			{
				return true;
			}
			if (stack.IsArmed && stack.RootModuleStack != initiator.RootModuleStack)
			{
				return true;
			}
			ModuleStack targetParent = target.Parent as ModuleStack;
			if (targetParent != null && stack.Parent == targetParent)
			{
				return true;
			}
			return false;
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

		private static IEnumerable<ModuleStack> stacksAtLocation(Location location)
		{
			if (location == null)
			{
				yield break;
			}

			foreach (ModuleStack stack in ModuleStack.All.Values)
			{
				if (stack.Location == location)
				{
					yield return stack;
				}
			}
		}

		public static string FactionPairKey(Faction a, Faction b)
		{
			if (a == null || b == null)
			{
				return string.Empty;
			}
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
			HashSet<string> factionPairs = new HashSet<string>();
			foreach (ModuleStack stack in ModuleStack.All.Values)
			{
				if (!stack.IsArmed || stack.Location == null || !stack.IsRootModuleStack)
				{
					continue;
				}
				if (!stack.HasOperationalModules)
				{
					continue;
				}
				foreach (ModuleStack other in stacksAtLocation(stack.Location))
				{
					if (other == stack)
					{
						continue;
					}
					if (!other.HasIntactModules())
					{
						continue;
					}
					if (stack.Owner.AttitudeTowardUnit(other) != FactionAttitude.Enemy)
					{
						continue;
					}
					string factionKey = string.Concat(
						stack.Location.Name,
						"|",
						FactionPairKey(stack.Owner, other.Owner));
					if (factionPairs.Contains(factionKey))
					{
						continue;
					}
					string key = PairKey(stack, other);
					if (paired.Contains(key))
					{
						continue;
					}
					paired.Add(key);
					factionPairs.Add(factionKey);
					started.Add(new Battle(stack, other));
				}
			}
			return started;
		}

		public static int HpDamageFromShot(int weaponDamage)
		{
			return (weaponDamage * 25) / 100;
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
		private ModuleStack battleDefender = null;
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
				targets = this.availableTargets(this.defenders, modulestack);
			}
			else
			{
				targets = this.availableTargets(this.attackers, modulestack);
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

			if (modulestack.HasPrioritizeArmed)
			{
				foreach (ModuleStack target in targets.Values)
				{
					if (target.IsArmed)
					{
						return target;
					}
				}
			}

			if (modulestack.HasPrioritizeCommand)
			{
				foreach (ModuleStack target in targets.Values)
				{
					if (target.IsCommandStack())
					{
						return target;
					}
				}
			}

			if (modulestack.HasPrioritizeCargo)
			{
				foreach (ModuleStack target in targets.Values)
				{
					if (target.IsCargoStack())
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
			if (this.defenders.Contains(moduleStack.Name))
			{
				this.defenders.Remove(moduleStack);
			}
		}

		private ModuleStacks availableTargets(ModuleStacks allTargets, ModuleStack attacker)
		{
			ModuleStacks filtered = new ModuleStacks();
			foreach (ModuleStack target in allTargets.Values)
			{
				if (!target.HasIntactModules())
				{
					continue;
				}

				bool includeDisabled = false;
				if (attacker.HasPrioritizeArmed && target.IsArmed)
				{
					includeDisabled = true;
				}
				if (attacker.HasPrioritizeCommand && target.IsCommandStack())
				{
					includeDisabled = true;
				}
				if (attacker.HasPrioritizeCargo && target.IsCargoStack())
				{
					includeDisabled = true;
				}

				if (!target.HasOperationalModules && !includeDisabled)
				{
					continue;
				}

				filtered.Add(target.Name, target);
			}
			return filtered;
		}

		private Dictionary<ModuleStack, int> unhitRounds = new Dictionary<ModuleStack, int>();
		private List<ModuleStack> hitThisRound = new List<ModuleStack>();
		private List<Module> pendingCaptures = new List<Module>();
		private List<ModuleStack> launchedHangarCraft = new List<ModuleStack>();
		private List<ModuleStack> hangarLaunchCarriers = new List<ModuleStack>();

		private void executeAttack(ModuleStack modulestack)
		{
			string line;
			if (this.round == 1 && this.launchedHangarCraft.Contains(modulestack))
			{
				return;
			}
			if (this.round == 1 && this.hangarLaunchCarriers.Contains(modulestack))
			{
				return;
			}
			if (!modulestack.IsArmed)
			{
				return;
			}
			else
			{
				ModuleStack target = this.findTarget(modulestack);
				if (target == null)
				{
					return;
				}
				ETactic firing = modulestack.FiringTactic;
				Modules firingModules = modulestack.GetFiringModules();
				int remainingItemShots = modulestack.ItemStacks.CombatDamageShotBudget(
					modulestack.ModuleType.Group,
					modulestack.QuantityActive);
				foreach (Module module in firingModules)
				{
					if (!this.defenders.Contains(target.Name) && !this.attackers.Contains(target.Name))
					{
						break;
					}
					string weaponName = module.Parent.ModuleType != null
						? module.Parent.ModuleType.ReportName
						: module.Parent.ReportName;
					line = string.Concat(
						modulestack.ReportName,
						" fires ",
						weaponName,
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
						bool targetWasOperational = target.HasOperationalModules;
						Module targetModule = this.resolveHitLocation(target, firing, target.HasEvade);
						if (targetModule == null)
						{
							continue;
						}

						int weaponDamage = module.Parent.ModuleShotDamage();
						if (module.Parent.RootModuleStack == modulestack)
						{
							weaponDamage += modulestack.ItemStacks.CombatDamageBonusForShot(
								modulestack.ModuleType.Group,
								ref remainingItemShots);
						}
						weaponDamage = this.applyShieldIntercept(target, weaponDamage);
						int hpDamage = weaponDamage;
						int captureDamage = 0;
						if (firing == ETactic.capture)
						{
							if (CombatMatchup.IsArmor(targetModule.Parent.ModuleType))
							{
								hpDamage = 0;
								captureDamage = 0;
							}
							else
							{
								hpDamage = HpDamageFromShot(weaponDamage);
								captureDamage = CaptureDamageFromShot(weaponDamage);
							}
						}
						int poolRemaining = targetModule.HitPoints - targetModule.Damage - targetModule.CaptureDamage;
						if (poolRemaining < 0)
						{
							poolRemaining = 0;
						}
						if (hpDamage > poolRemaining)
						{
							hpDamage = poolRemaining;
						}
						poolRemaining -= hpDamage;
						if (captureDamage > poolRemaining)
						{
							captureDamage = poolRemaining;
						}
						if (captureDamage < 0)
						{
							captureDamage = 0;
						}
						targetModule.Damage += hpDamage;
						targetModule.CaptureDamage += captureDamage;

						line = this.formatHitLine(line, targetModule, hpDamage, captureDamage);
						this.report(line);
						this.reportObserver(string.Format("{0} fires {1} on {2} and hits {3} {4}.",
							modulestack.ReportName,
							weaponName,
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

								if (targetWasOperational && !target.HasOperationalModules)
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
							weaponName,
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

		private string formatHitLine(string line, Module targetModule, int hpDamage, int captureDamage)
		{
			if (captureDamage > 0)
			{
				return string.Format("{0} hits {1} {2} doing {3} damage and {4} capture damage.",
					line,
					targetModule.ReportID,
					targetModule.Parent.ReportName,
					hpDamage,
					captureDamage);
			}
			return string.Format("{0} hits {1} {2} doing {3} damage.",
				line,
				targetModule.ReportID,
				targetModule.Parent.ReportName,
				hpDamage);
		}

		private int getRoll(int dice, string description)
		{
			int maximum = dice + 1;
			int roll = Sequence.GenerateRandomInt(1, maximum, description);
			if (roll >= maximum || roll < 1)
			{
				int range = maximum - 1;
				if (range > 0)
				{
					roll = 1 + (Math.Abs(roll) % range);
				}
				else
				{
					roll = 1;
				}
			}
			return roll;
		}

		private int getChance(ModuleStack modulestack, ModuleStack target, ETactic eTactic)
		{
			int chance = System.Convert.ToInt32((modulestack.Attack + modulestack.ModuleStacks.Attack()) / 2);
			if (target != null && !string.IsNullOrEmpty(modulestack.ModuleType != null ? modulestack.ModuleType.WeaponGroup : null))
			{
				string resists = target.ModuleType != null ? target.ModuleType.Resists : string.Empty;
				chance = System.Convert.ToInt32(Math.Ceiling(chance * CombatMatchup.ChanceMultiplier(modulestack.ModuleType.WeaponGroup, resists)));
			}
			if (target != null && target.HasEvade)
			{
				chance = chance / 2;
			}
			if (target != null && target.IsImmobile)
			{
				chance = chance + (chance / 2);
			}
			return chance;
		}

		private int applyShieldIntercept(ModuleStack target, int damage)
		{
			if (damage <= 0 || target == null)
			{
				return damage;
			}
			Module shield = this.findShieldModule(target);
			if (shield == null)
			{
				return damage;
			}
			int intercepted = CombatMatchup.ShieldIntercept(damage);
			int remainingHp = shield.HitPoints - shield.Damage;
			if (remainingHp < 0)
			{
				remainingHp = 0;
			}
			if (intercepted > remainingHp)
			{
				intercepted = remainingHp;
			}
			shield.Damage += intercepted;
			int remainder = damage - intercepted;
			return remainder < 0 ? 0 : remainder;
		}

		private Module findShieldModule(ModuleStack stack)
		{
			if (stack == null || stack.ModuleType == null)
			{
				return null;
			}
			if (CombatMatchup.IsShield(stack.ModuleType))
			{
				foreach (Module module in stack.Modules)
				{
					if (!module.IsWrecked)
					{
						return module;
					}
				}
			}
			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				Module found = this.findShieldModule(nested);
				if (found != null)
				{
					return found;
				}
			}
			return null;
		}

		private int intactModuleCount(ModuleStack moduleStack)
		{
			int count = 0;
			foreach (Module module in moduleStack.Modules)
			{
				if (!module.IsWrecked)
				{
					count++;
				}
			}
			return count;
		}

		private int hitWeight(ModuleStack moduleStack, ETactic firing, bool targetEvade)
		{
			int intactCount = this.intactModuleCount(moduleStack);
			if (intactCount < 1)
			{
				return 0;
			}
			int size = moduleStack.ModuleType.DamageCapacity * intactCount;
			if (CombatMatchup.IsArmor(moduleStack.ModuleType))
			{
				size = size * 5;
			}
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

		private int getDamageArea(ModuleStack moduleStack, ETactic firing, bool targetEvade, Faction targetOwner)
		{
			if (targetOwner != null && moduleStack.Owner != targetOwner)
			{
				return 0;
			}
            int totalArea = this.hitWeight(moduleStack, firing, targetEvade);
            foreach (ModuleStack subModuleStack in moduleStack.ModuleStacks.Values)
            {
				totalArea += this.getDamageArea(subModuleStack, firing, targetEvade, targetOwner);
            }
			return totalArea;
        }

		private Module getModule(ModuleStack moduleStack, int target, ETactic firing, bool targetEvade, Faction targetOwner)
		{
			if (targetOwner != null && moduleStack.Owner != targetOwner)
			{
				return null;
			}
			Module targetModule = null;
			int perModule = 0;
			int intactCount = this.intactModuleCount(moduleStack);
			if (intactCount > 0)
			{
				perModule = this.hitWeight(moduleStack, firing, targetEvade) / intactCount;
			}
			foreach (Module module in moduleStack.Modules)
			{
				if (module.IsWrecked)
				{
					continue;
				}
				target -= perModule > 0 ? perModule : moduleStack.ModuleType.DamageCapacity;
				if (target <= 0)
				{
                    targetModule = module;
					break;
				}
			}
			if (targetModule == null)
			{
				foreach (ModuleStack subModuleStack in moduleStack.ModuleStacks.Values)
				{
					if (targetModule == null)
					{
						targetModule = this.getModule(subModuleStack, target, firing, targetEvade, targetOwner);
					}
					if (targetModule == null)
					{
						target -= getDamageArea(subModuleStack, firing, targetEvade, targetOwner);
					}
				}
			}
			return targetModule;
		}

        private Module resolveHitLocation(ModuleStack moduleStack, ETactic eTactic, bool targetEvade)
		{
			Faction targetOwner = moduleStack.Owner;
			int damageArea = this.getDamageArea(moduleStack, eTactic, targetEvade, targetOwner);
			if (damageArea < 1)
			{
				damageArea = 1;
			}
            int roll = this.getRoll(damageArea, string.Concat("Hit location 1 to ", damageArea.ToString()));
			return this.getModule(moduleStack, roll, eTactic, targetEvade, targetOwner);
		}

		public void executeMovement()
		{

		}

		private bool concluded;

		public void Execute(int week)
		{
			this.concluded = false;
			this.week = week;
			this.resetCaptureDamage();

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
				if (this.round == 1)
				{
					this.launchHangarCraft();
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
						if (modulestack.HasOperationalModules)
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

		private void launchHangarCraft()
		{
			Location location = null;
			if (this.attacker != null)
			{
				location = this.attacker.Location;
			}
			if (location == null && this.battleDefender != null)
			{
				location = this.battleDefender.Location;
			}
			if (location == null)
			{
				return;
			}

			List<ModuleStack> toLaunch = new List<ModuleStack>();
			foreach (ModuleStack stack in stacksAtLocation(location))
			{
				if (!stack.IsHangarCraft)
				{
					continue;
				}
				ModuleStack parent = stack.Parent as ModuleStack;
				if (parent == null || parent.ModuleType == null || !parent.ModuleType.IsDroneBay)
				{
					continue;
				}
				toLaunch.Add(stack);
			}

			foreach (ModuleStack craft in toLaunch)
			{
				this.addLaunchedHangarCraft(craft);
			}
		}

		private void addLaunchedHangarCraft(ModuleStack craft)
		{
			ModuleStack root = craft.RootModuleStack;
			bool onAttack = Battle.sideContains(this.attackers, craft) || Battle.sideContains(this.attackers, root);
			bool onDefense = Battle.sideContains(this.defenders, craft) || Battle.sideContains(this.defenders, root);

			ModuleStack bay = craft.Parent as ModuleStack;
			ModuleStack carrier = craft.RootModuleStack;
			string bayName = bay != null && bay.ModuleType != null
				? bay.ModuleType.ReportName
				: "hangar";
			string carrierName = carrier != null ? carrier.ReportName : craft.ReportName;

			craft.Parent = craft.Location;
			if (!this.launchedHangarCraft.Contains(craft))
			{
				this.launchedHangarCraft.Add(craft);
			}
			if (carrier != null && !this.hangarLaunchCarriers.Contains(carrier))
			{
				this.hangarLaunchCarriers.Add(carrier);
			}
			this.report(string.Format("{0} launches {1} from {2}.", carrierName, craft.ReportName, bayName));

			if (Battle.sideContains(this.attackers, craft) || Battle.sideContains(this.defenders, craft))
			{
				return;
			}
			if (onAttack && !onDefense)
			{
				this.attackers.Add(craft);
			}
			else if (onDefense)
			{
				this.defenders.Add(craft);
			}
		}

		private static bool sideContains(ModuleStacks side, ModuleStack stack)
		{
			return side != null && stack != null && side.Contains(stack.Name);
		}

		private void resetCaptureDamage()
		{
			this.resetCaptureDamage(this.attackers);
			this.resetCaptureDamage(this.defenders);
		}

		private void resetCaptureDamage(ModuleStacks stacks)
		{
			if (stacks == null)
			{
				return;
			}
			foreach (ModuleStack stack in stacks.Values)
			{
				this.resetCaptureDamage(stack);
			}
		}

		private void resetCaptureDamage(ModuleStack stack)
		{
			if (stack == null)
			{
				return;
			}
			foreach (Module module in stack.Modules)
			{
				module.CaptureDamage = 0;
			}
			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				this.resetCaptureDamage(nested);
			}
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
			this.applyCompleteCaptures(this.attackers, week);
			this.applyCompleteCaptures(this.defenders, week);
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

		private void applyCompleteCaptures(ModuleStacks stacks, int week)
		{
			if (stacks == null)
			{
				return;
			}
			List<ModuleStack> stackList = new List<ModuleStack>(stacks.Values);
			foreach (ModuleStack stack in stackList)
			{
				this.applyCompleteCaptures(stack, week);
			}
		}

		private void applyCompleteCaptures(ModuleStack stack, int week)
		{
			List<Module> modules = new List<Module>(stack.Modules);
			foreach (Module module in modules)
			{
				if (module.IsWrecked || !module.IsCaptureComplete)
				{
					continue;
				}
				this.transferCapturedModule(module, week);
			}
			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				this.applyCompleteCaptures(nested, week);
			}
		}

		private void transferCapturedModule(Module module, int week)
		{
			ModuleStack source = module.Parent;
			int originalCount = source.Quantity;
			Faction originalOwner = source.Owner;
			if (originalCount < 1)
			{
				return;
			}
			if (originalOwner == this.attacker.Owner)
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

			this.transferProportionalItems(source, captured, 1, originalCount, source.ModuleType.Group == EModuleTypesGroup.command);
			this.transferProportionalNested(source, captured, 1, originalCount);
			this.transferProportionalPeople(source, captured, 1, originalCount);

			source.EventReports.Add(week, string.Format("lost {0} to capture.", module.ReportName));
			captured.EventReports.Add(week, string.Format("captured {0} from {1}.", captured.ModuleType.ReportName, source.ReportName));

			if (source.ModuleType.Group == EModuleTypesGroup.command)
			{
				ModuleStack root = source.RootModuleStack;
				if (root.Owner == originalOwner
					&& this.countCommandModules(root, originalOwner) == 0)
				{
					this.captureParentByOwnership(root);
				}
			}

			if (source.Modules.Count == 0 || source.Quantity < 1)
			{
				this.removeFromBattle(source);
			}
		}

		private int countCommandModules(ModuleStack stack, Faction owner)
		{
			int count = 0;
			if (stack.Owner == owner && stack.ModuleType != null
				&& stack.ModuleType.Group == EModuleTypesGroup.command)
			{
				count += stack.Modules.Count;
			}
			foreach (ModuleStack child in stack.ModuleStacks.Values)
			{
				count += this.countCommandModules(child, owner);
			}
			return count;
		}

		private void captureParentByOwnership(ModuleStack root)
		{
			this.changeOwnerRecursive(root, this.attacker.Owner);
			string line = string.Format("  {0} captured by {1}.",
				root.ReportName,
				this.attacker.Owner.ReportName);
			this.report(line);
			this.reportObserver(line);
			this.removeFromBattle(root);
		}

		private void changeOwnerRecursive(ModuleStack stack, Faction owner)
		{
			stack.Owner = owner;
			foreach (Person person in stack.People.Values)
			{
				person.Owner = owner;
			}
			foreach (ModuleStack child in stack.ModuleStacks.Values)
			{
				this.changeOwnerRecursive(child, owner);
			}
		}

		private string capturedStackName(ModuleStack source)
		{
			int serial = 1;
			string name = this.formatCapturedName(serial);
			while (ModuleStack.All.ContainsKey(name))
			{
				serial++;
				name = this.formatCapturedName(serial);
			}
			return name;
		}

		private string formatCapturedName(int serial)
		{
			int digits = NamedObject.MaxNameLength - 1;
			return string.Concat("c", serial.ToString().PadLeft(digits, '0'));
		}

		private void transferProportionalItems(ModuleStack source, ModuleStack dest, int taken, int originalCount, bool crewCasualties)
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
				if (crewCasualties && itemStack.ItemType.Group == EItemTypesGroup.crew)
				{
					this.transferCrewCasualties(source, dest, itemStack.ItemType, move);
					continue;
				}
				source.ItemStacks.Remove(new ItemStack(itemStack.ItemType, move));
				dest.ItemStacks.Add(new ItemStack(itemStack.ItemType, move));
			}
		}

		private void transferCrewCasualties(ModuleStack source, ModuleStack dest, ItemType crewType, int move)
		{
			int killed = move / 4;
			int wounded = move / 2;
			int healthy = move - killed - wounded;
			source.ItemStacks.Remove(new ItemStack(crewType, move));
			if (killed > 0)
			{
				this.report(string.Format("  {0} killed.", new ItemStack(crewType, killed).ReportName));
			}
			if (wounded > 0)
			{
				ItemType woundedType = ItemType.All.ContainsKey("wndtrn") ? ItemType.All["wndtrn"] : crewType;
				dest.ItemStacks.Add(new ItemStack(woundedType, wounded));
				this.report(string.Format("  {0} wounded.", new ItemStack(crewType, wounded).ReportName));
			}
			if (healthy > 0)
			{
				dest.ItemStacks.Add(new ItemStack(crewType, healthy));
				this.report(string.Format("  {0} captured.", new ItemStack(crewType, healthy).ReportName));
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
			toMove.Sort(People.CompareByNames);

			int killed = moveCount / 4;
			int wounded = moveCount / 2;
			Race woundedRace = Race.All.ContainsKey("wndtrn") ? Race.All["wndtrn"] : null;
			int index = 0;
			foreach (Person person in toMove)
			{
				string personName = person.ReportName;
				if (index < killed)
				{
					this.report(string.Format("  {0} is killed.", personName));
					Person.All.Remove(person);
				}
				else
				{
					person.Parent = dest;
					person.Owner = this.attacker.Owner;
					if (index < killed + wounded && woundedRace != null)
					{
						person.Race = woundedRace;
						this.report(string.Format("  {0} is wounded.", personName));
					}
					else
					{
						this.report(string.Format("  {0} is captured.", personName));
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
			if (!modulestack.IsAvoiding)
			{
				return;
			}

			string line;
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
