using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class ItemStacks : Dictionary<ItemType, ItemStack>, IReporting, IProducable
	{
		public double Mass()
		{
			double mass = 0;
			foreach (ItemStack itemStack in this.Values)
			{
				mass += itemStack.Mass;
			}
			return mass;
		}

		public double Size()
		{
			double size = 0;
			foreach (ItemStack itemStack in this.Values)
			{
				size += itemStack.Size;
			}
			return size;
		}

		public int CountCrew()
		{
			int count = 0;
			foreach (ItemStack itemStack in this.Values)
			{
				if (itemStack.ItemType.Group == EItemTypesGroup.crew)
				{
					count += itemStack.Quantity;
				}
			}
			return count;
		}


		public void Add(ItemStack additionalItemStack)
		{
			if (this.ContainsKey(additionalItemStack.ItemType)) 
			{
				this[additionalItemStack.ItemType].Quantity += additionalItemStack.Quantity;
			} else
			{				
				ItemType itemType = additionalItemStack.ItemType;
				base.Add(itemType, new ItemStack(itemType, additionalItemStack.Quantity));
			}
		}

		public void Sum(ItemStacks itemStacks)
		{
			foreach (ItemStack itemStack in itemStacks.Values)
			{
				this.Add(itemStack);
			}
		}

		public void Multiply(int multiplier)
		{
			foreach (ItemStack itemStack in this.Values)
			{
				itemStack.Quantity *= multiplier;
			}
		}

		public ItemStacks Upkeep
		{
			get
			{
				ItemStacks upkeepItemStacks = new ItemStacks();
				foreach (ItemStack itemStack in this.Values)
				{
					upkeepItemStacks.Sum(itemStack.Upkeep);
				}
				return upkeepItemStacks;
			}
		}

		public ItemStacks Consume
		{
			get
			{
				ItemStacks consumeItemStacks = new ItemStacks();
				foreach (ItemStack itemStack in this.Values)
				{
					consumeItemStacks.Sum(itemStack.Consume);
				}
				return consumeItemStacks;
			}
		}

		public int Quantity(ItemType itemType)
		{
			if (this.ContainsKey(itemType))
			{
				return this[itemType].Quantity;
			}
			return 0;
		}

        public int Quantity(string itemTypeName)
        {
            ItemType itemType = ItemType.All[itemTypeName];
            return this.Quantity(itemType);
        }

		public int CombatAttack(EModuleTypesGroup carrierGroup, int quantityActive)
		{
			int attack = 0;
			foreach (ItemStack itemStack in this.Values)
			{
				if (this.isCombatEligible(itemStack.ItemType, carrierGroup))
				{
					attack += this.combatQuantity(itemStack, quantityActive) * itemStack.ItemType.Attack;
				}
			}
			return attack;
		}

		public int CombatDefense(EModuleTypesGroup carrierGroup, int quantityActive)
		{
			int defense = 0;
			foreach (ItemStack itemStack in this.Values)
			{
				if (this.isCombatEligible(itemStack.ItemType, carrierGroup))
				{
					defense += this.combatQuantity(itemStack, quantityActive) * itemStack.ItemType.Defense;
				}
			}
			return defense;
		}

		public int CombatDamageShotBudget(EModuleTypesGroup carrierGroup, int quantityActive)
		{
			int budget = 0;
			foreach (ItemStack itemStack in this.Values)
			{
				if (this.isCombatEligible(itemStack.ItemType, carrierGroup) && itemStack.ItemType.Damage > 0)
				{
					budget += this.combatQuantity(itemStack, quantityActive);
				}
			}
			return budget;
		}

		public int CombatDamageBonusForShot(EModuleTypesGroup carrierGroup, ref int remainingShots)
		{
			if (remainingShots <= 0)
			{
				return 0;
			}
			remainingShots--;
			int bonus = 0;
			foreach (ItemStack itemStack in this.Values)
			{
				if (this.isCombatEligible(itemStack.ItemType, carrierGroup) && itemStack.ItemType.Damage > 0)
				{
					bonus += itemStack.ItemType.Damage;
				}
			}
			return bonus;
		}

		public int CombatInitiative(EModuleTypesGroup carrierGroup, int quantityActive)
		{
			int initiative = 0;
			foreach (ItemStack itemStack in this.Values)
			{
				if (this.isCombatEligible(itemStack.ItemType, carrierGroup))
				{
					initiative += this.combatQuantity(itemStack, quantityActive) * itemStack.ItemType.Initiative;
				}
			}
			return initiative;
		}

		private bool isCombatEligible(ItemType itemType, EModuleTypesGroup carrierGroup)
		{
			return itemType.UseAllowedModuleTypesGroup != null
				&& itemType.UseAllowedModuleTypesGroup == carrierGroup;
		}

		private int combatQuantity(ItemStack itemStack, int quantityActive)
		{
			if (itemStack.Quantity <= 0 || quantityActive <= 0)
			{
				return 0;
			}
			return Math.Min(itemStack.Quantity, quantityActive);
		}

		#region IReporting Members

		public List<string> Report(Faction faction)
		{
			return this.Report(faction, 0);
		}

		public List<string> Report(Faction faction, int level)
		{			
			string line = "";
			line = string.Concat(line, "items: ");
			bool firstAdded = false;
			foreach (ItemStack itemstack in this.Values)
			{
				line = string.Format("{0}{1}", line, (firstAdded) ? ", " : "");
				firstAdded = true;

				if (itemstack.Quantity > 1)
				{
					line = string.Format("{0}{1} {2}", line, itemstack.Quantity, itemstack.ItemType.ReportNameMultiple);
				}
				else
				{
					line = string.Concat(line, itemstack.ItemType.ReportName);
				}
				if (itemstack.Size > 0 || itemstack.Mass > 0)
				{
					line = string.Concat(line, " (");

					bool firstDetailAdded = false;
					if (itemstack.Size > 0)
					{
						line = string.Format("{0}{1}{2}", line, (firstDetailAdded) ? ", size: " : "size: ", itemstack.Size);
						firstDetailAdded = true;
					}

					if (itemstack.Mass > 0)
					{
						line = string.Format("{0}{1}{2}", line, (firstDetailAdded) ? ", mass: " : "mass: ", itemstack.Mass);
						firstDetailAdded = true;
					}

					#region upkeep
					if (itemstack.Upkeep.Count > 0)
					{
						line = string.Format("{0}{1}", line, (firstDetailAdded) ? ", upkeep: " : "upkeep: ");
						bool firstUpkeepAdded = false;
						foreach (ItemStack upkeepitemstack in itemstack.Upkeep.Values)
						{
							line = string.Format("{0}{1}", line, (firstUpkeepAdded) ? ", " : "");
							firstUpkeepAdded = true;

							if (upkeepitemstack.Quantity > 1)
							{
								line = string.Format("{0}{1} {2}", line, upkeepitemstack.Quantity, upkeepitemstack.ItemType.ReportNameMultiple);
							}
							else
							{
								line = string.Concat(line, upkeepitemstack.ItemType.ReportName);
							}
						}
					}
					#endregion

					#region consume
					if (itemstack.Consume.Count > 0)
					{
						line = string.Format("{0}{1}", line, (firstDetailAdded) ? ", consume: " : "consume: ");
						bool firstConsumeAdded = false;
						foreach (ItemStack consumeItemStack in itemstack.Consume.Values)
						{
							line = string.Format("{0}{1}", line, (firstConsumeAdded) ? ", " : "");
							firstConsumeAdded = true;

							if (consumeItemStack.Quantity > 1)
							{
								line = string.Format("{0}{1} {2}",
									line, 
									consumeItemStack.Quantity,
									consumeItemStack.ItemType.ReportNameMultiple);
							}
							else
							{
								line = string.Concat(line, consumeItemStack.ItemType.ReportName);
							}
						}
					}
					#endregion
					line = string.Concat(line, ")");
				}
			}
			line = string.Concat(line, ".");
			ReportLine reportLine = new ReportLine(line, level);
			return reportLine.IndentedLines;
		}

		public string ReportList
		{
			get
			{
				string line = "";
				bool firstAdded = false;
				foreach (ItemStack itemstack in this.Values)
				{
					line = string.Format("{0}{1}{2}", 
						line, 
						(firstAdded) ? ", " : "",
						itemstack.ReportName);
					firstAdded = true;				
				}
				return line;
			}
		}

		#endregion

		public bool Has(ItemStacks itemStacks)
		{
			foreach (ItemStack itemStack in itemStacks.Values)
			{
				if (!this.Has(itemStack))
					return false;
			}
			return true;
		}

		public bool Has(ItemStack itemStack)
		{
			if (!this.ContainsKey(itemStack.ItemType) || this[itemStack.ItemType].Quantity < itemStack.Quantity)
			{
				return false;
			}
			return true;
		}

		public bool Has(ItemType itemType)
		{
			if (!this.ContainsKey(itemType) || this[itemType].Quantity == 0)
			{
				return false;
			}
			return true;
		}

		public void Minus(ItemStacks itemStacks)
		{
            if (itemStacks != null)
            {
                foreach (ItemStack itemStack in itemStacks.Values)
                {
                    this.Remove(itemStack);
                }
            }
		}

		public void Minus(ItemStack itemStack)
		{
			this.Remove(itemStack);
		}

		public void Minus(ItemType itemType, int quantity = 1)
		{
			this.Remove(new ItemStack(itemType, quantity));
		}

		public void Remove(ItemStack usedItemStack)
		{
			if (!this.ContainsKey(usedItemStack.ItemType) || this[usedItemStack.ItemType].Quantity < usedItemStack.Quantity)
			{
				throw new Exception("Not enough of " + usedItemStack.ItemType.Name);
			}
			else
			{
				this[usedItemStack.ItemType].Quantity -= usedItemStack.Quantity;
				if (this[usedItemStack.ItemType].Quantity == 0)
				{
					base.Remove(usedItemStack.ItemType);
				}				
			}
		}

        public void LoadXml(XmlElement elHolder, IItemStacksHolder holder, string itemStackGroup = "itemstack")
        {
            ItemStack itemStack;
            ItemType itemType;

            foreach (XmlElement elItemStack in elHolder.SelectNodes(itemStackGroup))
            {
                itemType = ItemType.All[elItemStack.GetAttribute("type")];
                itemStack = new ItemStack(itemType);
                itemStack.LoadXml(elItemStack);
                this.Add(itemStack);
            }
        }

        public XmlElement SaveXml(XmlDocument doc, XmlElement elHolder, Faction faction = null, string itemStackGroup = "itemstack")
        {
            XmlElement elItemStack;

            foreach (ItemStack itemStack in this.Values)
            {
                if (!itemStack.Visible(faction))
                    continue;

                elItemStack = itemStack.SaveXml(doc, itemStackGroup);
                elHolder.AppendChild(elItemStack);
            }

            return elHolder;
        }
	}
}
