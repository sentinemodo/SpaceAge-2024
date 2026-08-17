using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public class Contract : NamedObject, IReporting
	{
		public static readonly Contracts All = new Contracts();

		public new Region Location { get; set; }
		public Faction Issuer { get; set; }
		public IContractTrigger Trigger { get; set; }
		public Technology RewardTechnology { get; set; }
		public bool CreatedThisSession { get; set; }

		public Contract(string name)
			: base(name)
		{
			if (string.IsNullOrEmpty(this.name))
			{
				this.name = this.GenerateRandomIdentifier("c");
				while (Contract.All.Contains(this.name))
				{
					this.name = this.GenerateRandomIdentifier("c");
				}
			}
			else if (Contract.All.Contains(this.name))
			{
				throw new Exception("Contract with name " + this.name + " already exists");
			}

			Contract.All.Add(this);
		}

		public Contract(string name, Region location, Faction issuer, IContractTrigger trigger, Technology reward)
			: this(name)
		{
			this.Location = location;
			this.Issuer = issuer;
			this.Trigger = trigger;
			this.RewardTechnology = reward;
		}

		public static Contract Load(XmlElement elContract)
		{
			Contract contract = new Contract(elContract.GetAttribute("name"));
			contract.LoadXml(elContract);
			return contract;
		}

		public override void LoadXml(XmlElement elContract)
		{
			base.LoadXml(elContract);
			this.Location = Region.All[elContract.GetAttribute("location")];
			this.Issuer = Faction.All[elContract.GetAttribute("issuer")];

			string rewardType = elContract.GetAttribute("reward-type");
			if (!string.IsNullOrEmpty(rewardType) && rewardType != "technology")
			{
				throw new Exception("Unknown contract reward type: " + rewardType);
			}
			this.RewardTechnology = Technology.All[elContract.GetAttribute("reward")];
			if (this.RewardTechnology == null)
			{
				throw new Exception("Unknown contract reward technology: " + elContract.GetAttribute("reward"));
			}

			string triggerType = elContract.GetAttribute("trigger");
			if (triggerType == "give-module")
			{
				this.Trigger = GiveModuleTrigger.Load(elContract);
			}
			else
			{
				throw new Exception("Unknown contract trigger: " + triggerType);
			}
		}

		public override XmlElement SaveXml(XmlDocument doc)
		{
			XmlElement elContract = doc.CreateElement("contract");
			elContract.SetAttribute("name", this.Name);
			elContract.SetAttribute("location", this.Location.Name);
			elContract.SetAttribute("issuer", this.Issuer.Name);
			elContract.SetAttribute("trigger", this.Trigger.TypeName);
			elContract.SetAttribute("reward-type", "technology");
			elContract.SetAttribute("reward", this.RewardTechnology.Name);
			this.Trigger.SaveAttributes(elContract);
			this.xmlElement = elContract;
			return elContract;
		}

		public bool Evaluate(int week)
		{
			if (this.Trigger == null || !this.Trigger.IsComplete())
			{
				return false;
			}

			this.Award(week);
			return true;
		}

		public void Award(int week)
		{
			Faction winner = this.Trigger.Winner;
			if (winner == null || this.RewardTechnology == null)
			{
				return;
			}

			if (!winner.TechnologiesSeen.Contains(this.RewardTechnology.Name))
			{
				winner.TechnologiesSeen.Add(this.RewardTechnology);
			}
			if (!winner.TechnologiesToShow.Contains(this.RewardTechnology.Name))
			{
				winner.TechnologiesToShow.Add(this.RewardTechnology);
			}

			winner.EventReports.Add(
				week,
				string.Format("completed contract {0} and received {1} technology.",
					this.Name,
					this.RewardTechnology.ReportName));

			if (this.Location != null)
			{
				this.Location.EventReports.Add(
					week,
					string.Format("contract {0} completed by {1}.",
						this.Name,
						winner.ReportName));
			}
		}

		public List<string> Report(Faction faction)
		{
			return this.Report();
		}

		public List<string> Report()
		{
			GiveModuleTrigger give = this.Trigger as GiveModuleTrigger;
			List<string> lines = new List<string>();
			if (give == null)
			{
				lines.Add(string.Format("  {0}.", this.Name));
				return lines;
			}

			string moduleName = (give.Quantity == 1)
				? give.ModuleType.ReportName
				: give.ModuleType.ReportNameMultiple;
			lines.Add(string.Format("  {0}: deliver {1} {2} to {3}.",
				this.Name,
				give.Quantity,
				moduleName,
				give.Receiver.ReportName));
			lines.Add(string.Format("    Reward: {0} technology.",
				this.RewardTechnology.ReportName));
			return lines;
		}
	}
}
