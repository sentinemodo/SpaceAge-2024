using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public class DeliveringPurchase : Receiving
	{
		public const double CancelRefundRatio = 0.8;

		public IItemStacksHolder Transferer { get; set; }
		public ModuleStack SellerStack { get; set; }
		public ItemStack ItemStack { get; set; }
		public ModuleStack ModuleStack { get; set; }
		public int PurchaseValue { get; set; }
		public int TransferCost { get; set; }
		public int PaidAmount { get; set; }
		public bool IsReturn { get; set; }
		public Region DestinationRegion { get; set; }

		public DeliveringPurchase(ModuleStack receiver)
			: base(receiver, 0)
		{
		}

		public DeliveringPurchase(
			ModuleStack receiver,
			IItemStacksHolder transferer,
			ModuleStack sellerStack,
			ItemStack itemStack,
			int purchaseValue,
			int transferCost,
			Region destinationRegion,
			int duration,
			bool isReturn)
			: base(receiver, duration)
		{
			this.Transferer = transferer;
			this.SellerStack = sellerStack;
			this.ItemStack = itemStack;
			this.PurchaseValue = purchaseValue;
			this.TransferCost = transferCost;
			this.PaidAmount = purchaseValue + transferCost;
			this.IsReturn = isReturn;
			this.DestinationRegion = destinationRegion;
		}

		public DeliveringPurchase(
			ModuleStack receiver,
			ModuleStack transferer,
			ModuleStack sellerStack,
			ModuleStack moduleStack,
			int purchaseValue,
			int transferCost,
			Region destinationRegion,
			int duration,
			bool isReturn)
			: base(receiver, duration)
		{
			this.Transferer = transferer;
			this.SellerStack = sellerStack;
			this.ModuleStack = moduleStack;
			this.PurchaseValue = purchaseValue;
			this.TransferCost = transferCost;
			this.PaidAmount = purchaseValue + transferCost;
			this.IsReturn = isReturn;
			this.DestinationRegion = destinationRegion;
		}

		public override string Description
		{
			get
			{
				string goods;
				if (this.ItemStack != null)
				{
					goods = this.ItemStack.ReportName;
				}
				else
				{
					goods = (this.ModuleStack.Quantity > 1)
						? string.Format("{0} {1}", this.ModuleStack.Quantity, this.ModuleStack.ModuleType.ReportNameMultiple)
						: this.ModuleStack.ModuleType.ReportName;
				}
				if (this.IsReturn)
				{
					return string.Format(
						"returning {0} to {1}, {2} weeks to complete.",
						goods,
						this.Receiver.ReportName,
						this.Duration);
				}
				return string.Format(
					"market delivery of {0} from {1}, {2} weeks to complete.",
					goods,
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

		public void CancelByBuyer(int week)
		{
			if (this.IsReturn || this.Executed)
			{
				return;
			}

			int refund = (int)Math.Floor(this.PurchaseValue * CancelRefundRatio);
			if (refund > 0)
			{
				this.collectFromSeller(week, refund);
				this.creditBuyer(week, refund);
			}

			ItemStack itemPayload = this.ItemStack;
			ModuleStack modulePayload = this.ModuleStack;
			this.ItemStack = null;
			this.ModuleStack = null;
			int remainingDuration = this.Duration;
			Region sellerRegion = (Region)this.SellerStack.Location;

			this.Receiver.EventReports.Add(
				week,
				string.Format(
					"market delivery cancelled, refunded {0} of {1} purchase value.",
					refund,
					this.PurchaseValue));

			this.Executed = true;
			this.Duration = 0;

			if (itemPayload != null)
			{
				DeliveringPurchase returnDelivery = new DeliveringPurchase(
					this.SellerStack,
					this.Receiver,
					this.SellerStack,
					itemPayload,
					0,
					0,
					sellerRegion,
					remainingDuration,
					true);
				returnDelivery.Execute(week);
			}
			else if (modulePayload != null)
			{
				DeliveringPurchase returnDelivery = new DeliveringPurchase(
					this.SellerStack,
					this.Receiver,
					this.SellerStack,
					modulePayload,
					0,
					0,
					sellerRegion,
					remainingDuration,
					true);
				returnDelivery.Execute(week);
			}
		}

		public static void CancelAll(ModuleStack buyer, int week)
		{
			List<DeliveringPurchase> pending = new List<DeliveringPurchase>();
			foreach (Effect effect in buyer.Effects)
			{
				DeliveringPurchase delivery = effect as DeliveringPurchase;
				if (delivery != null && !delivery.Executed && !delivery.IsReturn)
				{
					pending.Add(delivery);
				}
			}
			foreach (DeliveringPurchase delivery in pending)
			{
				delivery.CancelByBuyer(week);
				buyer.Effects.Remove(delivery);
			}
		}

		public static void CancelReturns(ModuleStack mover, int week)
		{
			List<DeliveringPurchase> pending = new List<DeliveringPurchase>();
			foreach (Effect effect in mover.Effects)
			{
				DeliveringPurchase delivery = effect as DeliveringPurchase;
				if (delivery != null && !delivery.Executed && delivery.IsReturn)
				{
					pending.Add(delivery);
				}
			}
			foreach (DeliveringPurchase delivery in pending)
			{
				delivery.destroyReturn(week);
				mover.Effects.Remove(delivery);
			}
		}

		private void destroyReturn(int week)
		{
			this.discardPayload();
			this.Receiver.EventReports.Add(week, "return shipment lost in transit.");
			this.Executed = true;
			this.Duration = 0;
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
				this.ModuleStack = null;
			}
			this.ItemStack = null;
		}

		private void creditBuyer(int week, int refund)
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

		private void collectFromSeller(int week, int amount)
		{
			ModuleStack seller = this.SellerStack;
			int localCash = seller.ItemStacks.Quantity(ItemType.All.Cash);
			int fromLocal = Math.Min(localCash, amount);
			if (fromLocal > 0)
			{
				seller.ItemStacks.Remove(ItemStack.Cash(fromLocal));
			}
			int remaining = amount - fromLocal;
			if (remaining > 0)
			{
				if (seller.HasBankAccess)
				{
					seller.Owner.Bank.Debit(week, remaining, "refund for cancelled market delivery.");
				}
				else
				{
					throw new InvalidOperationException("Market delivery refund exceeded seller local cash without bank access.");
				}
			}
		}

		public override void LoadXml(XmlElement elDeliveringPurchase)
		{
			base.LoadXml(elDeliveringPurchase);
			this.PurchaseValue = this.XMLAssignInteger(elDeliveringPurchase.GetAttribute("purchase-value"), 0);
			this.PaidAmount = this.XMLAssignInteger(elDeliveringPurchase.GetAttribute("paid-amount"), 0);
			if (elDeliveringPurchase.HasAttribute("transfer-cost"))
			{
				this.TransferCost = this.XMLAssignInteger(elDeliveringPurchase.GetAttribute("transfer-cost"), 0);
			}
			else
			{
				this.TransferCost = Math.Max(0, this.PaidAmount - this.PurchaseValue);
			}
			if (this.PurchaseValue == 0 && this.PaidAmount > 0 && !elDeliveringPurchase.HasAttribute("purchase-value"))
			{
				this.PurchaseValue = this.PaidAmount;
			}
			this.IsReturn = this.XMLAssignBoolean(elDeliveringPurchase.GetAttribute("is-return"), false);
			this.DestinationRegion = Region.All[elDeliveringPurchase.GetAttribute("destination-region")];
			this.Transferer = this.findItemStacksHolder(elDeliveringPurchase.GetAttribute("transferrer"));
			this.SellerStack = ModuleStack.All[elDeliveringPurchase.GetAttribute("seller")];
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
			this.xmlElement.SetAttribute("purchase-value", this.PurchaseValue.ToString());
			this.xmlElement.SetAttribute("transfer-cost", this.TransferCost.ToString());
			this.xmlElement.SetAttribute("paid-amount", this.PaidAmount.ToString());
			this.xmlElement.SetAttribute("is-return", this.IsReturn.ToString());
			this.xmlElement.SetAttribute("destination-region", this.DestinationRegion.Name);
			this.xmlElement.SetAttribute("transferrer", this.Transferer.Name);
			this.xmlElement.SetAttribute("seller", this.SellerStack.Name);
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
