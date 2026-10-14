using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public partial class Battle
	{
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
				bool compositeFire = modulestack.UsesCompositeBattleFireReporting();
				bool pluralVolley = modulestack.UsesPluralBattleFireOpening() && !compositeFire;
				foreach (Module module in firingModules)
				{
					if (!this.defenders.Contains(target.Name) && !this.attackers.Contains(target.Name))
					{
						break;
					}
					string weaponName = module.Parent.ModuleType != null
						? module.Parent.ModuleType.ReportName
						: module.Parent.ReportName;
					string fireOpening = modulestack.FormatBattleFireOpening(target, weaponName);

					int chance = this.getChance(modulestack, target, firing);

					int dice = modulestack.Attack + modulestack.ModuleStacks.Attack() + target.Defense + target.ModuleStacks.Defense();
					if (pluralVolley)
					{
						if (!this.pluralFireHeadersThisRound.Contains(modulestack.Name))
						{
							this.report(string.Concat(fireOpening, "."));
							this.pluralFireHeadersThisRound.Add(modulestack.Name);
						}
						line = string.Format("(chance: {0}/{1}) and", chance, dice);
					}
					else
					{
						line = string.Format("{0} (chance: {1}/{2}) and",
							fireOpening,
							chance,
							dice);
					}

                    int roll = this.getRoll(dice, line);

					if (roll <= chance)
					{
						this.markHit(target);
						bool targetWasOperational = target.HasOperationalModules;
						Module targetModule = this.resolveHitLocation(modulestack, target, firing, target.HasEvade);
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
						this.reportObserver(this.formatObserverHitLine(
							modulestack,
							target,
							weaponName,
							targetModule,
							compositeFire));

						if (targetModule.IsWrecked)
						{
							this.pendingCaptures.Remove(targetModule);
							line = string.Format("  {0} is wrecked.", targetModule.ReportBattleStatusName);
							this.report(line);
							this.reportObserver(line);
							if (!target.HasIntactModules())
							{
								Contract.All.NotifyStackDestroyed(this.attacker.Owner, target);
							}
						}
						else if (firing == ETactic.capture && targetModule.IsCaptureComplete)
						{
							targetModule.Online = false;
							string capturedName = targetModule.ReportBattleStatusName;
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
									targetModule.ReportBattleStatusName, 
									targetModule.ReportDamage);
								this.report(line);
							}
							if (!targetModule.IsActive)
							{
								line = string.Format("  {0} is {1}.",
									targetModule.ReportBattleStatusName, 
									targetModule.ReportActive);
								this.report(line);
								this.reportObserver(string.Format("  {0} is {1}.",
									targetModule.ReportBattleStatusName,
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
						this.reportObserver(this.formatObserverMissLine(
							modulestack,
							target,
							weaponName,
							compositeFire));
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
			bool simplifyHitLocation = targetModule.Parent != null
				&& targetModule.Parent.UsesSimplifiedHitLocationReportingFor(targetModule);
			if (captureDamage > 0)
			{
				if (simplifyHitLocation)
				{
					return string.Format("{0} hits doing {1} damage and {2} capture damage.",
						line,
						hpDamage,
						captureDamage);
				}
				return string.Format("{0} hits {1} doing {2} damage and {3} capture damage.",
					line,
					targetModule.ReportHitLocationLabel,
					hpDamage,
					captureDamage);
			}
			if (simplifyHitLocation)
			{
				return string.Format("{0} hits doing {1} damage.",
					line,
					hpDamage);
			}
			return string.Format("{0} hits {1} doing {2} damage.",
				line,
				targetModule.ReportHitLocationLabel,
				hpDamage);
		}

		private string formatObserverHitLine(
			ModuleStack modulestack,
			ModuleStack target,
			string weaponName,
			Module targetModule,
			bool compositeFire)
		{
			if (compositeFire)
			{
				if (target.UsesSimplifiedHitLocationReportingFor(targetModule))
				{
					return string.Format("{0} fires {1} on {2} and hits.",
						modulestack.ReportName,
						weaponName,
						target.ReportName);
				}
				return string.Format("{0} fires {1} on {2} and hits {3}.",
					modulestack.ReportName,
					weaponName,
					target.ReportName,
					targetModule.ReportHitLocationLabel);
			}
			if (target.UsesSimplifiedHitLocationReportingFor(targetModule))
			{
				return string.Format("{0} {1} and hits.",
					modulestack.BattleFireSubjectVerbOn(),
					target.ReportName);
			}
			return string.Format("{0} {1} and hits {2}.",
				modulestack.BattleFireSubjectVerbOn(),
				target.ReportName,
				targetModule.ReportHitLocationLabel);
		}

		private string formatObserverMissLine(
			ModuleStack modulestack,
			ModuleStack target,
			string weaponName,
			bool compositeFire)
		{
			if (compositeFire)
			{
				return string.Format("{0} fires {1} on {2} and misses.",
					modulestack.ReportName,
					weaponName,
					target.ReportName);
			}
			return string.Format("{0} {1} and misses.",
				modulestack.BattleFireSubjectVerbOn(),
				target.ReportName);
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
			int chance = modulestack.Attack + modulestack.ModuleStacks.Attack();
			if (target != null && !string.IsNullOrEmpty(modulestack.ModuleType != null ? modulestack.ModuleType.WeaponGroup : null))
			{
				string resists = target.ModuleType != null ? target.ModuleType.Resists : string.Empty;
				chance = System.Convert.ToInt32(Math.Ceiling(chance * CombatMatchup.ChanceMultiplier(modulestack.ModuleType.WeaponGroup, resists)));
			}
			if (target != null && target.HasEvade)
			{
				chance = chance / 2;
			}
			if (target != null && target.IsImmobile
				&& (target.ModuleType == null || !target.ModuleType.LivingUnit))
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

        private Module resolveHitLocation(ModuleStack shooter, ModuleStack moduleStack, ETactic eTactic, bool targetEvade)
		{
			Faction targetOwner = moduleStack.Owner;
			int damageArea = this.getDamageArea(moduleStack, eTactic, targetEvade, targetOwner);
			if (damageArea < 1)
			{
				damageArea = 1;
			}
            int roll = this.getRoll(damageArea, string.Concat("Hit location 1 to ", damageArea.ToString()));
			List<string> candidates = new List<string>();
			this.collectHitLocationCandidates(moduleStack, eTactic, targetEvade, targetOwner, candidates);
			Module selected = this.getModule(moduleStack, roll, eTactic, targetEvade, targetOwner);
			this.logGmHitLocation(shooter, moduleStack, roll, damageArea, candidates, selected);
			return selected;
		}

		private void collectHitLocationCandidates(
			ModuleStack moduleStack,
			ETactic eTactic,
			bool targetEvade,
			Faction targetOwner,
			List<string> candidates)
		{
			if (moduleStack == null)
			{
				return;
			}
			if (targetOwner == null || moduleStack.Owner == targetOwner)
			{
				int weight = this.hitWeight(moduleStack, eTactic, targetEvade);
				if (weight > 0)
				{
					candidates.Add(string.Format("{0} weight={1}", moduleStack.ReportName, weight));
				}
			}
			foreach (ModuleStack nested in moduleStack.ModuleStacks.Values)
			{
				this.collectHitLocationCandidates(nested, eTactic, targetEvade, targetOwner, candidates);
			}
		}

		private void logGmBattleCreated()
		{
			TurnGmLog log = TurnGmLog.Current;
			if (log == null || !log.IsEnabled(ETurnGmLogChannel.Battles))
			{
				return;
			}
			if (this.attacker == null || this.battleDefender == null || this.attacker.Location == null)
			{
				return;
			}
			log.AppendSection(string.Format(
				"battle created week={0} location={1} initiator={2} primary_defender={3}",
				this.week,
				this.attacker.Location.Name,
				this.attacker.Name,
				this.battleDefender.Name));
			foreach (ModuleStack stack in stacksAtLocation(this.attacker.Location))
			{
				if (!stack.IsRootModuleStack)
				{
					continue;
				}
				if (!stack.HasIntactModules() && !stack.IsArmed)
				{
					continue;
				}
				string factionName = stack.Owner != null ? stack.Owner.Name : "-";
				string decision;
				if (this.attackers.Contains(stack.Name))
				{
					decision = "join attacker";
				}
				else if (this.defenders.Contains(stack.Name))
				{
					decision = "join defender";
				}
				else
				{
					decision = "skip: " + this.describeBattleSkipReason(stack);
				}
				log.Append(string.Format(
					"faction={0} unit={1} decision={2}",
					factionName,
					stack.ReportName,
					decision));
			}
			log.Append(string.Empty);
		}

		private string describeBattleSkipReason(ModuleStack stack)
		{
			if (!stack.HasIntactModules())
			{
				return "not intact";
			}
			if (this.attacker == null || this.battleDefender == null)
			{
				return "not eligible";
			}
			bool couldAttack = this.canJoinAsAttacker(stack)
				&& this.joinsAttack(stack.Owner, this.attacker.Owner, this.battleDefender.Owner);
			bool couldDefend = this.joinsDefense(stack.Owner, this.attacker.Owner, this.battleDefender.Owner);
			if (couldAttack || couldDefend)
			{
				return "not selected for side";
			}
			if (!stack.IsArmed && !this.joinsDefense(stack.Owner, this.attacker.Owner, this.battleDefender.Owner))
			{
				return "diplomacy or not armed";
			}
			return "diplomacy";
		}

		private void logGmHitLocation(
			ModuleStack shooter,
			ModuleStack target,
			int roll,
			int damageArea,
			List<string> candidates,
			Module selected)
		{
			TurnGmLog log = TurnGmLog.Current;
			if (log == null || !log.IsEnabled(ETurnGmLogChannel.Battles))
			{
				return;
			}
			string shooterFaction = shooter != null && shooter.Owner != null ? shooter.Owner.Name : "-";
			string selectedStack = selected != null && selected.Parent != null
				? selected.Parent.ReportName
				: "-";
			log.Append(string.Format(
				"hit week={0} round={1} shooter_faction={2} shooter={3} target={4} roll={5}/{6} selected_stack={7} selected_module={8} candidates=[{9}]",
				this.week,
				this.round,
				shooterFaction,
				shooter != null ? shooter.ReportName : "-",
				target != null ? target.ReportName : "-",
				roll,
				damageArea,
				selectedStack,
				selected != null ? selected.ReportID : "-",
				string.Join("; ", candidates.ToArray())));
		}
	}
}
