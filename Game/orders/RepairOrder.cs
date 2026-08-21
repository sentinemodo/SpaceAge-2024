using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public class RepairOrder : LongOrder
	{
		public const string EngineeringShopModuleTypeName = "engshp";
		public const int EngineeringShopRepairPoints = 20;
		public const int ManualRepairPoints = 10;
		public const int UnsuppliedRepairPoints = 1;

		public RepairOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.repair;
		}

		public ModuleStack Repairer
		{
			get { return (ModuleStack)this.Subject; }
		}

		public bool IsEngineeringShop
		{
			get
			{
				return this.Repairer.ModuleType != null
					&& this.Repairer.ModuleType.Name == EngineeringShopModuleTypeName;
			}
		}

		public int RepairPoints
		{
			get
			{
				if (this.IsEngineeringShop)
				{
					int copies = this.Repairer.QuantityActive;
					if (copies < 1)
					{
						copies = 1;
					}
					return EngineeringShopRepairPoints * copies;
				}
				return ManualRepairPoints;
			}
		}

		public int SparePartsRequired
		{
			get
			{
				if (this.IsEngineeringShop)
				{
					int copies = this.Repairer.QuantityActive;
					if (copies < 1)
					{
						copies = 1;
					}
					return copies;
				}
				return 1;
			}
		}

		public ModuleStack RepairScope
		{
			get
			{
				ModuleStack parent = this.Repairer.Parent as ModuleStack;
				if (parent != null)
				{
					return parent;
				}
				return this.Repairer;
			}
		}

		public override void Parse(string command)
		{
			if (!(this.Subject is ModuleStack))
			{
				throw new Exception("REPAIR can only be issued by a modulestack.");
			}
		}

		public override List<string> Report(Faction owner)
		{
			List<string> lines = new List<string>
			{
				string.Format("{0}{1}repair",
					this.Conditions,
					(this.Repeat > 1) ? string.Concat(this.Repeat.ToString(), " ") : ((this.Repeat < 0) ? "@" : string.Empty))
			};
			return lines;
		}

		public override void LoadXml(XmlElement elOrder)
		{
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			XmlElement elRepair = doc.CreateElement("repair");
			this.xmlElement.AppendChild(elRepair);
			return this.xmlElement;
		}

		public override void Execute(int week)
		{
			if (!this.CanOperate(week))
			{
				base.Execute(week);
				return;
			}

			int damage = this.totalDamage(this.RepairScope);
			if (damage < 1)
			{
				return;
			}

			int points = this.RepairPoints;
			int spareNeeded = this.SparePartsRequired;
			ItemStacks needed = this.spareParts(spareNeeded);
			IItemStacksHolder holder = this.findItemHolder(this.RepairScope, needed);
			if (holder != null)
			{
				holder.ItemStacks.Minus(needed);
				holder.EventReports.Add(
					week,
					string.Format("consumed {0} for repairs{1}.",
						needed.ReportList,
						(holder == this.Repairer) ? string.Empty : string.Concat(" for ", this.Repairer.ReportName)));
			}
			else
			{
				points = UnsuppliedRepairPoints;
			}

			if (this.durationLeft == Int32.MaxValue || this.durationLeft <= 0)
			{
				this.durationLeft = 1;
			}
			this.durationLeft--;
			int repaired = this.applyRepair(this.RepairScope, points);
			this.Repairer.EventReports.Add(week, string.Format("repaired {0} damage.", repaired));
			this.Executing = true;
			base.Execute(week);
		}

		private ItemStacks spareParts(int quantity)
		{
			ItemStacks stacks = new ItemStacks();
			stacks.Add(new ItemStack(ItemType.All["spare"], quantity));
			return stacks;
		}

		private int totalDamage(ModuleStack stack)
		{
			int damage = stack.Damage;
			foreach (ModuleStack nested in this.nestedSorted(stack))
			{
				damage += this.totalDamage(nested);
			}
			return damage;
		}

		private int applyRepair(ModuleStack stack, int points)
		{
			int remaining = points;
			foreach (Module module in stack.Modules)
			{
				if (remaining < 1)
				{
					break;
				}
				if (module.Damage < 1)
				{
					continue;
				}
				int repair = module.Damage;
				if (repair > remaining)
				{
					repair = remaining;
				}
				module.Damage -= repair;
				remaining -= repair;
			}
			foreach (ModuleStack nested in this.nestedSorted(stack))
			{
				if (remaining < 1)
				{
					break;
				}
				remaining -= this.applyRepair(nested, remaining);
			}
			return points - remaining;
		}

		private List<ModuleStack> nestedSorted(ModuleStack stack)
		{
			List<ModuleStack> nested = new List<ModuleStack>();
			foreach (ModuleStack child in stack.ModuleStacks.Values)
			{
				nested.Add(child);
			}
			nested.Sort(ModuleStacks.CompareByNames);
			return nested;
		}

		private IItemStacksHolder findItemHolder(IItemStacksHolder holder, ItemStacks needed)
		{
			if (holder.ItemStacks.Has(needed))
			{
				return holder;
			}

			ModuleStack stack = holder as ModuleStack;
			if (stack != null)
			{
				foreach (ModuleStack nested in this.nestedSorted(stack))
				{
					IItemStacksHolder found = this.findItemHolder(nested, needed);
					if (found != null)
					{
						return found;
					}
				}
				foreach (Person person in stack.People.Values)
				{
					IItemStacksHolder found = this.findItemHolder(person, needed);
					if (found != null)
					{
						return found;
					}
				}
			}
			return null;
		}
	}
}
