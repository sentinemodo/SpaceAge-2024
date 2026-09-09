using System;
using System.Collections.Generic;

namespace SpaceAge
{
	public partial class ModuleStack
	{
		#region combat
		public int HitPoints
		{
			get
			{
                if (this.IsFormed)
                {
                    return this.Quantity * this.moduleType.DamageCapacity;
                }
                return 0;
			}
		}

		private Modules modules = new Modules();
		public Modules Modules
		{
			get { return this.modules; }
		}

		private bool isAvoiding = false;
		public bool IsAvoiding
		{
			get { return this.isAvoiding; }
			set { this.isAvoiding = value; }
		}

		public int Damage
		{
			get
			{
				int damage = 0;
				foreach (Module module in this.modules)
				{
					damage += module.Damage;
				}
				return damage;
			}
		}

		public int CaptureDamage
		{
			get
			{
				int captureDamage = 0;
				foreach (Module module in this.modules)
				{
					captureDamage += module.CaptureDamage;
				}
				return captureDamage;
			}
		}

		public int Attack
		{
			get
			{
				int attack = 0;
                if (this.IsFormed)
                {
                    attack += this.QuantityActive * this.moduleType.Attack;
                    attack += this.technologies.Attack;
                    attack += this.People.CombatAttack(this.RootModuleStack);
					attack += this.ItemStacks.CombatAttack(this.moduleType.Group, this.QuantityActive);
                }
				return attack;
			}
		}

		public int Defense
		{
			get
			{
				int defense = 0;
                if (this.IsFormed)
                {
                    defense += this.QuantityActive * this.moduleType.Defense;
                    defense += this.technologies.Defense;
                    defense += this.People.CombatDefense(this.RootModuleStack);
					defense += this.ItemStacks.CombatDefense(this.moduleType.Group, this.QuantityActive);
                }
				return defense;
			}
		}

		public int ModuleShotDamage()
		{
			if (!this.IsFormed || this.moduleType == null)
			{
				return 0;
			}
			return this.moduleType.Damage;
		}

		public int Initiative
		{
			get
			{
				return this.InitiativeBonus + this.ModuleStacks.InitiativeBonus();
			}
		}

		public int InitiativeManeuverabilityBonus
		{
			get
			{
				double energyReserves = 0;
				double massCapacityReserves = 0;

                if (this.EnergyRequired + this.ModuleStacks.EnergyRequired() > 0)
				{
					energyReserves = (this.EnergyProduction + this.ModuleStacks.EnergyProduction())
						/ System.Convert.ToDouble(this.EnergyRequired + this.ModuleStacks.EnergyRequired());
				}
				if (this.Mass > 0)
				{
					massCapacityReserves = (this.MassCapacity + this.ModuleStacks.MassCapacity) / this.Mass;
				}
				return System.Convert.ToInt32(energyReserves + massCapacityReserves) * 10;
			}
		}

		public int InitiativeBonus
		{
			get 
			{
				int initiativeBonus = 0;
				if (this.IsRootModuleStack)
				{
					initiativeBonus += this.InitiativeManeuverabilityBonus;
				}
				if (this.moduleType != null)
				{
					initiativeBonus += this.moduleType.Initiative;
				}
				initiativeBonus += this.technologies.Initiative;
				initiativeBonus += this.People.CombatInitiative(this.RootModuleStack);
				if (this.IsFormed && this.moduleType != null)
				{
					initiativeBonus += this.ItemStacks.CombatInitiative(this.moduleType.Group, this.QuantityActive);
				}
				return initiativeBonus;
			}
		}

		public int MedicalCureChance
		{
			get
			{
				int max = this.People.CureChance(this.RootModuleStack);
				foreach (ModuleStack nested in this.ModuleStacks.Values)
				{
					int nestedMax = nested.MedicalCureChance;
					if (nestedMax > max)
					{
						max = nestedMax;
					}
				}
				return max;
			}
		}

		public int ResearchSkillOutputBonus
		{
			get
			{
				int bonus = this.People.ResearchOutputBonus(this);
				foreach (ModuleStack nested in this.ModuleStacks.Values)
				{
					bonus += nested.ResearchSkillOutputBonus;
				}
				return bonus;
			}
		}

		private Tactics tactics = new Tactics();
		public Tactics Tactics
		{
			get { return this.tactics; }
		}

		public string PreferredTargetName { get; set; }

		public bool HasEvade
		{
			get { return this.Tactics.ContainsName("evade"); }
		}

		public bool HasCapture
		{
			get { return this.Tactics.ContainsName("capture"); }
		}

		public bool HasPrioritizeArmed
		{
			get { return this.Tactics.ContainsName("prioritize armed"); }
		}

		public bool HasPrioritizeCommand
		{
			get { return this.Tactics.ContainsName("prioritize command"); }
		}

		public bool HasPrioritizeCargo
		{
			get
			{
				return this.Tactics.ContainsName("prioritize storage")
					|| this.Tactics.ContainsName("prioritize cargo");
			}
		}

		public ETactic FiringTactic
		{
			get { return this.HasCapture ? ETactic.capture : ETactic.destroy; }
		}

		public void ApplyTactic(string tacticName)
		{
			if (tacticName == "evade")
			{
				if (!this.Tactics.ContainsName("evade"))
				{
					this.Tactics.Add(new EvadeTactic(this));
				}
				return;
			}

			this.Tactics.RemoveByName("destroy");
			this.Tactics.RemoveByName("capture");
			this.Tactics.RemoveByName("disable");
			if (tacticName == "capture")
			{
				this.Tactics.Add(new CaptureTactic(this));
			}
			else
			{
				this.Tactics.Add(new DestroyTactic(this));
			}
		}

		public void ApplyPrioritizeTactic(string tacticName)
		{
			this.Tactics.RemoveByName("prioritize armed");
			this.Tactics.RemoveByName("prioritize command");
			this.Tactics.RemoveByName("prioritize cargo");
			this.Tactics.RemoveByName("prioritize storage");
			if (tacticName == "prioritize armed")
			{
				this.Tactics.Add(new PrioritizeArmedTactic(this));
			}
			else if (tacticName == "prioritize command")
			{
				this.Tactics.Add(new PrioritizeCommandTactic(this));
			}
			else if (tacticName == "prioritize cargo" || tacticName == "prioritize storage")
			{
				this.Tactics.Add(new PrioritizeCargoTactic(this));
			}
		}

		public bool HasIntactModules()
		{
			foreach (Module module in this.Modules)
			{
				if (!module.IsWrecked)
				{
					return true;
				}
			}
			foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
			{
				if (moduleStack.HasIntactModules())
				{
					return true;
				}
			}
			return false;
		}

		public bool IsCommandStack()
		{
			if (this.moduleType != null && this.moduleType.Group == EModuleTypesGroup.command)
			{
				return true;
			}
			foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
			{
				if (moduleStack.IsCommandStack())
				{
					return true;
				}
			}
			return false;
		}

		public bool IsCargoStack()
		{
			if (this.moduleType != null && this.moduleType.Group == EModuleTypesGroup.storage)
			{
				return true;
			}
			foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
			{
				if (moduleStack.IsCargoStack())
				{
					return true;
				}
			}
			return false;
		}

		public bool IsHangarCraft
		{
			get { return this.moduleType != null && this.moduleType.IsHangarCraft; }
		}

		public static bool CanNestHangarCraft(IHolder parent, ModuleType craftType)
		{
			if (parent == null)
			{
				return false;
			}
			if (parent.IsLocation)
			{
				return true;
			}
			ModuleStack stack = parent as ModuleStack;
			if (stack == null || stack.ModuleType == null)
			{
				return false;
			}
			if (stack.ModuleType.IsDroneBay)
			{
				return true;
			}
			return craftType != null
				&& craftType.Name == "shuttl"
				&& ModuleType.IsShipHull(stack.ModuleType.Group);
		}

		public void SetOnline(bool online)
		{
			this.online = online;
			foreach (Module module in this.modules)
			{
				module.Online = online;
			}
		}

		#endregion
		public Modules GetFiringModules()
		{
			Modules firingModules = new Modules();
			if (this.IsFormed && this.moduleTypeIsCombatArmed(this.moduleType))
			{
				foreach (Module module in this.modules)
				{
					if (this.IsModuleOperational(module))
					{
						firingModules.Add(module);
					}
				}
			}
			foreach (ModuleStack modulestack in this.ModuleStacks.Values)
			{
				if (modulestack.IsHangarCraft)
				{
					continue;
				}
				if (modulestack.ModuleType != null && modulestack.ModuleType.IsDroneBay)
				{
					continue;
				}
				firingModules.AddRange(modulestack.GetFiringModules());
			}
			return firingModules;
		}

		public Modules GetModules(EModuleTypesGroup group)
		{			
			Modules groupModules = new Modules();
			if (this.moduleType.Group == group)
			{
				groupModules.AddRange(this.modules);
			}
			foreach (ModuleStack modulestack in this.ModuleStacks.Values)
			{
				groupModules.AddRange(modulestack.GetModules(group));
			}
			return groupModules;
		}
	}
}
