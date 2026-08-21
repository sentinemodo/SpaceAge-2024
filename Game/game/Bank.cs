using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
    //TODO: rename to bank account and create bank as a separate class, to which the credit rates should be moved
	public class Bank : IReporting
	{
        public Bank (Faction faction)
			: base()
		{
			this.faction = faction;
		}

        private Faction faction;

		private double balance;
		public double Balance
		{
			get { return this.balance; }
			set 
			{ 
				this.balance = Math.Round(value, MidpointRounding.AwayFromZero);
				if (this.balance < -this.creditLine)
					throw new Exception("credit line exceeded");
			}
		}

		private double creditLine;
        public double CreditLine
		{
			get { return this.creditLine; }
			set { this.creditLine = value; }
		}

		private double creditRate;
		public double CreditRate
		{
			get { return this.creditRate; }
			set { this.creditRate = value; }
		}

		private double depositRate;
		public double DepositRate
		{
			get { return this.depositRate; }
			set { this.depositRate = value; }
		}

        public double AvailableFunds
		{
			get { return this.balance + this.creditLine; }
		}

        public void Credit(int week, double amount, string title)
        {
            this.Balance += amount;
            this.faction.EventReports.Add(week, string.Concat("Bank account credited: ", title));
        }

        public void Debit(int week, double amount, string title)
        {
            this.Balance -= amount;
            this.faction.EventReports.Add(week, string.Concat("Bank account debited: ", title));
        }

		public void AddQuarterlyInterest(int week)
		{
            // credit/deposit rate is divided by for because of quarterly accrual
            double interest;
            if (this.balance < 0)
			{
                interest = Math.Round(this.Balance * this.creditRate / 4, MidpointRounding.AwayFromZero);
                this.Debit(week, interest, string.Format("Credit line interests accounted to {0}. Current balance: {1}.",
                            interest,
                            this.balance + interest));
			} else
			{
                interest = Math.Round(this.Balance * this.depositRate / 4, MidpointRounding.AwayFromZero);
                this.Credit(week, interest, string.Format("Deposit interests accounted to {0}. Current balance: {1}.",
                            interest,
                            this.balance + interest));
            }
		}

		#region IReporting Members

		public List<string>  Report(Faction faction)
		{
			List<string> reportLines = new List<string>();

			reportLines.AddRange(this.reportHeader(faction));

			return reportLines;
		}

		private List<string> reportHeader(Faction faction)
		{
			List<string> lines = new List<string>
            {
                "Bank report:",
                string.Format("  Bank account balance: {0}.", this.Balance.ToString("0")),
                string.Format("  Credit line maximum: {0}.", this.CreditLine),
                string.Format("  Credit rate: {0}%, Deposit rate: {1}%.", this.CreditRate * 100, this.DepositRate * 100)
            };
			return lines;
		}

		#endregion
	}
}
