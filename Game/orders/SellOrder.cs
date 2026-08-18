using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class SellOrder : ImmediateOrder
	{
		public SellOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.sell;
            this.Price = -1;
            this.Quantity = 1;
            this.SellType = EOfferType.SellItems;
		}

		public Offer Sell { get; set; }

		public IOfferent Seller
		{
			get { return (IOfferent)this.Subject; }
		}

		public EOfferType SellType      { get; set; }		
		public Technology Technology    { get; set; }
		public ModuleType ModuleType    { get; set; }
		public ItemType ItemType        { get; set; }

		public int Price        { get; set; }

        public int Quantity     { get; set; }
		public bool AllQuantity { get; set; } // default false
        public bool Everywhere  { get; set; } // default false
        		
		public override void Parse(string command)
		{
			//// sell 25 terrans in batches of 5 per week at max price of 50 per terran (caution may not succeed)
			//testcommands.Add("5 sell 5 terran at 50 ");
			//// sell infinite amount of item/module for infinite amount of time at average selling price (caution can sell all the stuff at bargain prices) 
			//testcommands.Add("@sell all terran at average");
            //// sell exact amount of modules at average price (caution can sell the stuff at bargain prices)
			//// note there is no syntax distinguish - potentialy bug source
			//testcommands.Add("sell 5 cdrill at average");
			//// sell copy technology
			//testcommands.Add("sell cdrill technology at 500");

			string token, token2;
			if (string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax");
			}

			token = LineParser.GetToken(ref command);
			if (token != "all" & Technology.All.Contains(token))
			{
				this.SellType = EOfferType.SellTechnologies;
				this.Technology = Technology.All[token];
				this.Quantity = 1;
			} else 
			{
				token2 = LineParser.GetToken(ref command);
				if (ItemType.All.ContainsKey(token2))
				{
					this.ItemType = ItemType.All[token2];
                    this.SellType = EOfferType.SellItems;
				} else if (ModuleType.All.ContainsKey(token2))
				{
					this.ModuleType = ModuleType.All[token2];
                    this.SellType = EOfferType.SellModules;
				} else 
				{
					throw new Exception("bad syntax item or module expected");
				}

				if (token == "all")
				{
					this.AllQuantity = true;
				}
				else 
				{
					try
					{
						this.Quantity = Convert.ToInt32(token);
					}
					catch (Exception ex)
					{
						throw new Exception("bad syntax technology or ALL or quantity expected", ex);
					}
				}
			}

			token = LineParser.GetToken(ref command);
            if (token != string.Empty & token != "at")
			{
				throw new Exception("bad syntax AT expected");
			} else if (token == "at")
			{
				token = LineParser.GetToken(ref command);
                if (token == "average")
                {
                    if (this.SellType == EOfferType.SellItems)
                    {
                        this.Price = (int)this.Seller.Location.Market.GetPrice(this.ItemType);
                    }
                    else if (this.SellType == EOfferType.SellModules)
                    {
                        this.Price = (int)this.Seller.Location.Market.GetPrice(this.ModuleType);
                    }
                    else
                    {
                        this.Price = (int)this.Seller.Location.Market.GetPrice(this.Technology);
                    }
                }
                else
                {
                    try
                    {
                        this.Price = Convert.ToInt32(token);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("bad syntax AVERAGE or price expected", ex);
                    }
                }
				
            } 


			token = LineParser.GetToken(ref command);
			if (token != string.Empty)
			{
				throw new Exception("bad syntax");
			}
		}

        public override List<string> Report(Faction owner)
        {
            List<string> lines = new List<string>();
            string line;
            line = string.Format("{0}{1}sell ",
				this.Conditions,
				(this.Repeat > 1) ? string.Concat(this.Repeat.ToString(), " ") : ((this.Repeat < 0) ? "@" : string.Empty));
			switch (this.SellType)
			{
				case EOfferType.SellItems:
					line = string.Concat(line, string.Format("{0} {1}{2}",
						(this.AllQuantity) ? "all" : this.Quantity.ToString(), 
						this.ItemType.Name,
						(this.Price > -1) ? string.Concat(" at ", this.Price.ToString()) : string.Empty));
					break;
				case EOfferType.SellModules:
					line = string.Concat(line, string.Format("{0} {1}{2}",
						(this.AllQuantity) ? "all" : this.Quantity.ToString(), 
						this.ModuleType.Name,
						(this.Price > -1) ? string.Concat(" at ", this.Price.ToString()) : string.Empty));
					break;
				case EOfferType.SellTechnologies:
					line = string.Concat(line, string.Format("{0} technology{1}",
						this.Technology.Name,
						(this.Price > -1) ? string.Concat(" at ", this.Price.ToString()) : string.Empty));
					break;
                default:
                    throw new InvalidOperationException("Unexpected offer type = " + this.SellType);
			}
            lines.Add(line);
			return lines;
		}

        public override void LoadXml(XmlElement elOrder)
        {
            XmlElement elSell = (XmlElement)elOrder.SelectNodes("sell")[0];
            this.Price = this.XMLAssignInteger(elSell.GetAttribute("price"), 0);
            if (elSell.GetAttribute("quantity") == "all")
            {
                this.AllQuantity = true;
            }
            else
            {
                this.Quantity = this.XMLAssignInteger(elSell.GetAttribute("quantity"), 1);
            }

            switch (elSell.GetAttribute("sell-type"))
            {
                case "items":
                    this.SellType = EOfferType.SellItems;
                    this.ItemType = ItemType.All[elSell.GetAttribute("item")];
                    break;
                case "modules":
                    this.SellType = EOfferType.SellModules;
                    this.ModuleType = ModuleType.All[elSell.GetAttribute("module")];
                    break;
                case "technology":
                    this.SellType = EOfferType.SellTechnologies;
                    this.Technology = Technology.All[elSell.GetAttribute("technology")];
                    break;
                default:
                    throw new Exception("Unknown type for Sell");

            }
        }

        public override XmlElement SaveXml_core(XmlDocument doc, string subject)
        {
            XmlElement elSell = doc.CreateElement("sell");

            elSell.SetAttribute("price", this.Price.ToString());

            if (this.AllQuantity)
            {
                elSell.SetAttribute("quantity", "all");
            }
            else
            {
                elSell.SetAttribute("quantity", this.Quantity.ToString());
            }

            switch (this.SellType)
            {
                case EOfferType.SellItems:
                    elSell.SetAttribute("sell-type", "items");
                    elSell.SetAttribute("item", this.ItemType.Name);
                    break;
                case EOfferType.SellModules:
                    elSell.SetAttribute("sell-type", "modules");
                    elSell.SetAttribute("module", this.ModuleType.Name);
                    break;
                case EOfferType.SellTechnologies:
                    elSell.SetAttribute("sell-type", "technology");
                    elSell.SetAttribute("technology", this.Technology.Name);
                    break;
                default:
                    throw new InvalidOperationException("Unexpected offer type = " + this.SellType);
            }

            xmlElement.AppendChild(elSell);
            return xmlElement;
        }


		public override void Execute(int week)
		{
			this.Executed = false;

            if (this.Sell == null)
			{
				// create an offer
				this.Sell = new Offer(this.Seller.Location.Market, this.Seller, this.SellType);
                this.Sell.SellOrder = this; 
                this.Sell.Quantity = this.Quantity;
				this.Sell.AllQuantity = this.AllQuantity;
				this.Sell.Price = this.Price;
				this.Sell.Technology = this.Technology;
				this.Sell.ModuleType = this.ModuleType;
				this.Sell.ItemType = this.ItemType;
            }

			// List the offer and leave it standing. Matching is driven from the buy side
			// (BuyOrder.Execute / Game.ProcessBuyOffers). Processing a sell as a buy
			// throws when a standing buy already exists (SampleGame food sells).
			this.Executed = false;
			base.Execute(week);
		}
	}
}
