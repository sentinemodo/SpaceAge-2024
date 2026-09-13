using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public class WithdrawOrder : ImmediateOrder
	{
		public WithdrawOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.withdraw;
		}

		public IItemStacksHolder Holder
		{
			get { return (IItemStacksHolder)this.Subject; }
		}

		public int Quantity { get; set; }
		public bool AllQuantity { get; set; }

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
				this.AllQuantity = true;
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
					throw new Exception("bad syntax ALL or quantity expected", ex);
				}
			}

			if (!string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax");
			}
		}

		public override void LoadXml(XmlElement elOrder)
		{
			XmlElement elWithdraw = (XmlElement)elOrder.SelectNodes("withdraw")[0];
			if (elWithdraw.GetAttribute("quantity") == "all")
			{
				this.AllQuantity = true;
			}
			else
			{
				this.Quantity = this.XMLAssignInteger(elWithdraw.GetAttribute("quantity"), 0);
			}
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			XmlElement elWithdraw = doc.CreateElement("withdraw");
			elWithdraw.SetAttribute("quantity", this.AllQuantity ? "all" : this.Quantity.ToString());
			this.xmlElement.AppendChild(elWithdraw);
			return this.xmlElement;
		}

		public override List<string> Report(Faction owner)
		{
			List<string> lines = new List<string>();
			string line = string.Format("{0}{1}withdraw {2}",
				this.Conditions,
				(this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")),
				this.AllQuantity ? "all" : this.Quantity.ToString());
			lines.Add(line);
			return lines;
		}

		public override void Execute(int week)
		{
			this.Executed = false;
			int available = Math.Max(0, (int)Math.Floor(this.Holder.Owner.Bank.Balance));
			int amount = this.AllQuantity ? available : this.Quantity;

			if (amount < 1 || amount > available)
			{
				if (!this.FailedToExecute)
				{
					this.Holder.EventReports.Add(
						week,
						string.Format("WITHDRAW failed. {0} cash [cash] in bank account, {1} requested.",
							available,
							this.AllQuantity ? "all" : this.Quantity.ToString()));
					this.FailedToExecute = true;
				}
				base.Execute(week);
				return;
			}

			ItemStack transfer = ItemStack.Cash(amount);
			if (transfer.Size > 0 && this.Holder.Capacity - this.Holder.CapacityUsed < transfer.Size)
			{
				if (!this.FailedToExecute)
				{
					this.Holder.EventReports.Add(
						week,
						string.Format("WITHDRAW failed, not enough capacity for {0} cash [cash].",
							amount));
					this.FailedToExecute = true;
				}
				base.Execute(week);
				return;
			}

			this.Holder.Owner.Bank.Debit(
				week,
				amount,
				string.Format("withdrawal to {0}.", this.Holder.ReportName));
			this.Holder.ItemStacks.Add(transfer);
			this.Holder.EventReports.Add(
				week,
				string.Format("withdrew {0} cash [cash] from bank account.", amount));
			this.Executed = true;
			base.Execute(week);
		}
	}
}
