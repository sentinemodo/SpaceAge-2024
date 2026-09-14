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
		public bool TransferAll     { get; set; }
		public bool DamagedOnly     { get; set; }

        public ModuleType ModuleType    { get; set; }
		public ModuleStack Receiver     { get; set; }
		public Faction ReceiverFaction  { get; set; }

		public override void Parse(string command)
		{
			string token;
			if (string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax");
			}

			token = LineParser.GetToken(ref command);
			if (token == "all")
			{
				this.TransferAll = true;
			}
			else if (token == "module")
			{
				try
				{
					token = LineParser.GetToken(ref command);
					this.Index = Convert.ToInt32(token);
					if (this.Index < 1)
					{
						throw new Exception("Bad syntax, positive module index expected");
					}
				}
				catch (Exception ex)
				{
					throw new Exception("bad syntax module index expected", ex);
				}
				this.Quantity = 1;
			}
			else
			{
				try
				{
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
			}

			while (true)
			{
				token = LineParser.GetToken(ref command);
				if (token == "damaged")
				{
					this.DamagedOnly = true;
				}
				else if (token == "modules" || token == "module")
				{
				}
				else if (token == "to")
				{
					break;
				}
				else
				{
					throw new Exception("bad syntax TO expected");
				}
			}

			this.parseReceiverToken(ref command);
			this.ModuleType = this.Transferer.ModuleType;
		}

		private void parseReceiverToken(ref string command)
		{
			string token;

			token = LineParser.GetToken(ref command);
			if (token == "faction")
			{
				try
				{
					token = LineParser.GetToken(ref command);
					this.ReceiverFaction = Faction.All[token];
				}
				catch (Exception ex)
				{
					throw new Exception("bad syntax or faction does not exist", ex);
				}
				return;
			}

			try
			{
				this.Receiver = ModuleStack.All[token];
			}
			catch (Exception ex)
			{
				throw new Exception("bad syntax or receiver does not exist", ex);
			}
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
			if (elTransfer.HasAttribute("transfer-all"))
			{
				this.TransferAll = elTransfer.GetAttribute("transfer-all") == "true";
			}
			if (elTransfer.HasAttribute("damaged-only"))
			{
				this.DamagedOnly = elTransfer.GetAttribute("damaged-only") == "true";
			}
			if (elTransfer.HasAttribute("receiver-faction"))
			{
				this.ReceiverFaction = Faction.All[elTransfer.GetAttribute("receiver-faction")];
			}
			else
			{
				this.Receiver = ModuleStack.All[elTransfer.GetAttribute("receiver")];
			}
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
			if (this.TransferAll)
			{
				elTransfer.SetAttribute("transfer-all", "true");
			}
			if (this.DamagedOnly)
			{
				elTransfer.SetAttribute("damaged-only", "true");
			}
			if (this.ReceiverFaction != null)
			{
				elTransfer.SetAttribute("receiver-faction", this.ReceiverFaction.Name);
			}
			else if (this.Receiver != null)
			{
				elTransfer.SetAttribute("receiver", this.Receiver.Name);
			}
            
            xmlElement.AppendChild(elTransfer);
            return xmlElement;
        }
		
		public override void Execute(int week)
		{
			this.Executed = false;
			if (this.ModuleType == null)
			{
				this.ModuleType = this.Transferer.ModuleType;
			}
            ModuleStack sourceStack = this.findSourceStack();

            if (sourceStack == null)
			{
				base.Execute(week);
				return;
			}

			if (this.ModuleType != null && this.ModuleType.IsHangarCraft
				&& this.Receiver != null
				&& !this.canTransferHangarCraft(this.Receiver))
			{
				this.Transferer.EventReports.Add(
					week,
					"TRANSFER failed. fighter drones can only nest in a location or a fighter drone bay.");
				base.Execute(week);
				return;
			}

			List<Module> picked = this.collectModules(sourceStack);
			if (picked.Count == 0)
			{
				this.Transferer.EventReports.Add(
					week,
					"TRANSFER failed. tried to transfer more modules than having.");
				base.Execute(week);
				return;
			}

			if (this.ReceiverFaction != null)
			{
				this.executeToFaction(week, sourceStack, picked);
			}
			else
			{
				this.executeToStack(week, sourceStack, picked);
			}

			base.Execute(week);
        }

		private ModuleStack findSourceStack()
		{
			if (this.ModuleType == this.Transferer.ModuleType)
			{
				return this.Transferer;
			}

			foreach (ModuleStack stack in this.Transferer.ModuleStacks.Values)
			{
				if (stack.ModuleType == this.ModuleType)
				{
					return stack;
				}
			}
			return null;
		}

		private List<Module> collectModules(ModuleStack sourceStack)
		{
			List<Module> picked = new List<Module>();
			if (this.Index > 0)
			{
				if (this.Index > sourceStack.Modules.Count)
				{
					return picked;
				}
				picked.Add(sourceStack.Modules[this.Index - 1]);
				return picked;
			}

			if (this.TransferAll && this.DamagedOnly)
			{
				for (int i = sourceStack.Modules.Count - 1; i >= 0; i--)
				{
					if (sourceStack.Modules[i].DamageStatus != EDamageStatus.undamaged)
					{
						picked.Add(sourceStack.Modules[i]);
					}
				}
				return picked;
			}

			int quantity = this.TransferAll ? sourceStack.Quantity : this.Quantity;
			if (quantity > sourceStack.Quantity)
			{
				return picked;
			}

			for (int i = 0; i < quantity; i++)
			{
				picked.Add(sourceStack.Modules[0]);
			}
			return picked;
		}

		private void executeToStack(int week, ModuleStack sourceStack, List<Module> picked)
		{
			ModuleStack transferringStack = new ModuleStack(this.Transferer, this.Transferer.Owner, this.ModuleType);
			this.moveModules(sourceStack, transferringStack, picked);

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

			ReceivingModules effect = new ReceivingModules(this.Receiver, this.Transferer, transferringStack, this.TransferTime);
			effect.Execute(week);

			if (effect.Executed)
			{
				this.Receiver.Effects.Remove(effect);
			}

			this.cleanupSource(sourceStack);
			this.Executed = true;
			this.notifyStackContracts(sourceStack, picked.Count);
		}

		private void executeToFaction(int week, ModuleStack sourceStack, List<Module> picked)
		{
			ModuleStack transferringStack = new ModuleStack(this.Transferer, this.Transferer.Owner, this.ModuleType);
			this.moveModules(sourceStack, transferringStack, picked);

			if (this.Transferer.ExecutedLongOrder)
			{
				transferringStack.ExecutedLongOrder = true;
			}

			transferringStack.SetOwnerRecursive(this.ReceiverFaction);
			Location location = this.Transferer.Location;
			transferringStack.Parent = location;

			this.Transferer.EventReports.Add(
				week,
				string.Format("transferred {0} to {1}.",
					(transferringStack.Quantity > 1) ? string.Format("{0} {1}",
						transferringStack.Quantity,
						transferringStack.ModuleType.ReportNameMultiple) :
						transferringStack.ModuleType.ReportName,
					this.ReceiverFaction.ReportName));

			this.cleanupSource(sourceStack);
			this.Executed = true;

			Region contractLocation = location as Region;
			if (contractLocation != null)
			{
				Contract.All.NotifyFactionTransfer(
					this.Transferer.Owner,
					transferringStack,
					this.ReceiverFaction,
					this.ModuleType,
					picked.Count,
					contractLocation);
			}
		}

		private void moveModules(ModuleStack sourceStack, ModuleStack destination, List<Module> picked)
		{
			if (this.Index > 0)
			{
				destination.AddModule(picked[0]);
				sourceStack.RemoveModule(this.Index - 1);
				return;
			}

			if (this.TransferAll && this.DamagedOnly)
			{
				for (int i = sourceStack.Modules.Count - 1; i >= 0; i--)
				{
					Module module = sourceStack.Modules[i];
					if (module.DamageStatus != EDamageStatus.undamaged)
					{
						sourceStack.RemoveModule(i);
						destination.AddModule(module);
					}
				}
				return;
			}

			for (int i = 0; i < picked.Count; i++)
			{
				Module module = sourceStack.Modules[0];
				sourceStack.RemoveModule(0);
				destination.AddModule(module);
			}
		}

		private void cleanupSource(ModuleStack sourceStack)
		{
			if (sourceStack.Quantity == 0)
			{
				sourceStack.ModuleType = null;
				ModuleStack.All.Remove(sourceStack);
			}
		}

		private void notifyStackContracts(ModuleStack sourceStack, int quantity)
		{
			ModuleStack contractGiver = this.Transferer;
			if (sourceStack.Quantity == 0)
			{
				ModuleStack parent = this.Transferer.Parent as ModuleStack;
				if (parent != null && parent.Owner == this.Transferer.Owner)
				{
					contractGiver = parent;
				}
			}
			Contract.All.NotifyTransfer(
				this.Transferer.Owner,
				contractGiver,
				this.Receiver,
				this.ModuleType,
				quantity);
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
