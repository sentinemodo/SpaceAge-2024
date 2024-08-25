using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class GiveOrder : ImmediateOrder
	{
		public GiveOrder (IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.give;
		}

		public GiveOrder(IItemStacksHolder transferer, string receiverName, ItemType itemType, int quantity)
			: base(transferer)
		{
			this.type = EOrderType.give; 
			this.receiverName = receiverName;
			this.itemType = itemType;
			this.quantity = quantity;
		}

		public GiveOrder(IItemStacksHolder transferer, string receiverName, ItemType itemType)
			: base(transferer)
		{
			this.type = EOrderType.give;
			this.receiverName = receiverName;
			this.itemType = itemType;
			this.givingAllQuantity = true;
		}

		public GiveOrder(IItemStacksHolder transferer, string receiverName)
			: base(transferer)
		{
			this.type = EOrderType.give;
			this.receiverName = receiverName;
			this.givingAllItemTypes = true;
			this.givingAllQuantity = true;
		}

		public IItemStacksHolder Transferer
		{
			get { return (IItemStacksHolder)this.Subject; }
		}

		private string receiverName = null;
		public string ReceiverName
		{
			get { return this.receiverName; }
			set { this.receiverName = value; }
		}

		private IItemStacksHolder receiver = null;
		public IItemStacksHolder Receiver 
		{
			get { return this.receiver; }
			set { this.receiver = value; }
		}

		private ItemType itemType = null;
		public ItemType ItemType
		{
			get { return this.itemType; }
			set { this.itemType = value; }
		}

		private int quantity = 0;
		public int Quantity
		{
			get { return this.quantity; }
			set { this.quantity = value; }
		}

		private bool givingAllQuantity = false;
		public bool GivingAllQuantity
		{
			get { return this.givingAllQuantity; }
			set { this.givingAllQuantity = value; }
		}

		private bool givingAllItemTypes = false;
		public bool GivingAllItemTypes
		{
			get { return this.givingAllItemTypes; }
			set { this.givingAllItemTypes = value; }
		}
		
		public override void Parse(string command)
		{
			//// give exact amount to defined modulestack
			//testcommands.Add("give 1 iron to 000006");
			//// give all except exact amount to defined modulestack
			//testcommands.Add("give -5 iron to 000006");
			//// give all iron to defined modulestack
			//testcommands.Add("give all iron to 000006");
			//// give all resources to defined modulestack
			//testcommands.Add("give all to 000006");

			//// give something to newly defined modulestack
			//testcommands.Add("give xx xxxx to new1");

			string token;
			if (string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax");
			}

			token = LineParser.GetToken(ref command);
				
			if (token == "all")
			{
				this.givingAllQuantity = true;
			}
			else
			{				
				try
				{
					this.quantity = Convert.ToInt32(token);
				}
				catch (Exception ex)
				{
					throw new Exception("bad syntax an ALL or quantity expected", ex);
				}
			}
						
			token = LineParser.GetToken(ref command);
			if (token == "to")
			{
				this.givingAllItemTypes = true;
				this.receiverName = LineParser.GetToken(ref command);
				// give all|quantity to 000000
			}
			else 
			{
				try 
				{
					this.itemType = ItemType.All[token];
				} catch (Exception ex)
				{
					throw new Exception("bad syntax item type expected", ex);
				}
				token = LineParser.GetToken(ref command);
				if (token == "to")
				{
					this.receiverName = LineParser.GetToken(ref command);
				}
				else
				{
					throw new Exception("bad syntax TO expected");
				}
				// give all|quantity xxx to 000000
			}

            this.assignReceiver(this.receiverName);	
		}

        private void assignReceiver(string token)
        {
            this.Receiver = ModuleStack.All.GetOrCreateNewModuleStack(this.Transferer.Owner, token);
            if (this.Receiver == null)
            {
                this.Receiver = Person.All[token];
            }
            if (this.Receiver == null)
            {
                throw new Exception("bad syntax modulestack or person id expected. Received: " + token);
            }
        }

        public override void LoadXml(XmlElement elOrder)
        {
            XmlElement elGive = (XmlElement)elOrder.SelectNodes("give")[0];
            this.assignReceiver(elGive.GetAttribute("receiver"));

            if (elGive.GetAttribute("item") == "all")
            {
                this.GivingAllItemTypes = true;
            }
            else
            {
                this.ItemType = ItemType.All[elGive.GetAttribute("item")];
            }
            if (elGive.GetAttribute("quantity") == "all")
            {
                this.GivingAllQuantity = true;
            }
            else
            {
                this.Quantity = this.XMLAssignInteger(elGive.GetAttribute("quantity"), 0);
            }
        }

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{	
			XmlElement elGive = doc.CreateElement("give");

            elGive.SetAttribute("receiver", this.Receiver.Name);
            elGive.SetAttribute("item", (this.GivingAllItemTypes) ? "all" : this.itemType.Name);
            elGive.SetAttribute("quantity", (this.GivingAllQuantity) ? "all" : this.quantity.ToString());
			
            this.xmlElement.AppendChild(elGive);
			return xmlElement;
		}

        //// give exact amount to defined modulestack
        //testcommands.Add("give 1 iron to 000006");
        //// give all except exact amount to defined modulestack
        //testcommands.Add("give -5 iron to 000006");
        //// give all iron to defined modulestack
        //testcommands.Add("give all iron to 000006");
        //// give all resources to defined modulestack
        //testcommands.Add("give all to 000006");
        public override List<string> Report(Faction owner)
        {
            List<string> lines = new List<string>();
            string line;
            line = string.Format("{0}{1}give {2} {3} {4}",
                this.Conditions,
                (this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")),
                this.givingAllQuantity ? "all" : this.quantity.ToString(),
                this.givingAllItemTypes ? string.Empty : this.itemType.Name,
                this.receiver.IsFormed ? string.Concat("to ", this.receiver.Name) : string.Concat("to new", this.receiver.Name));
            lines.Add(line);
            return lines;
        }

		public override void Execute(int week)
		{
			this.Executed = false;

			if (this.givingAllItemTypes) 
			{				
				this.executeGiveAllItems(week);
			} else 
			{
				this.executeGiveQuantity(week, this.itemType);
			}
			
			base.Execute(week);
		}

		private void executeGiveAllItems(int week)
		{
			List<ItemType> itemTypes = new List<ItemType>();
			foreach (ItemType itemType in this.Transferer.ItemStacks.Keys)
			{
				itemTypes.Add(itemType);	
			}
			foreach (ItemType itemType in itemTypes)
			{
				this.executeGiveQuantity(week, itemType);
			}
		}

		private void executeGiveQuantity(int week, ItemType itemType)
		{
			int available = 0;
			int transferrable = 0;
			if (this.Transferer.ItemStacks.ContainsKey(itemType))
			{
				available = this.Transferer.ItemStacks[itemType].Quantity;
				if (this.givingAllQuantity | this.quantity == 0)
				{
					transferrable = available;
				}
				else if (this.quantity > 0)
				{
					transferrable = Math.Min(this.quantity, available);
				}
				else if (available > -this.quantity)
				{
					transferrable = available + this.quantity;
				}
				else
				{
					//if (this.Level == 0)
					//{
					//    this.Transferer.EventReports.Add(
					//        week,
					//        string.Format("GIVE failed. Less {0} available, then ordered to be left.",
					//            itemType.ReportNameMultiple));
					//}
				}
			}
			else
			{
				//if (this.Level == 0)
				//{
				//    this.Transferer.EventReports.Add(
				//       week,
				//       string.Format("GIVE failed. {0} not available.",
				//           itemType.ReportNameMultiple));
				//}
			}
			if (transferrable > 0)
			{
				ItemStack transfer = new ItemStack(itemType, transferrable);
				if (receiver.Capacity - receiver.CapacityUsed < transfer.Size)
				{
                    if (this.Level == 0 & this.receiver.IsFormed & !this.FailedToExecute)
                    {
                        this.Transferer.EventReports.Add(
                            week,
                            string.Format("GIVE failed, tried to give {0} to {1}, but there is not enough capacity: {2} required, {3} available.",
                                transfer.ReportName,
                                this.receiver.ReportName,
                                transfer.Size,
                                receiver.Capacity - receiver.CapacityUsed));
                        this.FailedToExecute = true;
                    }
				}
				else
				{
					// TODO: accepting gifts form other factions
					this.Transferer.ItemStacks.Remove(transfer);
					this.Receiver.ItemStacks.Add(transfer);
					this.Receiver.EventReports.Add(
						week, 
						string.Format("got {0} from {1}.",
							transfer.ReportName,
							this.Transferer.ReportName));
					this.Transferer.EventReports.Add(
						week, 
						string.Format("given {0} to {1}.",
							transfer.ReportName,
							this.receiver.ReportName));
					this.Executed = true;
				}
			}
		}
	}
}
