using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class GetOrder : ImmediateOrder
	{
		public GetOrder (IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.get;
		}

		public GetOrder(IItemStacksHolder receiver, IItemStacksHolder transferer, ItemType itemType, int quantity)
			: base(receiver)
		{
			this.type = EOrderType.get;
			this.Transferer = transferer;
			this.ItemType = itemType;
			this.Quantity = quantity;
            this.GettingAllQuantity = false;
            this.GettingAllItemTypes = false;
        }

		public GetOrder(IItemStacksHolder receiver, IItemStacksHolder transferer, ItemType itemType)
			: base(receiver)
		{
			this.type = EOrderType.get;
			this.Transferer = transferer;
			this.ItemType = itemType;
			this.GettingAllQuantity = true;
            this.GettingAllItemTypes = false;
            this.GettingFromAll = false;
        }

		public GetOrder(IItemStacksHolder receiver, IItemStacksHolder transferer)
			: base(receiver)
		{
			this.type = EOrderType.get;
			this.Transferer = transferer;
			this.GettingAllItemTypes = true;
			this.GettingAllQuantity = true;
		    this.GettingFromAll = false;
		}

		public GetOrder(IItemStacksHolder receiver, ItemType itemType)
			: base(receiver)
		{
			this.type = EOrderType.get;
			this.GettingFromAll = true;
			this.ItemType = itemType;
			this.GettingAllQuantity = true;
		    this.GettingAllItemTypes = false;
		}

		public GetOrder(IItemStacksHolder receiver)
			: base(receiver)
		{
			this.type = EOrderType.get;
			this.GettingFromAll = true;
			this.GettingAllItemTypes = true;
			this.GettingAllQuantity = true;
		}

		public IItemStacksHolder Receiver 
		{
			get { return (IItemStacksHolder)this.Subject; }
		}

		public IItemStacksHolder    Transferer  { get; set; }
		public ItemType             ItemType    {get; set; }
		public int                  Quantity    { get; set; }
		
        public bool GettingAllQuantity  { get; set; }
		public bool GettingAllItemTypes { get; set; }
		public bool GettingFromAll      { get; set; }

		public override void Parse(string command)
		{
			//// get exact amount from defined modulestack
			//testcommands.Add("get 1 iron from 000006");
			//// get all except exact amount from defined modulestack
			//testcommands.Add("get -5 iron from 000006");
			//// get all iron from defined modulestack
			//testcommands.Add("get all iron from 000006");
			//// get all resources from defined modulestack
			//testcommands.Add("get all from 000006");
			//// get all resources from all modulestacks
			//testcommands.Add("get all iron");
			//// get all resources from all modulestacks
			//testcommands.Add("get all");

			string token;
			if (string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax");
			}

			token = LineParser.GetToken(ref command);
				
			if (token == "all")
			{
				this.GettingAllQuantity = true;
			}
			else
			{				
				try
				{
					this.Quantity = Convert.ToInt32(token);
				}
				catch (Exception ex)
				{
					throw new Exception("bad syntax an ALL or quantity expected", ex);
				}
			}
						
			token = LineParser.GetToken(ref command);
			if (token == "")
			{
				this.GettingAllItemTypes = true;
                this.GettingFromAll = true;
				// get xxx
			}
			else if (token == "from")
			{
                this.GettingAllItemTypes = true;
				token = LineParser.GetToken(ref command);
				if (ModuleStack.All.ContainsKey(token))
				{
					this.Transferer = ModuleStack.All[token];
				} else if (Person.All.ContainsKey(token))
				{
					this.Transferer = Person.All[token];
				} else
				{
					throw new Exception("bad syntax modulestack or person id expected");
				}
				// get xxx from 000000
			}
			else
			{
				try
				{
					this.ItemType = ItemType.All[token];
				}
				catch (Exception ex)
				{
					throw new Exception("bad syntax FROM or item type expected", ex);
				}

				token = LineParser.GetToken(ref command);
				if (token == "")
				{
                    this.GettingFromAll = true;
					// get xxx iron
				}
				else if (token == "from")
				{
					token = LineParser.GetToken(ref command);
                    this.assignTransferer(token);
					// get xxx iron from 000000
				}
				else
				{
					throw new Exception("bad syntax FROM expected");
				}
			}
		}

        private void assignTransferer(string token)
        {
            this.Transferer = ModuleStack.All.GetOrCreateNewModuleStack(this.Receiver.Owner, token);
            if (this.Transferer == null)
            {
                this.Transferer = Person.All[token];
            }
            if (this.Transferer == null)
            {
                throw new Exception("bad syntax modulestack or person id expected. Received: " + token);
            }
        }

		public override void Execute(int week)
		{
			this.Executed = false;
            if (this.GettingFromAll)
			{
				this.executeGetAll(week, this.Receiver.Location.ModuleStacks);
            }
            else if (this.GettingAllItemTypes) 
			{				
				this.executeGetAllItems(week, this.Transferer);
			} else 
			{
				this.executeGetQuantity(week, this.Transferer, this.ItemType);
			}			
			base.Execute(week);
		}

		private void executeGetAll(int week, ModuleStacks moduleStacks)
		{
			foreach (ModuleStack modulestack in moduleStacks.Values)
			{
				this.executeGetAll(week, modulestack.ModuleStacks);
				if (modulestack.Owner == this.Receiver.Owner)
				{
                    if (this.GettingAllItemTypes)
					{
						this.executeGetAllItems(week, modulestack);
					}
					else if (modulestack.ItemStacks.ContainsKey(this.ItemType))
					{						
						this.executeGetQuantity(week, modulestack, this.ItemType);
					}
				}
				foreach (Person person in modulestack.People.Values)
				{
					if (person.Owner == this.Receiver.Owner)
					{
                        if (this.GettingAllItemTypes)
						{
							this.executeGetAllItems(week, person);
						}
						else if (person.ItemStacks.ContainsKey(this.ItemType))
						{
							this.executeGetQuantity(week, person, this.ItemType);
						}
					}
				}
			}
		}

		private void executeGetAllItems(int week, IItemStacksHolder itemStacksHolder)
		{
			List<ItemType> itemTypes = new List<ItemType>();
			foreach (ItemType itemType in itemStacksHolder.ItemStacks.Keys)
			{
				itemTypes.Add(itemType);	
			}
			foreach (ItemType itemType in itemTypes)
			{
				this.executeGetQuantity(week, itemStacksHolder, itemType);
			}
		}

		private void executeGetQuantity(int week, IItemStacksHolder itemStacksHolder, ItemType itemType)
		{
			int available = 0;
			int transferrable = 0;
			if (itemStacksHolder.ItemStacks.ContainsKey(itemType))
			{
				available = itemStacksHolder.ItemStacks[itemType].Quantity;
				if (this.GettingAllQuantity | this.Quantity == 0)
				{
					transferrable = available;
				}
                else if (this.Quantity > 0 & this.Quantity <= available)
				{
					transferrable = this.Quantity;
				}
				else if (this.Quantity < 0 & available > -this.Quantity)
				{
					transferrable = available + this.Quantity;
				}
				else
				{
                    if (!this.FailedToExecute)
                    {
                        this.Receiver.EventReports.Add(
                            week,
                            string.Format("GET failed. {0} have less {1} then needed to complete.",
                                itemStacksHolder.ReportName,
                                itemType.ReportNameMultiple));
                        this.FailedToExecute = true;
                    }
				}
			}
			else
			{
                if (!this.FailedToExecute & this.Repeat >= 0)
				{
					this.Receiver.EventReports.Add(
						week,
						string.Format("GET failed. {0} does not have {1}.",
							itemStacksHolder.ReportName,
							itemType.ReportNameMultiple));
                    this.FailedToExecute = true;
				}
			}
			if (transferrable > 0)
			{
                ItemStack transfer = new ItemStack(itemType, transferrable);
                if ((this.Receiver.Location != itemStacksHolder.Location) & !this.FailedToExecute)
                {
                    this.Receiver.EventReports.Add(
                    week,
                    string.Format("GET failed, tried to get {0}, but receiver is not present in this location.",
                        transfer.ReportName));
                    this.FailedToExecute = true;
                }

                if ((this.Receiver.Capacity - this.Receiver.CapacityUsed < transfer.Size) & !this.FailedToExecute)
				{
                    this.Receiver.EventReports.Add(
                        week,
                        string.Format("GET failed, tried to get {0}, but there is not enough capacity: {1} required, {2} available.",
                            transfer.ReportName,
                            transfer.Size,
                            this.Receiver.Capacity - this.Receiver.CapacityUsed));
                    this.FailedToExecute = true;
				}

                if ((this.Receiver.Location == itemStacksHolder.Location)
                    & (this.Receiver.Capacity - this.Receiver.CapacityUsed >= transfer.Size))
				{
                    itemStacksHolder.ItemStacks.Remove(transfer);
					this.Receiver.ItemStacks.Add(transfer);
					itemStacksHolder.EventReports.Add(
						week,
						string.Format("given {0} to {1}.",
							transfer.ReportName,
							this.Receiver.ReportName));
					this.Receiver.EventReports.Add(
						week,
						string.Format("got {0} from {1}.",
							transfer.ReportName,
							itemStacksHolder.ReportName));
					this.Executed = true;
					base.Execute(week);
				}
			}
		}

        public override void LoadXml(XmlElement elOrder)
        {
            XmlElement elGet = (XmlElement)elOrder.SelectNodes("get")[0];
            this.assignTransferer(elGet.GetAttribute("transferer"));

            if (elGet.GetAttribute("item") == "all")
            {
                this.GettingAllItemTypes = true;
            }
            else 
            {
                this.ItemType = ItemType.All[elGet.GetAttribute("item")];
            }
            if (elGet.GetAttribute("quantity") == "all")
            {
                this.GettingAllQuantity = true;
            }
            else
            {
                this.Quantity = this.XMLAssignInteger(elGet.GetAttribute("quantity"), 0);
            }
        }

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
            XmlElement elGet = doc.CreateElement("get");
	
			elGet.SetAttribute("item", this.GettingAllItemTypes ? "all" : this.ItemType.Name);
            elGet.SetAttribute("quantity", this.GettingAllQuantity ? "all" : this.Quantity.ToString());
            elGet.SetAttribute("transferer", this.Transferer.Name);

            xmlElement.AppendChild(elGet);
			return xmlElement;
		}

        public override List<string> Report(Faction owner)
        {
            List<string> lines = new List<string>();
            string line;
            line = string.Format("{0}{1}get {2} {3} {4}",
                this.Conditions,
                (this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")),
                this.GettingAllQuantity ? "all" : this.Quantity.ToString(), 
                this.GettingAllItemTypes ? string.Empty : this.ItemType.Name,
                this.GettingFromAll ? string.Empty : string.Concat(this.Transferer.IsFormed ? "from " : "from new", this.Transferer.Name));
            lines.Add(line);
            return lines;
        }

	}
}
