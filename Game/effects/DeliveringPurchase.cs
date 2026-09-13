using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public class DeliveringPurchase : Receiving
	{
		public const double CancelRefundRatio = 0.8;

		public IItemStacksHolder Transferer { get; set; }
		public ItemStack ItemStack { get; set; }
		public ModuleStack ModuleStack { get; set; }
		public int PaidAmount { get; set; }
		public Region DestinationRegion { get; set; }

		public DeliveringPurchase(ModuleStack receiver)
			: base(receiver, 0)
		{
		}

		public DeliveringPurchase(
			ModuleStack receiver,
			IItemStacksHolder transferer,
			ItemStack itemStack,
			int paidAmount,
			Region destinationRegion,
			int duration)
			: base(receiver, duration)
		{
			this.Transferer = transferer;
			this.ItemStack = itemStack;
			this.PaidAmount = paidAmount;
			this.DestinationRegion = destinationRegion;
		}

		public DeliveringPurchase(
			ModuleStack receiver,
			ModuleStack transferer,
			ModuleStack moduleStack,
			int paidAmount,
			Region destinationRegion,
			int duration)
			: base(receiver, duration)
		{
			this.Transferer = transferer;
			this.ModuleStack = moduleStack;
			this.PaidAmount = paidAmount;
			this.DestinationRegion = destinationRegion;
		}

		public override string Description
		{
			get
			{
				if (this.ItemStack != null)
				{
					return string.Format(
						"market delivery of {0} from {1}, {2} weeks to complete.",
						this.ItemStack.ReportName,
						this.Transferer.ReportName,
						this.Duration);
				}
				return string.Format(
					"market delivery of {0} from {1}, {2} weeks to complete.",
					(this.ModuleStack.Quantity > 1)
						? string.Format("{0} {1}", this.ModuleStack.Quantity, this.ModuleStack.ModuleType.ReportNameMultiple)
						: this.ModuleStack.ModuleType.ReportName,
					this.Transferer.ReportName,
					this.Duration);
			}
		}

		public override void Execute(int week)
		{
			base.Execute(week);
			if (this.Duration == 0)
			{
				this.completeDelivery(week);
				this.Executed = true;
			}
		}

		public void Cancel(int week)
		{
			int refund = (int)Math.Floor(this.PaidAmount * CancelRefundRatio);
			if (refund > 0)
			{
				this.refundBuyer(week, refund);
			}
			this.discardPayload();
			this.Receiver.EventReports.Add(
				week,
				string.Format(
					"market delivery cancelled, refunded {0} of {1} paid.",
					refund,
					this.PaidAmount));
			this.Executed = true;
			this.Duration = 0;
		}

		public static void CancelAll(ModuleStack buyer, int week)
		{
			List<DeliveringPurchase> pending = new List<DeliveringPurchase>();
			foreach (Effect effect in buyer.Effects)
			{
				DeliveringPurchase delivery = effect as DeliveringPurchase;
				if (delivery != null && !delivery.Executed)
				{
					pending.Add(delivery);
				}
			}
			foreach (DeliveringPurchase delivery in pending)
			{
				delivery.Cancel(week);
				buyer.Effects.Remove(delivery);
			}
		}

		private void completeDelivery(int week)
		{
			if (this.ItemStack != null)
			{
				this.Receiver.ItemStacks.Add(this.ItemStack);
				return;
			}

			if (this.ModuleStack == null)
			{
				return;
			}

			this.Receiver.EventReports.Add(
				week,
				string.Format(
					"received {0} from {1}.",
					(this.ModuleStack.Quantity > 1)
						? string.Format("{0} {1}", this.ModuleStack.Quantity, this.ModuleStack.ModuleType.ReportNameMultiple)
						: this.ModuleStack.ModuleType.ReportName,
					this.Transferer.ReportName));

			if (this.Receiver.ModuleType == null)
			{
				this.Receiver.ModuleType = this.ModuleStack.ModuleType;
			}

			if (this.Receiver.ModuleType == this.ModuleStack.ModuleType)
			{
				Module module;
				int quantity = this.ModuleStack.Quantity;
				for (int i = 0; i < quantity; i++)
				{
					module = this.ModuleStack.Modules[0];
					this.ModuleStack.RemoveModule(0);
					this.Receiver.AddModule(module);
				}
				if (this.ModuleStack.ExecutedLongOrder)
				{
					this.Receiver.ExecutedLongOrder = true;
				}
				ModuleStack.All.Remove(this.ModuleStack);
			}
			else
			{
				this.ModuleStack.Parent = this.Receiver;
				this.ModuleStack.Owner = this.Receiver.Owner;
			}
		}

		private void discardPayload()
		{
			if (this.ModuleStack != null)
			{
				ModuleStack.All.Remove(this.ModuleStack);
			}
		}

		private void refundBuyer(int week, int refund)
		{
			if (this.Receiver.HasBankAccess)
			{
				this.Receiver.Owner.Bank.Credit(week, refund, "refund from cancelled market delivery.");
			}
			else
			{
				this.Receiver.ItemStacks.Add(ItemStack.Cash(refund));
			}
		}

		public override void LoadXml(XmlElement elDeliveringPurchase)
		{
			base.LoadXml(elDeliveringPurchase);
			this.PaidAmount = this.XMLAssignInteger(elDeliveringPurchase.GetAttribute("paid-amount"), 0);
			this.DestinationRegion = Region.All[elDeliveringPurchase.GetAttribute("destination-region")];
			this.Transferer = this.findItemStacksHolder(elDeliveringPurchase.GetAttribute("transferrer"));
			if (elDeliveringPurchase.GetAttribute("payload") == "items")
			{
				XmlElement elReceiving = (XmlElement)elDeliveringPurchase.SelectSingleNode("receiving");
				this.ItemStack = new ItemStack(ItemType.All[elReceiving.GetAttribute("type")]);
				this.ItemStack.LoadXml(elReceiving);
			}
			else
			{
				this.ModuleStack = ModuleStack.All.GetOrCreateNewModuleStack(
					this.Receiver.Owner,
					elDeliveringPurchase.GetAttribute("modulestack"));
			}
		}

		public override XmlElement SaveXml(XmlDocument doc)
		{
			base.SaveXml(doc);
			this.xmlElement.SetAttribute("type", "delivering-purchase");
			this.xmlElement.SetAttribute("paid-amount", this.PaidAmount.ToString());
			this.xmlElement.SetAttribute("destination-region", this.DestinationRegion.Name);
			this.xmlElement.SetAttribute("transferrer", this.Transferer.Name);
			if (this.ItemStack != null)
			{
				this.xmlElement.SetAttribute("payload", "items");
				this.xmlElement.AppendChild(this.ItemStack.SaveXml(doc, "receiving"));
			}
			else
			{
				this.xmlElement.SetAttribute("payload", "modules");
				this.xmlElement.SetAttribute("modulestack", this.ModuleStack.Name);
			}
			return this.xmlElement;
		}

		private IItemStacksHolder findItemStacksHolder(string name)
		{
			Person person = Person.All[name];
			if (person != null)
			{
				return person;
			}
			return ModuleStack.All.GetOrCreateNewModuleStack(this.Receiver.Owner, name);
		}
	}
}
