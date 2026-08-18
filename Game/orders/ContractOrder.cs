using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	// Faction-level immediate order: publish a location contract, or withdraw one by id.
	public class ContractOrder : ImmediateOrder
	{
		public override bool AllowedBetweenTurns
		{
			get { return true; }
		}

		public bool IsWithdraw { get; set; }
		public string ContractName { get; set; }
		public Region Location { get; set; }
		public int Quantity { get; set; }
		public ModuleType ModuleType { get; set; }
		public ModuleStack Receiver { get; set; }
		public Technology RewardTechnology { get; set; }

		public Faction Issuer
		{
			get { return this.Subject as Faction; }
		}

		public ContractOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.contract;
		}

		public override void Parse(string command)
		{
			string token = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(token))
			{
				throw new Exception("Bad syntax, location or contract id expected.");
			}

			string second = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(second))
			{
				throw new Exception("Bad syntax, give or WITHDRAW expected.");
			}

			if (second.ToLowerInvariant() == "withdraw")
			{
				this.IsWithdraw = true;
				this.ContractName = token;
				return;
			}

			this.IsWithdraw = false;
			if (!Region.All.ContainsKey(token))
			{
				throw new Exception("Unknown location: " + token);
			}
			this.Location = Region.All[token];

			if (second.ToLowerInvariant() != "give")
			{
				throw new Exception("Bad syntax, give expected.");
			}

			string quantityToken = LineParser.GetToken(ref command);
			try
			{
				this.Quantity = Convert.ToInt32(quantityToken);
				if (this.Quantity < 1)
				{
					throw new Exception("Bad syntax, positive amount expected.");
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Bad syntax, quantity expected.", ex);
			}

			string moduleName = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(moduleName) || !ModuleType.All.ContainsKey(moduleName))
			{
				throw new Exception("Unknown module: " + moduleName);
			}
			this.ModuleType = ModuleType.All[moduleName];

			string toToken = LineParser.GetToken(ref command);
			if (toToken.ToLowerInvariant() != "to")
			{
				throw new Exception("Bad syntax, TO expected.");
			}

			string receiverName = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(receiverName) || !ModuleStack.All.ContainsKey(receiverName))
			{
				throw new Exception("Unknown stack: " + receiverName);
			}
			this.Receiver = ModuleStack.All[receiverName];

			string rewardToken = LineParser.GetToken(ref command);
			if (rewardToken.ToLowerInvariant() != "reward")
			{
				throw new Exception("Bad syntax, REWARD expected.");
			}

			string technologyName = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(technologyName) || Technology.All[technologyName] == null)
			{
				throw new Exception("Unknown technology: " + technologyName);
			}
			this.RewardTechnology = Technology.All[technologyName];
		}

		public override void Execute(int week)
		{
			this.Executed = false;
			Faction issuer = this.Issuer;
			if (issuer == null)
			{
				base.Execute(week);
				return;
			}

			if (this.IsWithdraw)
			{
				Contract contract = Contract.All[this.ContractName];
				if (contract == null)
				{
					issuer.EventReports.Add(week, string.Format("CONTRACT failed. Unknown contract {0}.", this.ContractName));
					base.Execute(week);
					return;
				}

				Contract.All.Remove(contract);
				issuer.EventReports.Add(week, string.Format("withdrew contract {0}.", contract.Name));
				this.Executed = true;
				base.Execute(week);
				return;
			}

			if (this.Location == null || this.Receiver == null || this.ModuleType == null || this.RewardTechnology == null)
			{
				issuer.EventReports.Add(week, "CONTRACT failed. Location, stack or technology unknown.");
				base.Execute(week);
				return;
			}

			GiveModuleTrigger trigger = new GiveModuleTrigger(this.Quantity, this.ModuleType, this.Receiver);
			Contract published = new Contract(this.ContractName, this.Location, issuer, trigger, this.RewardTechnology);
			published.CreatedThisSession = true;
			issuer.EventReports.Add(week, string.Format("published contract {0}.", published.Name));
			this.Executed = true;
			base.Execute(week);
		}

		public override void LoadXml(XmlElement elOrder)
		{
			XmlElement elContract = (XmlElement)elOrder.SelectNodes("contract")[0];
			if (elContract.HasAttribute("withdraw"))
			{
				this.IsWithdraw = true;
				this.ContractName = elContract.GetAttribute("withdraw");
				return;
			}

			this.IsWithdraw = false;
			this.Location = Region.All[elContract.GetAttribute("location")];
			this.Quantity = this.XMLAssignInteger(elContract.GetAttribute("quantity"), 1);
			this.ModuleType = ModuleType.All[elContract.GetAttribute("module")];
			this.Receiver = ModuleStack.All[elContract.GetAttribute("receiver")];
			this.RewardTechnology = Technology.All[elContract.GetAttribute("reward")];
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			XmlElement elContract = doc.CreateElement("contract");
			if (this.IsWithdraw)
			{
				elContract.SetAttribute("withdraw", this.ContractName);
			}
			else
			{
				elContract.SetAttribute("location", this.Location.Name);
				elContract.SetAttribute("quantity", this.Quantity.ToString());
				elContract.SetAttribute("module", this.ModuleType.Name);
				elContract.SetAttribute("receiver", this.Receiver.Name);
				elContract.SetAttribute("reward-type", "technology");
				elContract.SetAttribute("reward", this.RewardTechnology.Name);
			}
			this.xmlElement.AppendChild(elContract);
			return this.xmlElement;
		}

		public override List<string> Report(Faction owner)
		{
			string prefix = string.Format("{0}{1}",
				this.Conditions,
				(this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")));
			List<string> lines = new List<string>();
			if (this.IsWithdraw)
			{
				lines.Add(string.Format("{0}contract {1} withdraw", prefix, this.ContractName));
			}
			else
			{
				lines.Add(string.Format("{0}contract {1} give {2} {3} to {4} reward {5} technology",
					prefix,
					this.Location.Name,
					this.Quantity,
					this.ModuleType.Name,
					this.Receiver.Name,
					this.RewardTechnology.Name));
			}
			return lines;
		}
	}
}
