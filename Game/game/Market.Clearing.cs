using System;
using System.Collections.Generic;

namespace SpaceAge
{
	public partial class Market
	{
		public void ProcessBuyClearing(int week)
		{
			HashSet<ItemType> itemTypes = new HashSet<ItemType>();
			HashSet<ModuleType> moduleTypes = new HashSet<ModuleType>();
			EOfferType buyItems = EOfferType.BuyItems;
			EOfferType buyModules = EOfferType.BuyModules;
			foreach (Offer offer in Offer.All[buyItems])
			{
				if (offer.Offerent.Location.Market == this)
				{
					itemTypes.Add(offer.ItemType);
				}
			}
			foreach (Offer offer in Offer.All[buyModules])
			{
				if (offer.Offerent.Location.Market == this)
				{
					moduleTypes.Add(offer.ModuleType);
				}
			}

			foreach (ItemType itemType in itemTypes)
			{
				this.processItemBuyClearing(week, itemType);
			}
			foreach (ModuleType moduleType in moduleTypes)
			{
				this.processModuleBuyClearing(week, moduleType);
			}
		}

		private void processItemBuyClearing(int week, ItemType itemType)
		{
			bool processed = true;
			while (processed)
			{
				processed = false;
				Offer sellOffer = this.findLowestRegionalSell(EOfferType.SellItems, itemType);
				if (sellOffer == null)
				{
					break;
				}

				Offers tier = this.findHighestBidTier(EOfferType.BuyItems, itemType, sellOffer);
				if (tier.Count == 0)
				{
					break;
				}

				Offer sampleBuy = tier.GetIndex(0);
				int sellQuantity = this.availableSellQuantity(sellOffer, sampleBuy);
				if (sellQuantity < 1)
				{
					break;
				}

				if (this.allocateProRata(week, tier, sellOffer, sellQuantity))
				{
					processed = true;
				}
			}
		}

		private void processModuleBuyClearing(int week, ModuleType moduleType)
		{
			bool processed = true;
			while (processed)
			{
				processed = false;
				Offer sellOffer = this.findLowestRegionalSell(EOfferType.SellModules, moduleType);
				if (sellOffer == null)
				{
					break;
				}

				Offers tier = this.findHighestBidTier(EOfferType.BuyModules, moduleType, sellOffer);
				if (tier.Count == 0)
				{
					break;
				}

				Offer sampleBuy = tier.GetIndex(0);
				int sellQuantity = sellOffer.Quantity;
				if (sellQuantity < 1)
				{
					break;
				}

				if (this.allocateProRata(week, tier, sellOffer, sellQuantity))
				{
					processed = true;
				}
			}
		}

		private Offer findLowestRegionalSell(EOfferType sellType, NamedType type)
		{
			Offers sells;
			if (sellType == EOfferType.SellItems)
			{
				sells = Offer.All[EOfferType.SellItems][(ItemType)type];
			}
			else
			{
				sells = Offer.All[EOfferType.SellModules][(ModuleType)type];
			}

			Offer bestOffer = null;
			foreach (Offer offer in sells)
			{
				if (offer.Offerent.Location.Market != this)
				{
					continue;
				}
				if (bestOffer == null || offer.Price < bestOffer.Price)
				{
					bestOffer = offer;
				}
			}
			return bestOffer;
		}

		private Offers findHighestBidTier(EOfferType buyType, NamedType type, Offer sellOffer)
		{
			Offers buys;
			if (buyType == EOfferType.BuyItems)
			{
				buys = Offer.All[EOfferType.BuyItems][(ItemType)type];
			}
			else
			{
				buys = Offer.All[EOfferType.BuyModules][(ModuleType)type];
			}

			Offers eligible = new Offers();
			int highestBid = int.MinValue;
			foreach (Offer buy in buys)
			{
				if (buy.Offerent.Location.Market != this)
				{
					continue;
				}
				if (buy.Offerent.Owner == sellOffer.Offerent.Owner)
				{
					continue;
				}
				if (buy.MatchesAsk(this.getSellAskPrice(sellOffer)))
				{
					int bidCap = buy.GetEffectiveBidCap();
					if (bidCap > highestBid)
					{
						highestBid = bidCap;
						eligible.Clear();
					}
					if (bidCap == highestBid)
					{
						eligible.Add(buy);
					}
				}
			}
			eligible.Sort(Offers.CompareByNames);
			return eligible;
		}

		private bool allocateProRata(int week, Offers tier, Offer sellOffer, int sellQuantity)
		{
			List<BuyerAllocation> allocations = new List<BuyerAllocation>();
			int totalWeight = 0;
			foreach (Offer buy in tier)
			{
				if (buy.Offerent.Owner == sellOffer.Offerent.Owner)
				{
					continue;
				}
				int weight = this.buyerMaxQuantity(buy, sellOffer);
				if (weight > 0)
				{
					allocations.Add(new BuyerAllocation(buy, weight));
					totalWeight += weight;
				}
			}

			if (allocations.Count == 0 || totalWeight == 0)
			{
				if (tier.Count > 0)
				{
					int ask = this.getSellAskPrice(sellOffer);
					foreach (Offer buy in tier)
					{
						if (this.availableFunds(buy) < ask)
						{
							buy.Offerent.EventReports.Add(week, "BUY failed, not enough cash.");
						}
					}
				}
				return false;
			}

			int toAllocate = Math.Min(sellQuantity, totalWeight);
			int allocated = 0;
			foreach (BuyerAllocation allocation in allocations)
			{
				allocation.Quantity = (toAllocate * allocation.Weight) / totalWeight;
				allocated += allocation.Quantity;
			}
			int remainder = toAllocate - allocated;
			for (int i = 0; remainder > 0 && i < allocations.Count; i++)
			{
				allocations[i].Quantity++;
				remainder--;
			}

			bool anyTrade = false;
			foreach (BuyerAllocation allocation in allocations)
			{
				if (allocation.Quantity > 0 && this.executeTrade(week, allocation.Buy, sellOffer, allocation.Quantity))
				{
					anyTrade = true;
				}
			}
			return anyTrade;
		}

		private int buyerMaxQuantity(Offer buy, Offer sell)
		{
			int ask = this.getSellAskPrice(sell);
			if (ask < 1)
			{
				return 0;
			}
			bool crossRegion = buy.Offerent.Location != sell.Offerent.Location;
			int transferCost = crossRegion ? 100 : 0;
			int funds = this.availableFunds(buy);
			if (funds <= transferCost)
			{
				return 0;
			}
			int maxByBudget = (funds - transferCost) / ask;

			int maxByCapacity = int.MaxValue;
			if (buy.OfferType == EOfferType.BuyItems)
			{
				int freeCapacity = (int)(buy.Offerent.Capacity - buy.Offerent.CapacityUsed);
				maxByCapacity = (int)(freeCapacity / buy.ItemType.Size);
			}
			else if (buy.OfferType == EOfferType.BuyModules)
			{
				ModuleStack parentStack = buy.Offerent.Parent as ModuleStack;
				if (parentStack != null)
				{
					int freeCapacity = (int)(parentStack.Capacity - parentStack.CapacityUsed);
					maxByCapacity = (int)(freeCapacity / buy.ModuleType.Size);
				}
			}

			int maxByDemand = buy.AllQuantity ? int.MaxValue : buy.Quantity;
			if (maxByDemand > 1000000)
			{
				maxByDemand = maxByBudget;
			}
			return Math.Max(0, Math.Min(Math.Min(maxByBudget, maxByCapacity), maxByDemand));
		}

		private bool executeTrade(int week, Offer buy, Offer sell, int quantity)
		{
			this.Buy = buy;
			this.Sell = sell;
			int transactionCost = this.calculateTransactionCost(quantity);
			if (!this.canBuy(week, transactionCost, quantity))
			{
				return false;
			}

			this.executeTransaction(week, transactionCost, quantity);
			sell.Quantity -= quantity;
			if (sell.Quantity <= 0)
			{
				Offer.All.Remove(sell);
			}

			if (!buy.AllQuantity)
			{
				buy.Quantity -= quantity;
				if (buy.Quantity <= 0)
				{
					Offer.All.Remove(buy);
				}
			}

			this.markBuyOrderExecuted(buy);
			return true;
		}

		private void markBuyOrderExecuted(Offer buy)
		{
			if (buy.BuyOrder != null)
			{
				buy.BuyOrder.Executed = true;
			}
		}

		private int getSellAskPrice(Offer sell)
		{
			if (sell.Price > 0)
			{
				return sell.Price;
			}
			int price;
			if (sell.ItemType != null)
			{
				price = (int)sell.Market.GetPrice(sell.ItemType);
			}
			else
			{
				price = (int)sell.Market.GetPrice(sell.ModuleType);
			}
			if (price < 1)
			{
				price = 1;
			}
			return price;
		}

		private class BuyerAllocation
		{
			public BuyerAllocation(Offer buy, int weight)
			{
				this.Buy = buy;
				this.Weight = weight;
			}

			public Offer Buy { get; private set; }
			public int Weight { get; private set; }
			public int Quantity { get; set; }
		}
	}
}
