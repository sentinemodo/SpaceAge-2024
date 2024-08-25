using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public interface IItemStacksHolder : IHolder, IReporting, IEventReporting, ISingular
	{
		bool Visible(Faction faction);
		ItemStacks ItemStacks { get; }
		bool IsFormed { get; }
		ItemStacks ItemStacksSumRecursive { get; }
		ItemStack ItemStackSumRecursive(ItemType itemType);
	}
}
