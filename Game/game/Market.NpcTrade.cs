using System;

namespace SpaceAge
{
	public partial class Market
	{
		private static bool usesNpcMarketFunds(Faction faction)
		{
			return ContractScope.IsNpcFaction(faction);
		}

		private static bool usesNpcMarketSupply(Faction faction)
		{
			return ContractScope.IsNpcFaction(faction);
		}

		private bool npcBuyer()
		{
			return usesNpcMarketFunds(this.Buy.Offerent.Owner);
		}

		private bool npcSeller()
		{
			return usesNpcMarketSupply(this.Sell.Offerent.Owner);
		}

		private ModuleStack createNpcSupplyModuleStack(ModuleStack seller, ModuleType moduleType, int quantity)
		{
			ModuleStack supplyStack = new ModuleStack(seller, seller.Owner, moduleType);
			supplyStack.AddModules(quantity);
			return supplyStack;
		}

		private void deliverPurchasedItems(int week, ItemStack boughtItems, int transportTime, bool crossRegionDelivery, Region buyerRegion, ModuleStack sellerStack, int purchaseValue, int transferCost)
		{
			if (!this.npcSeller())
			{
				try
				{
					this.Sell.Offerent.ItemStacks.Remove(boughtItems);
				}
				catch (Exception ex)
				{
					throw new Exception(string.Concat("Offerent: ", this.Sell.Offerent.ReportName), ex);
				}
			}

			if (crossRegionDelivery)
			{
				DeliveringPurchase delivery = new DeliveringPurchase(
					ModuleStack.All[this.Buy.Offerent.Name],
					this.Sell.Offerent,
					sellerStack,
					boughtItems,
					purchaseValue,
					transferCost,
					buyerRegion,
					transportTime,
					false);
				delivery.Execute(week);
			}
			else
			{
				ReceivingItems transferItems = new ReceivingItems(this.Buy.Offerent, this.Sell.Offerent, boughtItems, transportTime);
				transferItems.Execute(week);
			}
		}

		private void deliverPurchasedModules(int week, int quantity, int transportTime, bool crossRegionDelivery, Region buyerRegion, ModuleStack sellerStack, int purchaseValue, int transferCost)
		{
			ModuleStack buyerStack = ModuleStack.All[this.Buy.Offerent.Name];
			if (crossRegionDelivery)
			{
				ModuleStack transferringStack;
				if (this.npcSeller())
				{
					transferringStack = this.createNpcSupplyModuleStack(sellerStack, this.Sell.ModuleType, quantity);
				}
				else
				{
					transferringStack = new ModuleStack(sellerStack, sellerStack.Owner, this.Sell.ModuleType);
					for (int i = 0; i < quantity; i++)
					{
						transferringStack.AddModule(sellerStack.Modules[0]);
						sellerStack.RemoveModule(0);
					}
				}

				DeliveringPurchase delivery = new DeliveringPurchase(
					buyerStack,
					sellerStack,
					sellerStack,
					transferringStack,
					purchaseValue,
					transferCost,
					buyerRegion,
					transportTime,
					false);
				delivery.Execute(week);
				return;
			}

			if (this.npcSeller())
			{
				ModuleStack transferringStack = this.createNpcSupplyModuleStack(sellerStack, this.Sell.ModuleType, quantity);
				ReceivingModules transferModules = new ReceivingModules(buyerStack, sellerStack, transferringStack, transportTime);
				transferModules.Execute(week);
				return;
			}

			TransferOrder transferModulesOrder = new TransferOrder(
				ModuleStack.All[this.Sell.Offerent.Name],
				buyerStack,
				this.Sell.ModuleType,
				quantity,
				transportTime);
			transferModulesOrder.Execute(week);
		}

		private void deliverPurchasedTechnology(int week, int transportTime)
		{
			ModuleStack buyerStack = ModuleStack.All[this.Buy.Offerent.Name];
			ModuleStack sellerStack = ModuleStack.All[this.Sell.Offerent.Name];
			if (this.npcSeller())
			{
				ReceivingTechnology effect = new ReceivingTechnology(
					buyerStack,
					sellerStack,
					this.Sell.Technology,
					transportTime);
				effect.Execute(week);
				return;
			}

			CopyOrder copyTechnology = new CopyOrder(sellerStack, buyerStack, this.Sell.Technology);
			copyTechnology.Execute(week);
		}
	}
}
