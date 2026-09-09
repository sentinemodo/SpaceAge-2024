
namespace SpaceAge
{
	public partial class ModuleStack
	{
		#region IItemStacksHolder Members

		private ItemStacks itemStacks = new ItemStacks();
		public ItemStacks ItemStacks
		{
			get { return this.itemStacks; }
		}

        public ItemStack ItemStackSumRecursive(ItemType itemType)
        {
            ItemStack stack = new ItemStack(itemType, 0);
            if (this.ItemStacks.ContainsKey(itemType))
            {
                stack.Quantity += this.ItemStacks[itemType].Quantity;
            }
            if (this.HasModuleStacks())
            {
                foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
                {
                    stack.Quantity += moduleStack.ItemStackSumRecursive(itemType).Quantity;
                }
            }
            if (this.HasPeople)
            {
                foreach (Person person in this.People.Values)
                {
                    stack.Quantity += person.ItemStackSumRecursive(itemType).Quantity;
                }
            }
            return stack;
        }

        // Number of modules of a given module type held by this stack and, recursively,
        // its nested sub-stacks. Mirrors ItemStackSumRecursive for modules.
        public int ModuleCountRecursive(ModuleType moduleType)
        {
            int count = 0;
            if (this.moduleType == moduleType)
            {
                count += this.Quantity;
            }
            foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
            {
                count += moduleStack.ModuleCountRecursive(moduleType);
            }
            return count;
        }

        public ItemStacks ItemStacksSumRecursive
        {
            get
            {
                ItemStacks stacks = new ItemStacks();
                stacks.Sum(this.ItemStacks);

                if (this.HasModuleStacks())
                {
                    foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
                    {
                        stacks.Sum(moduleStack.ItemStacksSumRecursive);
                    }
                }
                if (this.HasPeople)
                {
                    foreach (Person person in this.People.Values)
                    {
                        stacks.Sum(person.ItemStacks);
                    }
                }
                return stacks;
            }
        }

		public People People
		{
			get { return Person.All[this, false]; }
		}

        public bool HasPeople
        {
            get
            {
                if (this.People.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }
		#endregion
	}
}
