using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Market : IReporting
	{
        public Offer Sell { get; set; }
        public Offer Buy { get; set; }

        readonly private Dictionary<NamedType, double> priceList = new Dictionary<NamedType, double>();
        public Dictionary<NamedType, double> PriceList
        {
            get { return this.priceList; }
        }

        public void AddPrice(NamedType type, double price)
        {
            if (this.PriceList.ContainsKey(type))
            {
                this.PriceList[type] = price;
            }
            else
            {
                this.PriceList.Add(type, price);
            }
        }

        private double getAveragePrice(NamedType type)
        {
            double sum = 0;
            double i = 0;

            foreach (Region region in Region.All.Values)
            {
                if (region.Market.PriceList.ContainsKey(type))
                {
                    sum += region.Market.PriceList[type];
                    i++;
                }
            }
            return (i > 0) ? (sum / i) : 0;
        }
        
        public double GetPrice(NamedType type)
        {
            if (!this.PriceList.ContainsKey(type))
            {
                double price = this.getAveragePrice(type);
                // TODO: add nominal itemtype values to xml 
                this.PriceList.Add(type, price);
            }
            return this.PriceList[type];
        }
        
        private int availableSellQuantity()
        {
            if (this.Sell == null)
            {
                return 0;
            }
            int listed = this.Sell.Quantity;
            if (this.Buy == null || this.Buy.OfferType != EOfferType.BuyItems || this.Buy.ItemType == null)
            {
                return listed;
            }
            int onHand = this.Sell.Offerent.ItemStacks.Quantity(this.Buy.ItemType);
            if (onHand < listed)
            {
                return onHand;
            }
            return listed;
        }

        private bool canBuy(int week, int amount)
		{
			//TODO: has cash & has cash for service as of now you need to have either full amount in account & has access to it, or have full amount in cash

            // can pay
            if ((this.Buy.Offerent.HasBankAccess & this.Buy.Offerent.Owner.Bank.AvailableFunds < amount) 
                & (this.Buy.Offerent.ItemStacks.Quantity(ItemType.All.Cash) < amount))
            {
                this.Buy.Offerent.EventReports.Add(week, "BUY failed, not enough cash.");
                return false;
            }

            switch (this.Buy.OfferType)
            {
                case EOfferType.BuyItems:
                    // have space for items
                    if (this.Buy.Offerent.Capacity < this.Buy.Offerent.CapacityUsed + this.Buy.ItemType.Size * this.Buy.Quantity)
                    {
                        this.Buy.Offerent.EventReports.Add(week, "BUY failed, not enough space to buy items.");
                        return false;
                    }
                    break;
                case EOfferType.BuyModules:
                    // have space for modules
                    if (this.Buy.Offerent.Parent.Capacity < this.Buy.Offerent.Parent.CapacityUsed + this.Buy.ModuleType.Size * this.Buy.Quantity)
                    {
                        this.Buy.Offerent.EventReports.Add(week, "BUY failed, not enough space to buy modules.");
                        return false;
                    }
                    break;
                case EOfferType.BuyTechnologies:
                    // have space for technology
                    if (this.Buy.Offerent.TechnologyCapacity < this.Buy.Offerent.TechnologyCapacityUsed + this.Buy.Technology.Level)
                    {
                        this.Buy.Offerent.EventReports.Add(week, "BUY failed, not enough space to buy items.");
                        return false;
                    }
                    break;
                default:
                    throw new InvalidOperationException("Unexpected offer type = " + this.Buy.OfferType);
            }

            return true;
		}

		private bool isCloser(Region currentRegion, Region testedRegion)
		{
			Region buyerRegion = (Region)this.Buy.Offerent.Location;
			if (Region.All.DistanceBetween(buyerRegion, currentRegion) > Region.All.DistanceBetween(buyerRegion, testedRegion))
			{
				return true;
			}
			return false;
		}

        public Offer FindMatch(Offer buyOffer)
		{
            Offers offers = this.availableOffers(buyOffer, buyOffer.Everywhere);

   			Offer bestOffer = null;
			foreach (Offer offer in offers)
			{
                 if ((buyOffer.Price <= 0) | (offer.Price <= buyOffer.Price))
				{
                    if (bestOffer == null)
                    {
                        // first matching offer
                        bestOffer = offer;
                    }
                    else
                    {
                        if ((bestOffer.Price > offer.Price) | 
                                ((bestOffer.Price == offer.Price 
                                & this.isCloser((Region)bestOffer.Offerent.Location, (Region)offer.Offerent.Location))))
                        {
                            // better offer or same offer but closer (this is not smart enoguh to factor the transaction costs)
                            bestOffer = offer;
                        }
                    }
				}
			}
			return bestOffer;
		}

        private Offers availableOffers(Offer offer, bool everywhere)
		{

			Offers offers;
            switch (offer.OfferType)
			{
				case EOfferType.BuyItems:
                    offers = Offer.All[EOfferType.SellItems][offer.ItemType];
					break;
				case EOfferType.BuyModules:
					offers = Offer.All[EOfferType.SellModules][offer.ModuleType];
					break;
				case EOfferType.BuyTechnologies:
					offers = Offer.All[EOfferType.SellTechnologies][offer.Technology][offer.Offerent.Location]; // technology may only be bought locally
					break;
                case EOfferType.SellItems:
                    offers = Offer.All[EOfferType.BuyItems][offer.ItemType];
                    break;
                case EOfferType.SellModules:
                    offers = Offer.All[EOfferType.BuyModules][offer.ModuleType];
                    break;
                case EOfferType.SellTechnologies:
                    offers = Offer.All[EOfferType.BuyTechnologies][offer.Technology][offer.Offerent.Location]; // technology may only be sold locally
                    break;
                default:
                    throw new InvalidOperationException("Unexpected offer type = " + offer.OfferType);
			}

            // technology may only be sold locally
            if (offer.OfferType != EOfferType.BuyTechnologies 
                & offer.OfferType != EOfferType.SellTechnologies 
                & !everywhere)
            {
                offers = offers[offer.Offerent.Location];
            }

            // exclude offerent offers (to avoid buying from self
            offers = offers[offer.Offerent.Owner, true];

			return offers;
		}

		private int calculateTransactionCost(int quantity)
		{
            // distance	service cost
            int transactionCost = Convert.ToInt32(quantity * this.Sell.Price);
            // TODO: migrate the transfercost to XML
            int transferCost = 100;

            if (this.Buy.Offerent.Location != this.Sell.Offerent.Location)
            {
                transactionCost = transactionCost + transferCost;
            }

            return transactionCost;
        }


		private int calculateTransportTime()
		{
            // TODO: migrate initial transfer time to xml
			int transportTime = 1;

			// distance	service time
			if (this.Buy.Offerent.Location != this.Sell.Offerent.Location)
			{
				transportTime = Region.All.DistanceBetween(
					(Region)this.Buy.Offerent.Location, 
					(Region)this.Sell.Offerent.Location) * 2;
			}
			return transportTime;
		}

		private void executeTransaction(int week, int transactionValue, int quantity)
		{
			int transportTime = this.calculateTransportTime();
            
            if (this.Buy.Offerent.HasBankAccess)
            {
                this.Buy.Offerent.Owner.Bank.Debit(week, transactionValue, string.Concat("payment to ", this.Sell.Offerent.Owner.ReportName, "."));
            } else
            {
                this.Buy.Offerent.ItemStacks.Remove(ItemStack.Cash(transactionValue));
            }
            if (this.Sell.Offerent.HasBankAccess)
            {
                this.Sell.Offerent.Owner.Bank.Credit(week, transactionValue, string.Concat("payment from ", this.Buy.Offerent.Owner.ReportName, "."));
            }
            else
            {
                ReceivingItems itemsTransfer = new ReceivingItems(this.Sell.Offerent, this.Buy.Offerent, ItemStack.Cash(transactionValue), transportTime);
                itemsTransfer.Execute(week);
            }
			switch (this.Buy.OfferType)
			{
				case EOfferType.BuyItems:
                    this.Sell.Market.AddPrice(this.Sell.ItemType, this.Sell.Price);                    
					ItemStack boughtItems = new ItemStack(this.Buy.ItemType, quantity);
                    try
                    {
                        this.Sell.Offerent.ItemStacks.Remove(boughtItems);
                    }
			        catch (Exception ex)
			        {
				        throw new Exception(string.Concat("Offerent: ", this.Sell.Offerent.ReportName), ex);
			        }
					ReceivingItems transferItems = new ReceivingItems(this.Buy.Offerent, this.Sell.Offerent, boughtItems, transportTime);
                    transferItems.Execute(week);
					break;
				case EOfferType.BuyModules:
                    this.Sell.Market.AddPrice(this.Sell.ModuleType, this.Sell.Price);
                    TransferOrder transferModules = new TransferOrder(
                        ModuleStack.All[this.Sell.Offerent.Name], 
                        ModuleStack.All[this.Buy.Offerent.Name], 
                        this.Sell.ModuleType, 
                        quantity, 
                        transportTime);
					transferModules.Execute(week);
                    break;
                case EOfferType.BuyTechnologies:
                    this.Sell.Market.AddPrice(this.Sell.Technology, this.Sell.Price);
                    CopyOrder copyTechnology = new CopyOrder(
                        ModuleStack.All[this.Sell.Offerent.Name], 
                        ModuleStack.All[this.Buy.Offerent.Name], 
                        this.Sell.Technology);
                    copyTechnology.Execute(week);
                    break;
                default:
                    throw new InvalidOperationException("Unexpected offer type = " + this.Buy.OfferType);
            }
		}

		public bool ProcessOffer(int week, Offer buyOffer)
		{
			switch (buyOffer.OfferType)
			{
				case EOfferType.SellItems:
				case EOfferType.SellModules:
				case EOfferType.SellTechnologies:
					return false;
			}

			this.Buy = buyOffer;
            this.Sell = this.FindMatch(buyOffer);

			if (this.Sell != null)
			{
				int sellQuantity = this.availableSellQuantity();
				if (sellQuantity < 1)
				{
					return false;
				}
                int transactionCost;
				if (this.Buy.Quantity == sellQuantity)
				{
					// both are fullfilled
					transactionCost = this.calculateTransactionCost(sellQuantity);
					if (this.canBuy(week, transactionCost))
					{
						this.executeTransaction(week, transactionCost, sellQuantity);
                        Offer.All.Remove(this.Sell);
                        Offer.All.Remove(this.Buy);
                    }
				}
				else if (this.Buy.AllQuantity | this.Buy.Quantity >= sellQuantity)
				{
					// sell is fullfilled, buy is reduced
					transactionCost = this.calculateTransactionCost(sellQuantity);
					if (this.canBuy(week, transactionCost))
					{
                        this.executeTransaction(week, transactionCost, sellQuantity);                        
                        this.Buy.Quantity -= sellQuantity;

                        Offer.All.Remove(this.Sell);
                        this.ProcessOffer(week, this.Buy);
                    }
				}
				else
				{
					// buy is fullfilled, sell is reduced
					transactionCost = this.calculateTransactionCost(this.Buy.Quantity);
					if (this.canBuy(week, transactionCost))
					{
						this.executeTransaction(week, transactionCost, this.Buy.Quantity);
						this.Sell.Quantity -= this.Buy.Quantity;
						this.Buy.Quantity = 0;

                        Offer.All.Remove(this.Buy);
					}
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		#region IReporting Members

		public List<string> Report(Faction faction)
		{
			return this.Report(faction, false);
		}

		public List<string> Report(Faction faction, bool full)
		{
			List<string> lines = new List<string>();
			if (Offer.All[this].Count > 0)
			{
				lines.Add("Market report:");
				lines.Add("  Offers of selling items:");
				lines.AddRange(Offer.All[this][EOfferType.SellItems].Report(faction, full));
				lines.Add("  Offers of buying items:");
				lines.AddRange(Offer.All[this][EOfferType.BuyItems].Report(faction, full));
				lines.Add("  Offers of selling modules:");
				lines.AddRange(Offer.All[this][EOfferType.SellModules].Report(faction, full));
				lines.Add("  Offers of buying modules:");
				lines.AddRange(Offer.All[this][EOfferType.BuyModules].Report(faction, full));
				lines.Add("  Offers of selling technologies:");
				lines.AddRange(Offer.All[this][EOfferType.SellTechnologies].Report(faction, full));
				lines.Add("  Offers of buying technologies:");
				lines.AddRange(Offer.All[this][EOfferType.BuyTechnologies].Report(faction, full));
				lines.Add("");
			}
			return lines;
		}
		#endregion
	}
}
