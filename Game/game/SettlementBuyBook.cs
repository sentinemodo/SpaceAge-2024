using System.Collections.Generic;

namespace SpaceAge
{
	internal struct SettlementBuyTarget
	{
		public ItemType ItemType;
		public ModuleType ModuleType;
		public int Quantity;
		public int Price;

		public bool IsModule
		{
			get { return this.ModuleType != null; }
		}
	}

	/// <summary>
	/// Default NPC settlement buy books. Quantities scale with settlement stack module count.
	/// Canonical design: play/designer/economy.md
	/// </summary>
	internal static class SettlementBuyBook
	{
		public static IEnumerable<SettlementBuyTarget> Targets(string settlementModuleName, int stackQuantity)
		{
			if (stackQuantity < 1)
			{
				stackQuantity = 1;
			}

			switch (settlementModuleName)
			{
				case "town":
					return townTargets(stackQuantity);
				case "city":
					return cityTargets(stackQuantity);
				case "mtrply":
					return metropolyTargets(stackQuantity);
				default:
					return new SettlementBuyTarget[0];
			}
		}

		private static IEnumerable<SettlementBuyTarget> townTargets(int stackQuantity)
		{
			yield return itemTarget("food", 60 * stackQuantity, 1);
			yield return itemTarget("iron", 15 * stackQuantity, 1);
			yield return itemTarget("carbon", 15 * stackQuantity, 2);
		}

		private static IEnumerable<SettlementBuyTarget> cityTargets(int stackQuantity)
		{
			yield return itemTarget("food", 100 * stackQuantity, 1);
			yield return itemTarget("iron", 25 * stackQuantity, 1);
			yield return itemTarget("carbon", 25 * stackQuantity, 2);
			yield return itemTarget("silici", 20 * stackQuantity, 2);
			yield return itemTarget("titani", 10 * stackQuantity, 2);

			yield return moduleTarget("cargob", 1, 100);
			yield return moduleTarget("farms", 1, 100);
			yield return moduleTarget("wnplnt", 1, 50);
			yield return moduleTarget("cplant", 1, 100);
			yield return moduleTarget("sdrill", 1, 100);
			yield return moduleTarget("cdrill", 1, 100);
			yield return moduleTarget("factry", 1, 100);
		}

		private static IEnumerable<SettlementBuyTarget> metropolyTargets(int stackQuantity)
		{
			yield return itemTarget("food", 500 * stackQuantity, 1);
			yield return itemTarget("iron", 40 * stackQuantity, 1);
			yield return itemTarget("carbon", 40 * stackQuantity, 2);
			yield return itemTarget("silici", 30 * stackQuantity, 2);
			yield return itemTarget("titani", 25 * stackQuantity, 2);
			yield return itemTarget("copper", 25 * stackQuantity, 2);
			yield return itemTarget("uraniu", 10 * stackQuantity, 4);

			yield return moduleTarget("cargob", 2, 100);
			yield return moduleTarget("farms", 2, 100);
			yield return moduleTarget("wnplnt", 2, 50);
			yield return moduleTarget("cplant", 2, 100);
			yield return moduleTarget("sdrill", 2, 100);
			yield return moduleTarget("cdrill", 2, 100);
			yield return moduleTarget("factry", 2, 100);
		}

		private static SettlementBuyTarget itemTarget(string itemName, int quantity, int price)
		{
			SettlementBuyTarget target = new SettlementBuyTarget();
			target.ItemType = ItemType.All[itemName];
			target.Quantity = quantity;
			target.Price = price;
			return target;
		}

		private static SettlementBuyTarget moduleTarget(string moduleName, int quantity, int price)
		{
			SettlementBuyTarget target = new SettlementBuyTarget();
			target.ModuleType = ModuleType.All[moduleName];
			target.Quantity = quantity;
			target.Price = price;
			return target;
		}
	}
}
