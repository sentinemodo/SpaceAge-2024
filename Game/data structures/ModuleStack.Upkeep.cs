using System;
using System.Collections.Generic;

namespace SpaceAge
{
	public partial class ModuleStack
	{
		// Deduct medici for wounded/mad crew on this stack only. Food and
		// terair are not deducted. Unsupplied crew no longer die weekly;
		// wounded terrans roll die/stay/recover at quarter end.
		public void ExecuteMedicalConsume(int week)
		{
			if (!ItemType.All.ContainsKey("medici"))
			{
				return;
			}

			ItemType medici = ItemType.All["medici"];
			this.consumeMedicalRace("wndtrn", medici);
			this.consumeMedicalRace("madtrn", medici);
		}

		public void ExecuteSickBayHeal(int week)
		{
			if (!this.IsFormed || this.moduleType == null)
			{
				return;
			}
			if (this.moduleType.HealTarget != "wndtrn")
			{
				return;
			}
			if (!ItemType.All.ContainsKey("wndtrn") || !ItemType.All.ContainsKey("terran"))
			{
				return;
			}

			int wounded = this.ItemStacks.Quantity(ItemType.All["wndtrn"]);
			if (wounded < 1)
			{
				return;
			}

			int mediciAvailable = 0;
			ItemType medici = null;
			if (!string.IsNullOrEmpty(this.moduleType.HealConsumeItem)
				&& ItemType.All.ContainsKey(this.moduleType.HealConsumeItem))
			{
				medici = ItemType.All[this.moduleType.HealConsumeItem];
				mediciAvailable = this.ItemStacks.Quantity(medici);
			}

			int converted = 0;
			if (medici != null && mediciAvailable > 0 && this.moduleType.HealQuantityWithItem > 0)
			{
				int perBay = this.moduleType.HealQuantityWithItem;
				int consumeEach = Math.Max(this.moduleType.HealConsumeQuantity, 1);
				int slots = perBay * this.Quantity;
				int byMedici = mediciAvailable / consumeEach;
				converted = this.convertWoundedInSickBay(week, Math.Min(slots, byMedici));
				if (converted > 0)
				{
					this.ItemStacks.Minus(medici, converted * consumeEach);
				}
				this.SickBayUnmedicatedWeeks = 0;
			}
			else if (this.moduleType.HealQuantity > 0 && this.moduleType.HealWeeks > 0)
			{
				this.SickBayUnmedicatedWeeks++;
				if (this.SickBayUnmedicatedWeeks >= this.moduleType.HealWeeks)
				{
					converted = this.convertWoundedInSickBay(week, this.moduleType.HealQuantity * this.Quantity);
					this.SickBayUnmedicatedWeeks = 0;
				}
			}
		}

		public int SickBayUnmedicatedWeeks { get; set; }

		private int convertWoundedInSickBay(int week, int count)
		{
			ItemType woundedType = ItemType.All["wndtrn"];
			int available = this.ItemStacks.Quantity(woundedType);
			int converted = Math.Min(count, available);
			if (converted < 1)
			{
				return 0;
			}

			this.ItemStacks.Minus(woundedType, converted);
			this.ItemStacks.Add(new ItemStack(ItemType.All["terran"], converted));
			string name = (converted == 1) ? woundedType.ReportName : woundedType.ReportNameMultiple;
			this.EventReports.Add(
				week,
				string.Format("{0} {1} recovered in sick bay.", converted, name));
			return converted;
		}

		// Cash, food, and terair are paid once per turn at quarter end.
		public void ExecuteMaintenance(int week)
		{
			if (!this.IsFormed)
			{
				return;
			}

			this.payLocalUpkeep(week);
			this.consumeLocalSupplies(week);
		}

		public bool NeedsCannedAir
		{
			get
			{
				Location location = this.Location;
				if (location == null)
				{
					return true;
				}
				if (location.LocationType == ELocationType.orbit || location.LocationType == ELocationType.space)
				{
					return true;
				}
				Region region = location as Region;
				if (region != null && region.RegionHolder is Moon)
				{
					return true;
				}
				return false;
			}
		}

		public void ExecuteQuarterlyWoundedOutcome(int week)
		{
			if (!ItemType.All.ContainsKey("wndtrn") || !ItemType.All.ContainsKey("terran"))
			{
				return;
			}

			ItemType woundedType = ItemType.All["wndtrn"];
			if (!this.itemStacks.ContainsKey(woundedType) || this.itemStacks[woundedType].Quantity < 1)
			{
				return;
			}

			int wounded = this.itemStacks[woundedType].Quantity;
			int died = 0;
			int recovered = 0;
			for (int i = 0; i < wounded; i++)
			{
				int roll = Sequence.GenerateRandomInt(0, 100, "quarterly wounded outcome");
				if (roll < 25)
				{
					died++;
				}
				else if (roll >= 75)
				{
					recovered++;
				}
			}

			if (died > 0)
			{
				this.itemStacks.Minus(woundedType, died);
				string name = (died == 1) ? woundedType.ReportName : woundedType.ReportNameMultiple;
				this.EventReports.Add(
					week,
					string.Format("{0} {1} died of their wounds.", died, name));
			}

			if (recovered > 0)
			{
				this.itemStacks.Minus(woundedType, recovered);
				this.itemStacks.Add(new ItemStack(ItemType.All["terran"], recovered));
				string name = (recovered == 1) ? woundedType.ReportName : woundedType.ReportNameMultiple;
				this.EventReports.Add(
					week,
					string.Format("{0} {1} recovered from their wounds.", recovered, name));
			}
		}

		private void consumeMedicalRace(string raceName, ItemType medici)
		{
			if (!ItemType.All.ContainsKey(raceName) || !Race.All.ContainsKey(raceName))
			{
				return;
			}

			ItemType raceItem = ItemType.All[raceName];
			if (!this.itemStacks.ContainsKey(raceItem) || this.itemStacks[raceItem].Quantity < 1)
			{
				return;
			}

			Race race = Race.All[raceName];
			int mediciPerPerson = 0;
			if (race.Consume != null && race.Consume.ContainsKey(medici))
			{
				mediciPerPerson = race.Consume[medici].Quantity;
			}
			if (mediciPerPerson < 1)
			{
				return;
			}

			int wounded = this.itemStacks[raceItem].Quantity;
			int mediciNeeded = wounded * mediciPerPerson;
			int mediciAvailable = this.itemStacks.ContainsKey(medici)
				? this.itemStacks[medici].Quantity
				: 0;
			int mediciConsumed = Math.Min(mediciNeeded, mediciAvailable);
			if (mediciConsumed > 0)
			{
				this.itemStacks.Minus(medici, mediciConsumed);
			}
		}

		private ItemStacks localUpkeep()
		{
			ItemStacks local = new ItemStacks();
			local.Sum(this.UpkeepNetto);
			local.Sum(this.ItemStacks.Upkeep);
			local.Sum(this.People.Upkeep);
			if (BodyEnvironment.GravityAt(this.Location) == EGravityBand.high)
			{
				foreach (ItemStack bill in local.Values)
				{
					if (this.isCash(bill.ItemType))
					{
						bill.Quantity = (int)Math.Ceiling(bill.Quantity * 1.5);
					}
				}
				if (this.HasPeople || (this.moduleType != null && this.moduleType.PopulationMaximum > 0))
				{
					if (ItemType.All.ContainsKey("food"))
					{
						local.Add(new ItemStack(ItemType.All["food"], 2));
					}
				}
			}
			return local;
		}

		private ItemStacks localConsume()
		{
			ItemStacks local = new ItemStacks();
			local.Sum(this.ConsumeNetto);
			local.Sum(this.ItemStacks.Consume);
			local.Sum(this.People.Consume);
			return local;
		}

		private void payLocalUpkeep(int week)
		{
			bool unpaid = false;
			foreach (ItemStack bill in this.copyStacks(this.localUpkeep()))
			{
				int taken = this.takeFromNest(bill.ItemType, bill.Quantity);
				if (this.isCash(bill.ItemType))
				{
					taken += this.takeFromBank(bill.Quantity - taken);
				}
				int shortfall = bill.Quantity - taken;
				if (taken > 0)
				{
					this.EventReports.Add(
						week,
						string.Format("paid {0} {1} upkeep.", taken, bill.ItemType.ReportName));
				}
				if (shortfall > 0)
				{
					unpaid = true;
					this.EventReports.Add(
						week,
						string.Format("Lack of upkeep: {0} {1}.", shortfall, bill.ItemType.ReportName));
				}
			}

			if (!unpaid)
			{
				return;
			}

			if (this.moduleType != null)
			{
				this.applyShortageEffect(
					week,
					this.moduleType.NoUpkeepEffect,
					this.moduleType.NoUpkeepChance,
					"upkeep");
			}
			this.applyRaceNoUpkeep(week);
		}

		private void consumeLocalSupplies(int week)
		{
			bool cannedAir = this.NeedsCannedAir;
			int foodShortfall = 0;
			int terairShortfall = 0;
			bool moduleConsumeUnpaid = false;

			foreach (ItemStack bill in this.copyStacks(this.localConsume()))
			{
				if (bill.ItemType.Name == "medici")
				{
					continue;
				}
				if (bill.ItemType.Name == "terair" && !cannedAir)
				{
					continue;
				}

				int taken = this.takeFromNest(bill.ItemType, bill.Quantity);
				int shortfall = bill.Quantity - taken;
				if (shortfall < 1)
				{
					continue;
				}

				this.EventReports.Add(
					week,
					string.Format("Lack of supplies: {0} {1}.", shortfall, bill.ItemType.ReportName));
				if (bill.ItemType.Name == "food")
				{
					foodShortfall += shortfall;
				}
				if (bill.ItemType.Name == "terair")
				{
					terairShortfall += shortfall;
				}
				if (this.ConsumeNetto.Quantity(bill.ItemType) > 0)
				{
					moduleConsumeUnpaid = true;
				}
			}

			this.woundUnsuppliedTerrans(week, Math.Max(foodShortfall, terairShortfall));
			if (moduleConsumeUnpaid && this.moduleType != null)
			{
				this.applyShortageEffect(
					week,
					this.moduleType.NoConsumeEffect,
					this.moduleType.NoConsumeChance,
					"supplies");
			}
		}

		private List<ItemStack> copyStacks(ItemStacks stacks)
		{
			List<ItemStack> copy = new List<ItemStack>();
			foreach (ItemStack itemStack in stacks.Values)
			{
				copy.Add(new ItemStack(itemStack.ItemType, itemStack.Quantity));
			}
			return copy;
		}

		private int takeFromNest(ItemType itemType, int needed)
		{
			int taken = 0;
			List<ModuleStack> visited = new List<ModuleStack>();

			taken += this.takeFromStackTree(this, itemType, needed - taken, visited);

			ModuleStack ancestor = this.Parent as ModuleStack;
			while (ancestor != null && taken < needed)
			{
				taken += this.takeFromStackOnly(ancestor, itemType, needed - taken, visited);
				ancestor = ancestor.Parent as ModuleStack;
			}

			ancestor = this.Parent as ModuleStack;
			if (ancestor != null && taken < needed)
			{
				taken += this.takeFromStackTree(ancestor, itemType, needed - taken, visited);
			}

			if (taken < needed && this.Location != null && this.Owner != null)
			{
				List<ModuleStack> snapshot = new List<ModuleStack>(ModuleStack.All.Values);
				foreach (ModuleStack other in snapshot)
				{
					if (taken >= needed)
					{
						break;
					}
					if (other == null || !other.IsFormed || other.Owner != this.Owner)
					{
						continue;
					}
					if (other.Location != this.Location || !other.IsRootModuleStack)
					{
						continue;
					}
					taken += this.takeFromStackTree(other, itemType, needed - taken, visited);
				}
			}

			return taken;
		}

		private int takeFromStackTree(ModuleStack stack, ItemType itemType, int needed, List<ModuleStack> visited)
		{
			int taken = this.takeFromStackOnly(stack, itemType, needed, visited);
			if (stack == null || stack.ModuleStacks.Count < 1)
			{
				return taken;
			}
			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				if (taken >= needed)
				{
					break;
				}
				taken += this.takeFromStackTree(nested, itemType, needed - taken, visited);
			}
			return taken;
		}

		private int takeFromStackOnly(ModuleStack stack, ItemType itemType, int needed, List<ModuleStack> visited)
		{
			if (stack == null || needed < 1 || visited.Contains(stack))
			{
				return 0;
			}
			visited.Add(stack);
			int available = stack.ItemStacks.Quantity(itemType);
			int take = Math.Min(available, needed);
			if (take > 0)
			{
				stack.ItemStacks.Minus(itemType, take);
			}
			return take;
		}

		private int takeFromBank(int needed)
		{
			if (needed < 1 || this.Owner == null || this.Owner.Bank == null)
			{
				return 0;
			}

			int available = (int)Math.Floor(this.Owner.Bank.Balance);
			if (available < 1)
			{
				return 0;
			}
			int take = Math.Min(available, needed);
			if (take > 0)
			{
				this.Owner.Bank.Balance -= take;
			}
			return take;
		}

		private void woundUnsuppliedTerrans(int week, int atRisk)
		{
			if (atRisk < 1 || !ItemType.All.ContainsKey("terran") || !Race.All.ContainsKey("terran"))
			{
				return;
			}

			ItemType terran = ItemType.All["terran"];
			int crew = this.ItemStacks.Quantity(terran);
			int rolls = Math.Min(atRisk, crew);
			if (rolls < 1)
			{
				return;
			}

			Race race = Race.All["terran"];
			if (race.NoConsumeEffect != "wound" || race.NoConsumeChance < 1)
			{
				return;
			}

			int wounded = 0;
			for (int i = 0; i < rolls; i++)
			{
				int roll = Sequence.GenerateRandomInt(0, 100, "no-consume wound");
				if (roll < race.NoConsumeChance)
				{
					wounded++;
				}
			}

			if (wounded < 1)
			{
				return;
			}

			this.ItemStacks.Minus(terran, wounded);
			this.ItemStacks.Add(new ItemStack(ItemType.All["wndtrn"], wounded));
			string name = (wounded == 1) ? terran.ReportName : terran.ReportNameMultiple;
			this.EventReports.Add(
				week,
				string.Format("{0} {1} wounded from lack of supplies.", wounded, name));
		}

		private void applyRaceNoUpkeep(int week)
		{
			if (!Race.All.ContainsKey("terran"))
			{
				return;
			}

			Race race = Race.All["terran"];
			if (string.IsNullOrEmpty(race.NoUpkeepEffect) || race.NoUpkeepChance < 1)
			{
				return;
			}
			if (this.ItemStacks.Quantity(ItemType.All["terran"]) < 1 && this.People.Count < 1)
			{
				return;
			}

			int roll = Sequence.GenerateRandomInt(0, 100, "no-upkeep race");
			if (roll < race.NoUpkeepChance)
			{
				this.EventReports.Add(
					week,
					string.Format("{0} from lack of upkeep.", this.shortageEffectLabel(race.NoUpkeepEffect)));
			}
		}

		private void applyShortageEffect(int week, string effect, int chance, string reason)
		{
			if (string.IsNullOrEmpty(effect) || chance < 1)
			{
				return;
			}

			int roll = Sequence.GenerateRandomInt(0, 100, "shortage " + reason);
			if (roll >= chance)
			{
				return;
			}

			if (effect == "damage")
			{
				this.applyShortageDamage();
				this.EventReports.Add(week, string.Format("Damaged from lack of {0}.", reason));
				return;
			}

			this.EventReports.Add(
				week,
				string.Format("{0} from lack of {1}.", this.shortageEffectLabel(effect), reason));
		}

		private void applyShortageDamage()
		{
			if (this.modules.Count < 1)
			{
				return;
			}

			Module target = this.modules[0];
			foreach (Module module in this.modules)
			{
				if (module.Damage < target.Damage)
				{
					target = module;
				}
			}
			target.Damage += 1;
		}

		private string shortageEffectLabel(string effect)
		{
			if (string.IsNullOrEmpty(effect) || effect.Length == 1)
			{
				return effect;
			}
			return char.ToUpper(effect[0]) + effect.Substring(1);
		}

		private bool isCash(ItemType itemType)
		{
			return itemType != null && itemType.Name == "cash";
		}
	}
}
