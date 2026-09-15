using System.Collections.Generic;

namespace SpaceAge
{
	public partial class ModuleStack
	{
		private readonly Dictionary<ItemType, int> itemHolds = new Dictionary<ItemType, int>();

		public int GetItemHold(ItemType itemType)
		{
			if (this.itemHolds.ContainsKey(itemType))
			{
				return this.itemHolds[itemType];
			}
			return 0;
		}

		public void SetItemHold(ItemType itemType, int quantity)
		{
			if (quantity <= 0)
			{
				this.itemHolds.Remove(itemType);
			}
			else
			{
				this.itemHolds[itemType] = quantity;
			}
		}

		public int GetTransferrableQuantity(ItemType itemType)
		{
			if (!this.ItemStacks.ContainsKey(itemType))
			{
				return 0;
			}
			int available = this.ItemStacks[itemType].Quantity - this.GetItemHold(itemType);
			return available > 0 ? available : 0;
		}
	}
}
