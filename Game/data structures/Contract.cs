using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public class Contract : NamedObject, IReporting
	{
		public const string NamePrefix = "CT";

		public static readonly Contracts All = new Contracts();

		public new Region Location { get; set; }
		public Faction Issuer { get; set; }
		public IContractTrigger Trigger { get; set; }
		public Technology RewardTechnology { get; set; }
		public ModuleStack RewardStack { get; set; }
		public string Title { get; set; }
		public string Flavour { get; set; }
		public bool CreatedThisSession { get; set; }

		public Contract(string name)
			: base(name)
		{
			if (string.IsNullOrEmpty(this.name))
			{
				this.name = Contract.GenerateName();
				while (Contract.All.Contains(this.name))
				{
					this.name = Contract.GenerateName();
				}
			}
			else if (Contract.All.Contains(this.name))
			{
				throw new Exception("Contract with name " + this.name + " already exists");
			}

			Contract.All.Add(this);
		}

		public static string GenerateName()
		{
			int numericLength = NamedObject.MaxNameLength - Contract.NamePrefix.Length;
			string numeric = Sequence.GenerateRandomString(numericLength, "Random identifier");
			return Contract.NamePrefix + numeric.PadLeft(numericLength, '0');
		}

		public Contract(string name, Region location, Faction issuer, IContractTrigger trigger, Technology reward)
			: this(name)
		{
			this.Location = location;
			this.Issuer = issuer;
			this.Trigger = trigger;
			this.RewardTechnology = reward;
		}

		public Contract(string name, Region location, Faction issuer, IContractTrigger trigger, ModuleStack rewardStack)
			: this(name)
		{
			this.Location = location;
			this.Issuer = issuer;
			this.Trigger = trigger;
			this.RewardStack = rewardStack;
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
			this.Title = elContract.GetAttribute("title");
			this.Flavour = elContract.GetAttribute("flavour");

			string rewardType = elContract.GetAttribute("reward-type");
			if (string.IsNullOrEmpty(rewardType) || rewardType == "technology")
			{
				this.RewardTechnology = Technology.All[elContract.GetAttribute("reward")];
				if (this.RewardTechnology == null)
				{
					throw new Exception("Unknown contract reward technology: " + elContract.GetAttribute("reward"));
				}
			}
			else if (rewardType == "unit")
			{
				this.RewardStack = ModuleStack.All[elContract.GetAttribute("reward")];
				if (this.RewardStack == null)
				{
					throw new Exception("Unknown contract reward unit: " + elContract.GetAttribute("reward"));
				}
			}
			else
			{
				throw new Exception("Unknown contract reward type: " + rewardType);
			}

			string triggerType = elContract.GetAttribute("trigger");
			if (triggerType == "give-module")
			{
				this.Trigger = GiveModuleTrigger.Load(elContract);
			}
			else if (triggerType == "research")
			{
				this.Trigger = ResearchWreckageTrigger.Load(elContract);
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
			if (!string.IsNullOrEmpty(this.Title))
			{
				elContract.SetAttribute("title", this.Title);
			}
			if (!string.IsNullOrEmpty(this.Flavour))
			{
				elContract.SetAttribute("flavour", this.Flavour);
			}
			if (this.RewardStack != null)
			{
				elContract.SetAttribute("reward-type", "unit");
				elContract.SetAttribute("reward", this.RewardStack.Name);
			}
			else
			{
				elContract.SetAttribute("reward-type", "technology");
				elContract.SetAttribute("reward", this.RewardTechnology.Name);
			}
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
			if (winner == null)
			{
				return;
			}

			if (this.RewardStack != null)
			{
				this.awardUnit(week, winner);
				return;
			}

			if (this.RewardTechnology == null)
			{
				return;
			}

			GiveModuleTrigger give = this.Trigger as GiveModuleTrigger;
			ModuleStack rewardStack = (give != null) ? give.LastGiverStack : null;
			if (rewardStack != null && ModuleStack.All.ContainsKey(rewardStack.Name))
			{
				rewardStack.ReceiveTechnologyCopy(
					this.RewardTechnology,
					week,
					string.Format("received copy of {0} technology.",
						this.RewardTechnology.ReportName));
			}

			winner.EventReports.Add(
				week,
				string.Format("completed contract {0} and received a copy of {1} technology.",
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

		private void awardUnit(int week, Faction winner)
		{
			this.RewardStack.SetOwnerRecursive(winner);
			foreach (Technology technology in this.RewardStack.CollectTechnologiesRecursive())
			{
				if (!winner.TechnologiesToShow.Contains(technology.Name)
					&& !winner.TechnologiesSeen.Contains(technology.Name))
				{
					winner.TechnologiesToShow.Add(technology);
				}
			}

			winner.EventReports.Add(
				week,
				string.Format("completed contract {0} and received {1}. Systems activated.",
					this.Name,
					this.RewardStack.ReportName));
			this.RewardStack.EventReports.Add(
				week,
				"activated onboard systems.");

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
			List<string> lines = new List<string>();
			if (!string.IsNullOrEmpty(this.Title))
			{
				lines.Add(string.Format("  {0}: {1}.", this.Name, this.Title));
			}

			ResearchWreckageTrigger research = this.Trigger as ResearchWreckageTrigger;
			if (research != null)
			{
				if (string.IsNullOrEmpty(this.Title))
				{
					lines.Add(string.Format("  {0}: research {1}.",
						this.Name,
						research.Target.ReportName));
				}
				else
				{
					lines.Add(string.Format("    Research {0} ({1}/{2} research points).",
						research.Target.ReportName,
						research.Progress,
						research.RequiredPoints));
				}
				if (!string.IsNullOrEmpty(this.Flavour))
				{
					lines.Add(string.Format("    {0}", this.Flavour));
				}
				if (this.RewardStack != null)
				{
					lines.Add(string.Format("    Reward: {0}.", this.RewardStack.ReportName));
				}
				return lines;
			}

			GiveModuleTrigger give = this.Trigger as GiveModuleTrigger;
			if (give == null)
			{
				if (lines.Count == 0)
				{
					lines.Add(string.Format("  {0}.", this.Name));
				}
				return lines;
			}

			string moduleName = (give.Quantity == 1)
				? give.ModuleType.ReportName
				: give.ModuleType.ReportNameMultiple;
			if (string.IsNullOrEmpty(this.Title))
			{
				lines.Add(string.Format("  {0}: deliver {1} {2} to {3}.",
					this.Name,
					give.Quantity,
					moduleName,
					give.Receiver.ReportName));
			}
			else
			{
				lines.Add(string.Format("    Deliver {0} {1} to {2}.",
					give.Quantity,
					moduleName,
					give.Receiver.ReportName));
			}
			if (!string.IsNullOrEmpty(this.Flavour))
			{
				lines.Add(string.Format("    {0}", this.Flavour));
			}
			if (this.RewardTechnology != null)
			{
				lines.Add(string.Format("    Reward: {0} technology.",
					this.RewardTechnology.ReportName));
			}
			return lines;
		}
	}
}
