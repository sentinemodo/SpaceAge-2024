using System;
using System.Collections.Generic;

namespace SpaceAge
{
	public partial class Market
	{
		public const int SellOutlierFactor = 10;
		public const int BuyOutlierFactor = 10;
		public const int MaxQuarterlyDriftPercent = 10;

		public static bool IsEconomicallySignificantSell(int askPrice, int listPrice)
		{
			if (askPrice < 1)
			{
				return false;
			}
			if (listPrice < 1)
			{
				return true;
			}
			if (askPrice <= listPrice)
			{
				return true;
			}
			return askPrice <= listPrice * SellOutlierFactor;
		}

		public static bool IsEconomicallySignificantBuy(int bidPrice, int listPrice)
		{
			if (bidPrice < 1)
			{
				return false;
			}
			if (listPrice < 1)
			{
				return true;
			}
			if (bidPrice >= listPrice)
			{
				return true;
			}
			return bidPrice * BuyOutlierFactor >= listPrice;
		}

		public int GetOfferAskPrice(Offer sell)
		{
			if (sell.Price > 0)
			{
				return sell.Price;
			}
			int price;
			if (sell.ItemType != null)
			{
				price = (int)this.GetPrice(sell.ItemType);
			}
			else if (sell.ModuleType != null)
			{
				price = (int)this.GetPrice(sell.ModuleType);
			}
			else if (sell.Technology != null)
			{
				price = (int)this.GetPrice(sell.Technology);
			}
			else
			{
				return 0;
			}
			if (price < 1)
			{
				price = 1;
			}
			return price;
		}

		public void PostTradePrice(Offer sell, NamedType type)
		{
			if (sell.Price <= 0)
			{
				return;
			}
			int unitPrice = sell.Price;
			int listPrice = (int)this.GetPrice(type);
			if (!IsEconomicallySignificantSell(unitPrice, listPrice))
			{
				return;
			}
			this.AddPrice(type, unitPrice);
		}

		public void ApplyOfferPressureDrift()
		{
			HashSet<ItemType> itemTypes = new HashSet<ItemType>();
			HashSet<ModuleType> moduleTypes = new HashSet<ModuleType>();
			HashSet<Technology> technologies = new HashSet<Technology>();

			foreach (Offer offer in Offer.All[this])
			{
				switch (offer.OfferType)
				{
					case EOfferType.BuyItems:
					case EOfferType.SellItems:
						if (offer.ItemType != null)
						{
							itemTypes.Add(offer.ItemType);
						}
						break;
					case EOfferType.BuyModules:
					case EOfferType.SellModules:
						if (offer.ModuleType != null)
						{
							moduleTypes.Add(offer.ModuleType);
						}
						break;
					case EOfferType.BuyTechnologies:
					case EOfferType.SellTechnologies:
						if (offer.Technology != null)
						{
							technologies.Add(offer.Technology);
						}
						break;
				}
			}

			foreach (ItemType itemType in itemTypes)
			{
				this.applyOfferPressureDrift(itemType);
			}
			foreach (ModuleType moduleType in moduleTypes)
			{
				this.applyOfferPressureDrift(moduleType);
			}
			foreach (Technology technology in technologies)
			{
				this.applyOfferPressureDrift(technology);
			}

			foreach (NamedType type in new List<NamedType>(this.PriceList.Keys))
			{
				if (type is ItemType && !itemTypes.Contains((ItemType)type))
				{
					this.applyOfferPressureDrift(type);
				}
				else if (type is ModuleType && !moduleTypes.Contains((ModuleType)type))
				{
					this.applyOfferPressureDrift(type);
				}
				else if (type is Technology && !technologies.Contains((Technology)type))
				{
					this.applyOfferPressureDrift(type);
				}
			}
		}

		private void applyOfferPressureDrift(NamedType type)
		{
			int listPrice = (int)this.GetPrice(type);
			if (listPrice < 1)
			{
				return;
			}

			int? maxBidBelow = null;
			int? minAskAbove = null;

			foreach (Offer offer in Offer.All[this])
			{
				if (this.isBuyOfferForType(offer, type))
				{
					int bid = offer.GetEffectiveBidCap();
					if (bid < listPrice && IsEconomicallySignificantBuy(bid, listPrice))
					{
						if (!maxBidBelow.HasValue || bid > maxBidBelow.Value)
						{
							maxBidBelow = bid;
						}
					}
				}
				else if (this.isSellOfferForType(offer, type))
				{
					int ask = this.GetOfferAskPrice(offer);
					if (ask > listPrice && IsEconomicallySignificantSell(ask, listPrice))
					{
						if (!minAskAbove.HasValue || ask < minAskAbove.Value)
						{
							minAskAbove = ask;
						}
					}
				}
			}

			if (!maxBidBelow.HasValue && !minAskAbove.HasValue)
			{
				return;
			}

			int target = listPrice;
			if (maxBidBelow.HasValue && minAskAbove.HasValue)
			{
				target = (maxBidBelow.Value + minAskAbove.Value) / 2;
			}
			else if (maxBidBelow.HasValue)
			{
				target = maxBidBelow.Value;
			}
			else
			{
				target = minAskAbove.Value;
			}

			int nextPrice = this.applyQuarterlyDriftCap(listPrice, target);
			if (nextPrice != listPrice)
			{
				this.AddPrice(type, nextPrice);
			}
		}

		private bool isBuyOfferForType(Offer offer, NamedType type)
		{
			if (type is ItemType)
			{
				return offer.OfferType == EOfferType.BuyItems && offer.ItemType == type;
			}
			if (type is ModuleType)
			{
				return offer.OfferType == EOfferType.BuyModules && offer.ModuleType == type;
			}
			return offer.OfferType == EOfferType.BuyTechnologies && offer.Technology == type;
		}

		private bool isSellOfferForType(Offer offer, NamedType type)
		{
			if (type is ItemType)
			{
				return offer.OfferType == EOfferType.SellItems && offer.ItemType == type;
			}
			if (type is ModuleType)
			{
				return offer.OfferType == EOfferType.SellModules && offer.ModuleType == type;
			}
			return offer.OfferType == EOfferType.SellTechnologies && offer.Technology == type;
		}

		private int applyQuarterlyDriftCap(int current, int target)
		{
			int maxDelta = Math.Max(1, current / MaxQuarterlyDriftPercent);
			if (target < current)
			{
				return Math.Max(target, current - maxDelta);
			}
			if (target > current)
			{
				return Math.Min(target, current + maxDelta);
			}
			return current;
		}
	}
}
