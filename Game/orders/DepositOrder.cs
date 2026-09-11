using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public class DepositOrder : ImmediateOrder
	{
		public DepositOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.deposit;
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
			XmlElement elDeposit = (XmlElement)elOrder.SelectNodes("deposit")[0];
			if (elDeposit.GetAttribute("quantity") == "all")
			{
				this.AllQuantity = true;
			}
			else
			{
				this.Quantity = this.XMLAssignInteger(elDeposit.GetAttribute("quantity"), 0);
			}
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			XmlElement elDeposit = doc.CreateElement("deposit");
			elDeposit.SetAttribute("quantity", this.AllQuantity ? "all" : this.Quantity.ToString());
			this.xmlElement.AppendChild(elDeposit);
			return this.xmlElement;
		}

		public override List<string> Report(Faction owner)
		{
			List<string> lines = new List<string>();
			string line = string.Format("{0}{1}deposit {2}",
				this.Conditions,
				(this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")),
				this.AllQuantity ? "all" : this.Quantity.ToString());
			lines.Add(line);
			return lines;
		}

		public override void Execute(int week)
		{
			this.Executed = false;
			ItemType cash = ItemType.All.Cash;
			int available = 0;
			if (this.Holder.ItemStacks.ContainsKey(cash))
			{
				available = this.Holder.ItemStacks[cash].Quantity;
			}

			int amount = this.AllQuantity ? available : this.Quantity;
			if (amount < 1 || amount > available)
			{
				if (!this.FailedToExecute)
				{
					this.Holder.EventReports.Add(
						week,
						string.Format("DEPOSIT failed. {0} cash [cash] available, {1} requested.",
							available,
							this.AllQuantity ? "all" : this.Quantity.ToString()));
					this.FailedToExecute = true;
				}
				base.Execute(week);
				return;
			}

			this.Holder.ItemStacks.Minus(cash, amount);
			this.Holder.Owner.Bank.Credit(
				week,
				amount,
				string.Format("deposit from {0}.", this.Holder.ReportName));
			this.Holder.EventReports.Add(
				week,
				string.Format("deposited {0} cash [cash] to bank account.", amount));
			this.Executed = true;
			base.Execute(week);
		}
	}
}
