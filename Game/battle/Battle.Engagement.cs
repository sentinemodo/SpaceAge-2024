using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public partial class Battle
	{
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
			this.logGmBattleCreated();
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
					if (!ShouldCommenceBattle(stack, other))
					{
						continue;
					}
					started.Add(new Battle(stack, other));
				}
			}
			return started;
		}

		public static bool ShouldCommenceBattle(ModuleStack initiator, ModuleStack target)
		{
			Battle battle = new Battle(initiator, target);
			bool commence = battle.HasActionableEngagement();
			Battle.All.Remove(battle);
			return commence;
		}

		public bool HasActionableEngagement()
		{
			return this.sideCanEngage(this.attackers, this.defenders)
				|| this.sideCanEngage(this.defenders, this.attackers);
		}

		private bool sideCanEngage(ModuleStacks side, ModuleStacks opponents)
		{
			if (side == null || side.Count == 0 || opponents == null || opponents.Count == 0)
			{
				return false;
			}

			foreach (ModuleStack stack in side.Values)
			{
				if (!stack.IsArmed || !stack.HasOperationalModules)
				{
					continue;
				}
				if (this.availableTargets(opponents, stack).Count > 0)
				{
					return true;
				}
				if (this.hasVictoryCleanupTactic(stack) && this.hasDisabledIntactOpponent(opponents))
				{
					return true;
				}
			}
			return false;
		}

		private bool hasVictoryCleanupTactic(ModuleStack stack)
		{
			return stack.HasDestroy || stack.HasCapture || stack.HasScavenge;
		}

		private bool hasDisabledIntactOpponent(ModuleStacks opponents)
		{
			foreach (ModuleStack target in opponents.Values)
			{
				if (target.HasIntactModules() && !target.HasOperationalModules)
				{
					return true;
				}
			}
			return false;
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
	}
}
