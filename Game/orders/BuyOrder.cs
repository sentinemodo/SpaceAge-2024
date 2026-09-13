using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class BuyOrder : ImmediateOrder
	{
		public BuyOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.buy;
            this.Price = -1;
            this.Quantity = 1;
            this.BuyType = EOfferType.BuyItems;
		}

		public Offer Buy { get; set; }

		public IOfferent Buyer
		{
			get { return (IOfferent)this.Subject; }
		}

		public EOfferType BuyType       { get; set; }		
		public Technology Technology    { get; set; }
		public ModuleType ModuleType    { get; set; }
		public ItemType ItemType        { get; set; }

		public int Price        { get; set; }
		public bool PriceRelative { get; set; }
		public int PriceOffset { get; set; }

        public int Quantity     { get; set; }
		public bool AllQuantity { get; set; } // default false
        public bool Everywhere  { get; set; } // default false
        		
		private void parseRelativeOffset(string token)
		{
			try
			{
				this.PriceOffset = Convert.ToInt32(token.Substring(1));
			}
			catch (Exception ex)
			{
				throw new Exception("bad syntax price expected", ex);
			}
		}

		private void parsePriceOffset(ref string token, ref string command)
		{
			if (token.StartsWith("+"))
			{
				this.parseRelativeOffset(token);
				token = LineParser.GetToken(ref command);
			}
			else if (token != string.Empty)
			{
				throw new Exception("bad syntax price expected");
			}
		}

		private string formatBuyPrice()
		{
			if (this.PriceRelative)
			{
				if (this.PriceOffset > 0)
				{
					return string.Concat(" at average +", this.PriceOffset.ToString());
				}
				return " at average";
			}
			if (this.Price > -1)
			{
				return string.Concat(" at ", this.Price.ToString());
			}
			return string.Empty;
		}

		public override void Parse(string command)
		{
			//// buy 25 terrans in batches of 5 per week at max price of 50 per terran (caution may not succeed)
			//testcommands.Add("5 buy 5 terran at 50 ");
			//// buy infinite amount of item/module for infinite amount of time at any price (caution can drain you cash like hell) 
            //// and at anyplace in reach (paying the transaction costs and having to wait for delivery)
			//testcommands.Add("@buy all terran everywhere");
			//// buy exact amount of modules at any price (caution can drain you cash like hell)
			//// note there is no syntax distinguish - potentialy bug source
			//testcommands.Add("buy 5 cdrill");
			//// buy specific technology
			//testcommands.Add("buy cdrill technology");

			string token, token2;
			if (string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax");
			}

			token = LineParser.GetToken(ref command);
			if (token != "all" & Technology.All.Contains(token))
			{
				this.BuyType = EOfferType.BuyTechnologies;
				this.Technology = Technology.All[token];
				this.Quantity = 1;
			} else 
			{
				token2 = LineParser.GetToken(ref command);
				if (ItemType.All.ContainsKey(token2))
				{
					this.ItemType = ItemType.All[token2];
                    this.BuyType = EOfferType.BuyItems;
				} else if (ModuleType.All.ContainsKey(token2))
				{
					this.ModuleType = ModuleType.All[token2];
                    this.BuyType = EOfferType.BuyModules;
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
            if (token != string.Empty & token != "at" & token != "everywhere")
			{
				throw new Exception("bad syntax AT or EVERYWHERE expected");
			} else if (token == "at")
			{
				token = LineParser.GetToken(ref command);
				if (token == "average")
				{
					this.PriceRelative = true;
					token = LineParser.GetToken(ref command);
					if (token != string.Empty)
					{
						this.parsePriceOffset(ref token, ref command);
					}
				}
				else if (token.StartsWith("+"))
				{
					this.PriceRelative = true;
					this.parseRelativeOffset(token);
				}
				else
				{
					try
					{
						this.Price = Convert.ToInt32(token);
					}
					catch (Exception ex)
					{
						throw new Exception("bad syntax price expected", ex);
					}
					if (this.Price <= 0)
					{
						throw new Exception("bad syntax price expected");
					}
				}
            } else if (token == "everywhere")
            {
                this.Everywhere = true;
            }

            token = LineParser.GetToken(ref command);
            if (token != string.Empty & token != "everywhere")
            {
                throw new Exception("bad syntax EVERYWHERE expected");
            }
            else if (token == "everywhere")
            {
                this.Everywhere = true;
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
            line = string.Format("{0}{1}buy ",
				this.Conditions,
				(this.Repeat > 1) ? string.Concat(this.Repeat.ToString(), " ") : ((this.Repeat < 0) ? "@" : string.Empty));
			switch (this.BuyType)
			{
				case EOfferType.BuyItems:
					line = string.Concat(line, string.Format("{0} {1}{2}{3}",
						(this.AllQuantity) ? "all" : this.Quantity.ToString(), 
						this.ItemType.Name,
						this.formatBuyPrice(),
                        (this.Everywhere) ? "everywhere" : string.Empty));
					break;
				case EOfferType.BuyModules:
					line = string.Concat(line, string.Format("{0} {1}{2}{3}",
						(this.AllQuantity) ? "all" : this.Quantity.ToString(), 
						this.ModuleType.Name,
						this.formatBuyPrice(),
                        (this.Everywhere) ? "everywhere" : ""));
					break;
				case EOfferType.BuyTechnologies:
					line = string.Concat(line, string.Format("{0} technology{1}",
						this.Technology.Name,
						(this.Price > -1) ? string.Concat(" at ", this.Price.ToString()) : string.Empty));
					break;
                default:
                    throw new InvalidOperationException("Unexpected offer type = " + this.BuyType);
			}
            lines.Add(line);
			return lines;
		}
        
        private void loadBuyPrice(XmlElement elBuy)
        {
            this.PriceRelative = false;
            this.PriceOffset = 0;
            this.Price = -1;
            if (elBuy.GetAttribute("price") == "average")
            {
                this.PriceRelative = true;
            }
            else if (elBuy.GetAttribute("price") == "any" || elBuy.GetAttribute("price") == string.Empty)
            {
                this.Price = -1;
            }
            else
            {
                this.Price = this.XMLAssignInteger(elBuy.GetAttribute("price"), 0);
            }
            if (elBuy.HasAttribute("price-offset"))
            {
                this.PriceRelative = true;
                this.PriceOffset = this.XMLAssignInteger(elBuy.GetAttribute("price-offset"), 0);
            }
        }

        private void saveBuyPrice(XmlElement elBuy)
        {
            if (this.PriceRelative)
            {
                elBuy.SetAttribute("price", "average");
                if (this.PriceOffset > 0)
                {
                    elBuy.SetAttribute("price-offset", this.PriceOffset.ToString());
                }
            }
            else if (this.Price > -1)
            {
                elBuy.SetAttribute("price", this.Price.ToString());
            }
            else
            {
                elBuy.SetAttribute("price", "-1");
            }
        }

        public override void LoadXml(XmlElement elOrder)
        {
            XmlElement elBuy = (XmlElement)elOrder.SelectNodes("buy")[0];
            this.loadBuyPrice(elBuy);
            this.Everywhere = this.XMLAssignBoolean(elBuy.GetAttribute("everywhere"), false);
            if (elBuy.GetAttribute("quantity") == "all")
            {
                this.AllQuantity = true;
            }
            else
            {
                this.Quantity = this.XMLAssignInteger(elBuy.GetAttribute("quantity"), 1);
            }                                       
            
            switch (elBuy.GetAttribute("buy-type"))
            {
                case "items":
                    this.BuyType= EOfferType.BuyItems;
                    this.ItemType = ItemType.All[elBuy.GetAttribute("item")];
                    break;
                case "modules":
                    this.BuyType = EOfferType.BuyModules;
                    this.ModuleType = ModuleType.All[elBuy.GetAttribute("module")];
                    break;
                case "technology":
                    this.BuyType = EOfferType.BuyTechnologies;
                    this.Technology = Technology.All[elBuy.GetAttribute("technology")];
                    break;
                default:
                    throw new Exception("Unknown type for Buy");

            }
        }

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
            XmlElement elBuy = doc.CreateElement("buy");

            this.saveBuyPrice(elBuy);
            elBuy.SetAttribute("everywhere", this.Everywhere.ToString());
            if (this.AllQuantity)
            {
                elBuy.SetAttribute("quantity", "all");
            }
            else
            {
                elBuy.SetAttribute("quantity", this.Quantity.ToString());
            }

            switch (this.BuyType)
			{
				case EOfferType.BuyItems:
                    elBuy.SetAttribute("buy-type", "items");
                    elBuy.SetAttribute("item", this.ItemType.Name);
					break;
				case EOfferType.BuyModules:
                    elBuy.SetAttribute("buy-type", "modules");
					elBuy.SetAttribute("module", this.ModuleType.Name);
					break;
				case EOfferType.BuyTechnologies:
                    elBuy.SetAttribute("buy-type", "technology");
                    elBuy.SetAttribute("technology", this.Technology.Name);
					break;
                default:
                    throw new InvalidOperationException("Unexpected offer type = " + this.BuyType);
			}

            xmlElement.AppendChild(elBuy);
            return xmlElement;
		}

		public override void Execute(int week)
		{
			this.Executed = false;

			if (this.Buy == null)
			{
				// create an offer, or keep a standing identical one
				this.Buy = new Offer(this.Buyer.Location.Market, this.Buyer, this.BuyType);
                this.Buy.BuyOrder = this; 
                this.Buy.Quantity = this.Quantity;
				this.Buy.AllQuantity = this.AllQuantity;
				this.Buy.Price = this.Price;
				this.Buy.PriceRelative = this.PriceRelative;
				this.Buy.PriceOffset = this.PriceOffset;
				this.Buy.Technology = this.Technology;
				this.Buy.ModuleType = this.ModuleType;
				this.Buy.ItemType = this.ItemType;
                this.Buy.Everywhere = this.Everywhere;
				this.Buy = Offer.All.ReuseEquivalent(this.Buy);
				this.Buy.BuyOrder = this;
            }

			this.Executed = false;
			base.Execute(week);
		}
	}
}
