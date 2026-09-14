using System;
using System.Collections.Generic;

namespace SpaceAge
{
	public partial class ModuleStack
	{
		private bool sharing = true;

		public bool Sharing
		{
			get { return this.sharing; }
			set { this.sharing = value; }
		}

		public bool CanSupplyTo(ModuleStack consumer)
		{
			return consumer != null && (this == consumer || this.Sharing);
		}

		public bool HasItemsAvailableTo(ModuleStack consumer, ItemStacks needed)
		{
			if (needed == null)
			{
				return true;
			}

			foreach (ItemStack itemStack in needed.Values)
			{
				if (this.QuantityAvailableTo(consumer, itemStack.ItemType) < itemStack.Quantity)
				{
					return false;
				}
			}
			return true;
		}

		public int QuantityAvailableTo(ModuleStack consumer, ItemType itemType)
		{
			int quantity = 0;
			List<ModuleStack> visited = new List<ModuleStack>();
			ModuleStack unitRoot = consumer.RootModuleStack;
			if (unitRoot != null)
			{
				quantity += this.quantityInTreeForConsumer(unitRoot, consumer, itemType, visited);
			}
			visited = new List<ModuleStack>();
			foreach (ModuleStack source in consumer.orderedSharingSources())
			{
				quantity += this.quantityInTreeForConsumer(source, consumer, itemType, visited);
			}
			return quantity;
		}

		public int TakeItemsAvailableTo(ModuleStack consumer, ItemType itemType, int needed, int week)
		{
			if (needed < 1)
			{
				return 0;
			}

			int taken = 0;
			List<ModuleStack> visited = new List<ModuleStack>();
			taken += this.takeFromStackOnly(consumer, consumer, itemType, needed - taken, week, visited);

			if (taken < needed)
			{
				visited = new List<ModuleStack>();
				taken += this.takeFromStackTree(consumer, consumer, itemType, needed - taken, week, visited);
			}

			ModuleStack unitRoot = consumer.RootModuleStack;
			if (unitRoot != null && unitRoot != consumer && taken < needed)
			{
				visited = new List<ModuleStack>();
				taken += this.takeFromStackTree(consumer, unitRoot, itemType, needed - taken, week, visited);
			}

			if (taken < needed)
			{
				foreach (ModuleStack source in consumer.orderedSharingSources())
				{
					visited = new List<ModuleStack>();
					taken += this.takeFromStackTree(consumer, source, itemType, needed - taken, week, true, visited);
					if (taken >= needed)
					{
						break;
					}
				}
			}

			return taken;
		}

		public IItemStacksHolder FindItemHolderAvailableTo(ModuleStack consumer, ItemStacks needed)
		{
			if (needed == null)
			{
				return null;
			}

			if (consumer.ItemStacks.Has(needed))
			{
				return consumer;
			}

			ModuleStack unitRoot = consumer.RootModuleStack;
			if (unitRoot != null)
			{
				IItemStacksHolder found = this.findItemHolderInTree(consumer, unitRoot, needed, new List<ModuleStack>());
				if (found != null)
				{
					return found;
				}
			}

			foreach (ModuleStack source in consumer.orderedSharingSources())
			{
				IItemStacksHolder found = this.findItemHolderInTree(consumer, source, needed, new List<ModuleStack>());
				if (found != null)
				{
					return found;
				}
			}

			return null;
		}

		public bool HasItemsWithSharing(ItemStacks needed)
		{
			return this.HasItemsAvailableTo(this, needed);
		}

		public bool ConsumeItemsWithSharing(ItemStacks needed, int week)
		{
			if (needed == null)
			{
				return true;
			}

			foreach (ItemStack itemStack in needed.Values)
			{
				int taken = this.TakeItemsAvailableTo(this, itemStack.ItemType, itemStack.Quantity, week);
				if (taken < itemStack.Quantity)
				{
					return false;
				}
			}
			return true;
		}

		private IItemStacksHolder findItemHolderInTree(ModuleStack consumer, ModuleStack stack, ItemStacks needed, List<ModuleStack> visited)
		{
			if (stack == null || visited.Contains(stack))
			{
				return null;
			}
			visited.Add(stack);

			if (stack.CanSupplyTo(consumer) && stack.ItemStacks.Has(needed))
			{
				return stack;
			}

			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				IItemStacksHolder found = this.findItemHolderInTree(consumer, nested, needed, visited);
				if (found != null)
				{
					return found;
				}
			}

			foreach (Person person in stack.People.Values)
			{
				if (stack.CanSupplyTo(consumer) && person.ItemStacks.Has(needed))
				{
					return person;
				}
			}

			return null;
		}

		private int quantityInTreeForConsumer(ModuleStack stack, ModuleStack consumer, ItemType itemType, List<ModuleStack> visited)
		{
			if (stack == null || visited.Contains(stack))
			{
				return 0;
			}
			visited.Add(stack);

			int quantity = 0;
			if (stack.CanSupplyTo(consumer))
			{
				quantity += stack.ItemStacks.Quantity(itemType);
				foreach (Person person in stack.People.Values)
				{
					quantity += person.ItemStacks.Quantity(itemType);
				}
			}

			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				quantity += this.quantityInTreeForConsumer(nested, consumer, itemType, visited);
			}

			return quantity;
		}

		private List<ModuleStack> orderedSharingSources()
		{
			List<ModuleStack> sources = new List<ModuleStack>();
			List<ModuleStack> snapshot = new List<ModuleStack>(ModuleStack.All.Values);
			foreach (ModuleStack stack in snapshot)
			{
				if (stack == null || !stack.IsFormed || stack == this)
				{
					continue;
				}
				if (!stack.Sharing || stack.Owner != this.Owner)
				{
					continue;
				}
				if (stack.Location != this.Location)
				{
					continue;
				}
				if (stack.RootModuleStack == this.RootModuleStack)
				{
					continue;
				}
				if (this.isUnderSharingAncestor(stack))
				{
					continue;
				}
				sources.Add(stack);
			}
			sources.Sort(delegate(ModuleStack a, ModuleStack b)
			{
				return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
			});
			return sources;
		}

		private bool isUnderSharingAncestor(ModuleStack stack)
		{
			IHolder parent = stack.Parent;
			while (parent is ModuleStack ancestor)
			{
				if (ancestor.Sharing)
				{
					return true;
				}
				parent = ancestor.Parent;
			}
			return false;
		}

		private int takeFromStackTree(ModuleStack consumer, ModuleStack stack, ItemType itemType, int needed, int week, List<ModuleStack> visited)
		{
			return this.takeFromStackTree(consumer, stack, itemType, needed, week, false, visited);
		}

		private int takeFromStackTree(ModuleStack consumer, ModuleStack stack, ItemType itemType, int needed, int week, bool fromExternalSharing, List<ModuleStack> visited)
		{
			int taken = this.takeFromStackOnly(consumer, stack, itemType, needed, week, fromExternalSharing, visited);
			if (taken >= needed || stack == null || stack.ModuleStacks.Count < 1)
			{
				return taken;
			}

			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				if (taken >= needed)
				{
					break;
				}
				taken += this.takeFromStackTree(consumer, nested, itemType, needed - taken, week, fromExternalSharing, visited);
			}

			if (taken < needed && stack.CanSupplyTo(consumer))
			{
				foreach (Person person in stack.People.Values)
				{
					if (taken >= needed)
					{
						break;
					}
					taken += this.takeFromPerson(consumer, person, stack, itemType, needed - taken, week, fromExternalSharing);
				}
			}

			return taken;
		}

		private int takeFromStackOnly(ModuleStack consumer, ModuleStack stack, ItemType itemType, int needed, int week, List<ModuleStack> visited)
		{
			return this.takeFromStackOnly(consumer, stack, itemType, needed, week, false, visited);
		}

		private int takeFromStackOnly(ModuleStack consumer, ModuleStack stack, ItemType itemType, int needed, int week, bool fromExternalSharing, List<ModuleStack> visited)
		{
			if (stack == null || needed < 1 || visited.Contains(stack) || !stack.CanSupplyTo(consumer))
			{
				return 0;
			}
			visited.Add(stack);

			int available = stack.ItemStacks.Quantity(itemType);
			int take = Math.Min(available, needed);
			if (take > 0)
			{
				stack.ItemStacks.Minus(itemType, take);
				this.reportSharedTake(consumer, stack, itemType, take, week, fromExternalSharing || stack != consumer);
			}
			return take;
		}

		private int takeFromPerson(ModuleStack consumer, Person person, ModuleStack host, ItemType itemType, int needed, int week, bool fromExternalSharing)
		{
			if (needed < 1 || person == null || !host.CanSupplyTo(consumer))
			{
				return 0;
			}

			int available = person.ItemStacks.Quantity(itemType);
			int take = Math.Min(available, needed);
			if (take > 0)
			{
				person.ItemStacks.Minus(itemType, take);
				this.reportSharedTake(consumer, host, itemType, take, week, fromExternalSharing || host != consumer);
			}
			return take;
		}

		private void reportSharedTake(ModuleStack consumer, ModuleStack source, ItemType itemType, int take, int week, bool fromSharing)
		{
			if (!fromSharing || week < 1 || source == consumer)
			{
				return;
			}

			consumer.EventReports.Add(
				week,
				string.Format("consumed {0} {1} from shared {2}.",
					take,
					itemType.ReportName,
					source.ReportName));
		}
	}
}
