using System.Collections.Generic;

namespace SpaceAge
{
	public class BankTransaction
	{
		public BankTransaction(int week, double amount, string reason, double balanceAfter)
		{
			this.Week = week;
			this.Amount = amount;
			this.Reason = reason;
			this.BalanceAfter = balanceAfter;
		}

		public int Week { get; private set; }
		public double Amount { get; private set; }
		public string Reason { get; private set; }
		public double BalanceAfter { get; private set; }

		public string ReportLine(int indentLevel)
		{
			string prefix = new ReportLine(string.Empty, indentLevel).IndentationString;
			string action = this.Amount >= 0 ? "credited" : "debited";
			double magnitude = this.Amount >= 0 ? this.Amount : -this.Amount;
			return string.Format(
				"{0}week {1}: Bank account {2}: {3} cash ({4}). Balance: {5}.",
				prefix,
				this.Week,
				action,
				magnitude.ToString("0"),
				this.Reason,
				this.BalanceAfter.ToString("0"));
		}
	}

	public class BankTransactions : List<BankTransaction>
	{
		public List<string> Report(int indentLevel)
		{
			List<string> lines = new List<string>();
			if (this.Count == 0)
			{
				return lines;
			}

			lines.Add(string.Format("{0}Account activity:", new ReportLine(string.Empty, indentLevel).IndentationString));
			foreach (BankTransaction transaction in this)
			{
				lines.Add(transaction.ReportLine(indentLevel + 1));
			}
			return lines;
		}
	}
}
