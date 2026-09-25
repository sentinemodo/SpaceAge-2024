using System;

namespace SpaceAge
{
	public static class GrantCost
	{
		public const int TechnologyCreditMultiplier = 125;
		public const int SkillCreditsPerWeek = 150;
		public const int ItemValueMultiplier = 4;
		public const int ModuleCostFloor = 250;
		public const int MaxCreditsPerLine = 15000;

		public static int TechnologyCredits(Technology technology)
		{
			if (technology == null)
			{
				return 0;
			}
			return TechnologyCreditMultiplier * technology.Cost;
		}

		public static int SkillCredits(SkillType skillType)
		{
			if (skillType == null)
			{
				return 0;
			}
			return SkillCreditsPerWeek * skillType.TrainingDuration;
		}

		public static int ItemCredits(ItemType itemType, int quantity)
		{
			if (itemType == null || quantity <= 0)
			{
				return 0;
			}
			if (itemType.Name == "cash")
			{
				return quantity;
			}
			int nominal = itemType.NominalValue;
			if (nominal < 1)
			{
				nominal = 2;
			}
			return quantity * nominal * ItemValueMultiplier;
		}

		public static int ModuleCredits(ModuleType moduleType, int quantity)
		{
			if (moduleType == null || quantity <= 0)
			{
				return 0;
			}
			Technology producer = Technology.All.FindProducerFor(moduleType);
			int buildCost = BuildCostFromUseConsume(producer);
			int level = producer != null ? producer.Level : 0;
			int unitCost = Math.Max(ModuleCostFloor, 5 * buildCost + 100 * level);
			return quantity * unitCost;
		}

		public static int BuildCostFromUseConsume(Technology technology)
		{
			if (technology == null || technology.UseConsumeItems == null)
			{
				return 0;
			}
			int total = 0;
			foreach (ItemStack stack in technology.UseConsumeItems.Values)
			{
				int nominal = stack.ItemType.NominalValue;
				if (nominal < 1)
				{
					nominal = 2;
				}
				total += stack.Quantity * nominal;
			}
			return total;
		}
	}
}
