using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class Offers : List<Offer>, IReporting
	{
		public double AveragePrice(ItemType itemType)
		{
			return 0;
		}

		public double AveragePrice(ModuleType moduleType)
		{
			return 0;
		}

		public double AveragePrice(Technology technology)
		{
			return 0;
		}

		public Offer LowestPrice()
		{
			return this.LowestPrice(int.MaxValue);
		}

		public Offer LowestPrice(int bidPrice)
		{
			Offer bestOffer = null;
			foreach (Offer offer in this)
			{
				if (offer.Price <= bidPrice &&
						(bestOffer == null || bestOffer.Price > offer.Price ||
						(bestOffer.Price == offer.Price && bestOffer.Quantity < offer.Quantity)))
				{
					bestOffer = offer;
				}
			}
			return bestOffer;
		}

		public Offer HighestPrice()
		{
			return this.HighestPrice(int.MinValue);
		}

		public Offer HighestPrice(int bidPrice)
		{
			Offer bestOffer = null;
			foreach (Offer offer in this)
			{
				if (offer.Price >= bidPrice &&
						(bestOffer == null || bestOffer.Price < offer.Price ||
						(bestOffer.Price == offer.Price && bestOffer.Quantity < offer.Quantity)))
				{
					bestOffer = offer;
				}
			}
			return bestOffer;
		}

		public Offers this[Market market]
		{
			get
			{
				Offers offers = new Offers();
				foreach (Offer offer in this)
				{
					if (offer.Market == market)
					{
						offers.Add(offer);
					}
				}
				return offers;
			}
		}

		public Offers this[ModuleStack moduleStack]
		{
			get
			{
				Offers offers = new Offers();
				foreach (Offer offer in this)
				{
					if (offer.Offerent == moduleStack)
					{
						offers.Add(offer);
					}
				}
				return offers;
			}
		}

		public Offers this[IHolder location]
		{
			get
			{
				Offers offers = new Offers();
				foreach (Offer offer in this)
				{
					if (offer.Offerent.Location == location)
					{
						offers.Add(offer);
					}
				}
				return offers;
			}
		}

		public Offers this[ItemType itemType]
		{
			get
			{
				Offers offers = new Offers();
				foreach (Offer offer in this)
				{
					if (offer.OfferType == EOfferType.BuyItems || offer.OfferType == EOfferType.SellItems) 						
					{
						if (offer.ItemType == itemType)
						{
							offers.Add(offer);
						}
					}
				}
				return offers;
			}
		}

		public Offers this[ModuleType moduleType]
		{
			get
			{
				Offers offers = new Offers();
				foreach (Offer offer in this)
				{
					if (offer.OfferType == EOfferType.BuyModules || offer.OfferType == EOfferType.SellModules)
					{
						if (offer.ModuleType == moduleType)
						{
							offers.Add(offer);
						}
					}
				}
				return offers;
			}
		}

		public Offers this[Technology technology]
		{
			get
			{
				Offers offers = new Offers();
				foreach (Offer offer in this)
				{
					if (offer.OfferType == EOfferType.BuyTechnologies || offer.OfferType == EOfferType.SellTechnologies)
					{
						if (offer.Technology == technology)
						{
							offers.Add(offer);
						}
					}
				}
				return offers;
			}
		}

		public Offers this[EOfferType offerType]
		{
			get
			{
				Offers offers = new Offers();
				foreach (Offer offer in this)
				{
					if (offer.OfferType == offerType)
					{
						offers.Add(offer);
					}
				}
				return offers;
			}
		}

		public Offers this[EOffersType offersType]
		{
			get
			{
				Offers offers = new Offers();
				foreach (Offer offer in this)
				{
					if (offersType == EOffersType.Buy)
					{
						if (offer.OfferType == EOfferType.BuyItems ||
							offer.OfferType == EOfferType.BuyModules ||
							offer.OfferType == EOfferType.BuyTechnologies)
						{
							offers.Add(offer);
						}
					}
					else
					{
						if (offer.OfferType == EOfferType.SellItems ||
							offer.OfferType == EOfferType.SellModules ||
							offer.OfferType == EOfferType.SellTechnologies)
						{
							offers.Add(offer);
						}
					}
				}
				return offers;
			}
		}

        public Offers this[Faction offerent, bool exclude = false]
        {
            get
            {
                Offers offers = new Offers();
                foreach (Offer offer in this)
                {
                    if (exclude)
                    {
                        if (offer.Offerent.Owner != offerent)
                        {
                            offers.Add(offer);
                        }
                    } 
                    else
                    {
                        if (offer.Offerent.Owner == offerent)
                        {
                            offers.Add(offer);
                        }
                    }
                }
                return offers;
            }
        }


        public static int CompareByNames(Offer offer1, Offer offer2)
        {
            string offerent1 = string.Concat(offer1.Offerent.Owner.Name, offer1.Offerent.Name);
            string offerent2 = string.Concat(offer2.Offerent.Owner.Name, offer2.Offerent.Name);
            return String.Compare(offerent1, offerent2);
        } 

        public Offer GetIndex(int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException();
            }

            int i = 0;
            foreach (Offer offer in this)
            {
                if (i == index)
                {
                    return offer;
                }
                i++;
            }

            // not found within index range
            throw new IndexOutOfRangeException();
        }

        public new int Count
        {
            get
            {
                int i = 0;
                foreach (Offer offer in this)
                {
                    i++;
                }
                return i;
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

            this.Sort(Offers.CompareByNames);
			foreach (Offer offer in this)
			{
				lines.AddRange(offer.Report(faction, full));
			}
			if (lines.Count == 0)
			{
				lines.Add("    none.");
			}
			return lines;
		}

		#endregion

        new public bool Remove(Offer offer)
        {
            if (offer.BuyOrder != null)
            {
                offer.BuyOrder.Executed = true;
            }
            if (offer.SellOrder != null)
            {
                offer.SellOrder.Executed = true;
            }
            return base.Remove(offer);
        }

		public Offer FindEquivalent(Offer candidate)
		{
			foreach (Offer offer in this)
			{
				if (!object.ReferenceEquals(offer, candidate) && offer.SameMarketPosition(candidate))
				{
					return offer;
				}
			}
			return null;
		}

		public Offer ReuseEquivalent(Offer offer)
		{
			Offer existing = this.FindEquivalent(offer);
			if (existing == null)
			{
				return offer;
			}

			offer.BuyOrder = null;
			offer.SellOrder = null;
			base.Remove(offer);
			return existing;
		}

        public void LoadXml(XmlElement elHolder, Market market, IOfferent offerent)
        {
            Offer offer;
            EOfferType offerType;

            foreach (XmlElement elOffer in elHolder.SelectNodes("buying"))
            {
                if (elOffer.HasAttribute("item"))
                {
                    offerType = EOfferType.BuyItems;
                }
                else if (elOffer.HasAttribute("module"))
                {
                    offerType = EOfferType.BuyModules;
                }
                else
                {
                    offerType = EOfferType.BuyTechnologies;
                }
                offer = new Offer(market, offerent, offerType);
                offer.LoadXml(elOffer);
				Offer.All.ReuseEquivalent(offer);
            }

            foreach (XmlElement elOffer in elHolder.SelectNodes("selling"))
            {
                if (elOffer.HasAttribute("item"))
                {
                    offerType = EOfferType.SellItems;
                }
                else if (elOffer.HasAttribute("module"))
                {
                    offerType = EOfferType.SellModules;
                }
                else
                {
                    offerType = EOfferType.SellTechnologies;
                }
                offer = new Offer(market, offerent, offerType);
                offer.LoadXml(elOffer);
				Offer.All.ReuseEquivalent(offer);
            }
        }
        
        public XmlElement SaveXml(XmlDocument doc, XmlElement elHolder)
        {
            XmlElement elOffer;

            foreach (Offer offer in this)
            {                
                elOffer = offer.SaveXml(doc);
                elHolder.AppendChild(elOffer);
            }

            return elHolder;
        }
	}
}
