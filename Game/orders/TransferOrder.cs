using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class TransferOrder : ImmediateOrder
	{
		public TransferOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.transfer;
		}

		public TransferOrder(ModuleStack transferer, ModuleStack receiver, int index)
			: base(transferer)
		{
			this.type = EOrderType.transfer;
			this.Receiver = receiver;
			this.Index = index;
            this.Quantity = 1;
            this.TransferTime = 0;
            this.ModuleType = transferer.ModuleType;
		}

        public TransferOrder(ModuleStack transferer, ModuleStack receiver, ModuleType type, int quantity, int transferTime)
            : base(transferer)
        {
            this.type = EOrderType.transfer;
            this.Receiver = receiver;
            this.Index = 0;
            this.Quantity = quantity;
            this.TransferTime = transferTime;
            this.ModuleType = type;
        }

		public ModuleStack Transferer
		{
			get { return (ModuleStack)this.Subject; }
		}

		public int Index            { get; set; }      
        public int Quantity         { get; set; }
        public int TransferTime     { get; set; }

        public ModuleType ModuleType    { get; set; }
		public ModuleStack Receiver     { get; set; }

		public override void Parse(string command)
		{
            // TODO: implement remainign transfer syntax
			// transfer first module to m00001
            // TRANSFER MODULE TO m00001
            // transfer a number of existing modules into receiver modulestack
            // TRANSFER 2 MODULES TO m00001
            // transfer all modules into receiver modulestack effectively clearing the stack (abandoned items get dropped to the region)
            // TRANSFER ALL MODULES TO m00001
            // transfer only damaged modules (in case all are damaged, abandoned items get dropped to the region)
            // TRANSFER ALL DAMAGED MODULES to m00001
            // transfer the specific module 2 in the stack (might get important, if you want to handpick a damaged module)
            // TRANSFER MODULE 2 to m00001

			string token;
			if (string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax");
			}

			try
			{
				token = LineParser.GetToken(ref command);
				this.Quantity = Convert.ToInt32(token);
				if (this.Quantity < 1)
				{
					throw new Exception("Bad syntax, positive amount expected");
				}
			}
			catch (Exception ex)
			{
				throw new Exception("bad syntax number of modules expected", ex);
			}

			token = LineParser.GetToken(ref command);
			if (token != "to")
			{
				throw new Exception("bad syntax TO expected");
			}

			try
			{
				token = LineParser.GetToken(ref command);
				this.Receiver = ModuleStack.All[token];
			}
			catch (Exception ex)
			{
				throw new Exception("bad syntax or receiver does not exist", ex);
			}

			this.ModuleType = this.Transferer.ModuleType;
		}

        public override void LoadXml(XmlElement elOrder)
        {
            XmlElement elTransfer = (XmlElement)elOrder.SelectNodes("transfer")[0];
            if (elTransfer.HasAttribute("quantity"))
            {
                this.Quantity = this.XMLAssignInteger(elTransfer.GetAttribute("quantity"), 0);
            }
            if (elTransfer.HasAttribute("index"))
            {
                this.Index = this.XMLAssignInteger(elTransfer.GetAttribute("index"), 0);
            }
            this.Receiver = ModuleStack.All[elTransfer.GetAttribute("receiver")];
            this.ModuleType = this.Transferer.ModuleType;
        }

        public override XmlElement SaveXml_core(XmlDocument doc, string subject)
        {
            XmlElement elTransfer = doc.CreateElement("transfer");
            if (this.Quantity != 0)
            {
                elTransfer.SetAttribute("quantity", this.Quantity.ToString());
            }
            if (this.Index != 0)
            {
                elTransfer.SetAttribute("index", this.Index.ToString());
            }
            
            elTransfer.SetAttribute("receiver", this.Receiver.Name);

            xmlElement.AppendChild(elTransfer);
            return xmlElement;
        }
		
		public override void Execute(int week)
		{
			this.Executed = false;
            ModuleStack sourceStack = null;

            if (this.ModuleType == this.Transferer.ModuleType)
            {
                sourceStack = this.Transferer;
            }
			else 
			{
                foreach (ModuleStack stack in this.Transferer.ModuleStacks.Values)
                {
                    if (stack.ModuleType == this.ModuleType)
                    {
                        sourceStack = stack;
                        break;
                    }
                }
            }

            if (sourceStack != null)
            {
				if (this.ModuleType != null && this.ModuleType.IsHangarCraft
					&& !this.canTransferHangarCraft(this.Receiver))
				{
					this.Transferer.EventReports.Add(
						week,
						"TRANSFER failed. fighter drones can only nest in a location or a fighter drone bay.");
					base.Execute(week);
					return;
				}
				if (sourceStack.Quantity >= this.Quantity)
				{             
                    // packaging
                    ModuleStack transferringStack = new ModuleStack(this.Transferer, this.Transferer.Owner, this.ModuleType);
                    Module module; 
                    for (int i = 0; i < this.Quantity; i++)
                    {
                        module = sourceStack.Modules[0];
                        sourceStack.RemoveModule(0);
                        transferringStack.AddModule(module);
                    }
                    
                    // keeping in mind status of long order execution to avoid transfer/new order abuse
                    if (this.Transferer.ExecutedLongOrder)
                    {
                        transferringStack.ExecutedLongOrder = true;
                    }

					this.Transferer.EventReports.Add(
						week,
						string.Format("transferred {0} to {1}.",						
                            (transferringStack.Quantity > 1) ? string.Format("{0} {1}",
                                transferringStack.Quantity,
                                transferringStack.ModuleType.ReportNameMultiple) : 
                                transferringStack.ModuleType.ReportName,
                            this.Receiver.ReportName));

                    // create receive effect
                    ReceivingModules effect = new ReceivingModules(this.Receiver, this.Transferer, transferringStack, this.TransferTime);
                    effect.Execute(week);

                    // if it is instantenously executed, remove it
                    if (effect.Executed)
                    {
                        this.Receiver.Effects.Remove(effect);
                    }

					if (sourceStack.Quantity == 0)
					{
                        sourceStack.ModuleType = null;
                        ModuleStack.All.Remove(sourceStack);
					}                   

					this.Executed = true;
					ModuleStack contractGiver = this.Transferer;
					if (sourceStack.Quantity == 0)
					{
						ModuleStack parent = this.Transferer.Parent as ModuleStack;
						if (parent != null && parent.Owner == this.Transferer.Owner)
						{
							contractGiver = parent;
						}
					}
					Contract.All.NotifyTransfer(this.Transferer.Owner, contractGiver, this.Receiver, this.ModuleType, this.Quantity);
				}
				else
				{
					this.Transferer.EventReports.Add(
						week, 
						"TRANSFER failed. tried to transfer more modules than having.");								
				}
			}
			base.Execute(week);
        }

		private bool canTransferHangarCraft(ModuleStack receiver)
		{
			if (receiver == null || receiver.ModuleType == null)
			{
				return false;
			}
			if (receiver.ModuleType == this.ModuleType)
			{
				return ModuleStack.CanNestHangarCraft(receiver.Parent, this.ModuleType);
			}
			return ModuleStack.CanNestHangarCraft(receiver, this.ModuleType);
		}
	}
}
