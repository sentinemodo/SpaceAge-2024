using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class Offer : XMLProcessing, IReporting
	{
		public static readonly Offers All = new Offers();

        public BuyOrder BuyOrder        { get; set; }
        public SellOrder SellOrder      { get; set; }
        
		public bool AllQuantity         { get; set; }
        public bool Everywhere          { get; set; }
        public IOfferent Offerent       { get; set; }
        public Market Market            { get; set; }

        public EOfferType OfferType     { get; set; }
        public ItemType ItemType        { get; set; }
		public ModuleType ModuleType    { get; set; }
		public Technology Technology    { get; set; }

		public int Price { get; set; }

        private int quantity = 1;
        public int Quantity
        {
            get 
            {
                if (this.BuyOrder != null)
                {
                   return this.BuyOrder.Quantity; 
                }
                else if (this.SellOrder != null)
                {
                   return this.SellOrder.Quantity; 
                }
                else
                {
                    return this.quantity;
                }
            }
            set 
            {
                if (this.BuyOrder != null)
                {
                    this.BuyOrder.Quantity = value;
                }
                else if (this.SellOrder != null)
                {
                    this.SellOrder.Quantity = value;
                }
                else
                {
                    this.quantity = value;
                }
            }
        }

		public Offer(Market market, IOfferent offerent, EOfferType type)
		{
			this.Market = market;
			this.Offerent = offerent;
			this.OfferType = type;
            this.Everywhere = false;
			Offer.All.Add(this);
		}

		public bool Process(int week)
		{
			bool processed = this.Market.ProcessOffer(week, this);
			return processed;
		}

        public override void LoadXml(XmlElement elOffer)
        {
            if (elOffer.GetAttribute("price") == "any")
            {
                this.Price = 0;
            }
            else
            {
                this.Price = this.XMLAssignInteger(elOffer.GetAttribute("price"), 0);
            }

            if (elOffer.HasAttribute("quantity"))
            {
                if (elOffer.GetAttribute("quantity") == "all")
                {
                    this.AllQuantity = true;
                }
                else
                {
                    this.Quantity = this.XMLAssignInteger(elOffer.GetAttribute("quantity"), 1);
                }
            }

            if (this.OfferType == EOfferType.BuyItems | this.OfferType == EOfferType.SellItems)
            {
                this.ItemType = ItemType.All[elOffer.GetAttribute("item")];
            }

            if (this.OfferType == EOfferType.BuyModules | this.OfferType == EOfferType.SellModules)
            {
                this.ModuleType = ModuleType.All[elOffer.GetAttribute("module")];
            }

            if (this.OfferType == EOfferType.BuyTechnologies | this.OfferType == EOfferType.SellTechnologies)
            {
                this.Technology = Technology.All[elOffer.GetAttribute("technology")];
            }
        }

        public override XmlElement SaveXml(XmlDocument doc)
        {
            switch (this.OfferType)
            {
                case EOfferType.BuyItems:
                    this.xmlElement = doc.CreateElement("buying");
                    this.xmlElement.SetAttribute("item", this.ItemType.Name);
                    this.xmlElement.SetAttribute("quantity", (this.AllQuantity) ? "all" : this.Quantity.ToString());
                    break;
                case EOfferType.BuyModules:
                    this.xmlElement = doc.CreateElement("buying");
                    this.xmlElement.SetAttribute("module", this.ModuleType.Name);
                    this.xmlElement.SetAttribute("quantity", (this.AllQuantity) ? "all" : this.Quantity.ToString());
                    break;
                case EOfferType.BuyTechnologies:
                    this.xmlElement = doc.CreateElement("buying");
                    this.xmlElement.SetAttribute("technology", this.Technology.Name);
                    break;
                case EOfferType.SellItems:
                    this.xmlElement = doc.CreateElement("selling");
                    this.xmlElement.SetAttribute("item", this.ItemType.Name);
                    this.xmlElement.SetAttribute("quantity", (this.AllQuantity) ? "all" : this.Quantity.ToString());
                    break;
                case EOfferType.SellModules:
                    this.xmlElement = doc.CreateElement("selling");
                    this.xmlElement.SetAttribute("module", this.ModuleType.Name);
                    this.xmlElement.SetAttribute("quantity", (this.AllQuantity) ? "all" : this.Quantity.ToString());
                    break;
                case EOfferType.SellTechnologies:
                    this.xmlElement = doc.CreateElement("selling");
                    this.xmlElement.SetAttribute("technology", this.Technology.Name);
                    break;
                default:
                    throw new Exception("Unknown type of offer");
            }
            this.xmlElement.SetAttribute("price", (this.Price > 0) ? this.Price.ToString() : "any");

            return this.xmlElement;
        }

        public List<string> Report(Faction faction)
        {
            return this.Report(faction, false);
        }

        public List<string> Report(Faction faction, bool full)
        {
            List<string> lines = new List<string>();
            string line = string.Empty;
            switch (this.OfferType)
            {
                case EOfferType.BuyItems:
                    line = string.Format("    buy {0}{1} {2} by {3}{4}.",
                        (this.AllQuantity) ? "all " : (this.Quantity > 1) ? string.Concat(this.Quantity, " ") : string.Empty,
                        (this.Quantity > 1 | this.AllQuantity) ? this.ItemType.ReportNameMultiple : this.ItemType.ReportName,
                        (this.Price > 0)
                            ? string.Concat("at ", this.Price.ToString(), (this.Quantity > 1 | this.AllQuantity) ? " each" : string.Empty)
                            : "at any price",
                        this.Offerent.ReportName,
                        (full == true) ? string.Concat(" in ", this.Offerent.Location.ReportName) : string.Empty);
                    break;
                case EOfferType.BuyModules:
                    line = string.Format("    buy {0}{1} {2} by {3}{4}.",
                        (this.AllQuantity) ? "all " : (this.Quantity > 1) ? string.Concat(this.Quantity, " ") : string.Empty,
                        (this.Quantity > 1 | this.AllQuantity) ? this.ModuleType.ReportNameMultiple : this.ModuleType.ReportName,
                        (this.Price > 0)
                            ? string.Concat("at ", this.Price.ToString(), (this.Quantity > 1 | this.AllQuantity) ? " each" : string.Empty)
                            : "at any price",
                        this.Offerent.ReportName,
                        (full == true) ? string.Concat(" in ", this.Offerent.Location.ReportName) : string.Empty);
                    break;
                case EOfferType.BuyTechnologies:
                    line = string.Format("    buy {0} {1} by {2}{3}.",
                        this.Technology.ReportName,
                        (this.Price > 0) ? string.Concat("at ", this.Price.ToString()) : "at any price",
                        this.Offerent.ReportName,
                        (full == true) ? string.Concat(" in ", this.Offerent.Location.ReportName) : string.Empty);
                    break;
                case EOfferType.SellItems:
                    line = string.Format("    sell {0}{1} {2} from {3}{4}.",
                        (this.AllQuantity) ? "all " : (this.Quantity > 1) ? string.Concat(this.Quantity, " ") : string.Empty,
                        (this.Quantity > 1 | this.AllQuantity) ? this.ItemType.ReportNameMultiple : this.ItemType.ReportName,
                        string.Concat("at ", this.Price.ToString(), " each"),
                        this.Offerent.ReportName,
                        (full == true) ? string.Concat(" in ", this.Offerent.Location.ReportName) : string.Empty);
                    break;
                case EOfferType.SellModules:
                    line = string.Format("    sell {0}{1} {2} from {3}{4}.",
                        (this.AllQuantity) ? "all " : (this.Quantity > 1) ? string.Concat(this.Quantity, " ") : string.Empty,
                        (this.Quantity > 1 | this.AllQuantity) ? this.ModuleType.ReportNameMultiple : this.ModuleType.ReportName,
                        string.Concat("at ", this.Price.ToString(), " each"),
                        this.Offerent.ReportName,
                        (full == true) ? string.Concat(" in ", this.Offerent.Location.ReportName) : string.Empty);
                    break;
                case EOfferType.SellTechnologies:
                    line = string.Format("    sell {0} {1} from {2}{3}.",
                        this.Technology.ReportName,
                        string.Concat("at ", this.Price.ToString()),
                        this.Offerent.ReportName,
                        (full == true) ? string.Concat(" in ", this.Offerent.Location.ReportName) : string.Empty);
                    break;
                default:
                    throw new Exception("Unknown type of offer");
            }
            lines.Add(line);
            return lines;
        }
    }
}
