using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public enum EGrantKind
	{
		technology,
		skill,
		item,
		module
	}

	public class GrantOrder : ImmediateOrder
	{
		public override bool AllowedBetweenTurns
		{
			get { return true; }
		}

		public EGrantKind GrantKind { get; set; }
		public Technology Technology { get; set; }
		public SkillType SkillType { get; set; }
		public ItemType ItemType { get; set; }
		public ModuleType ModuleType { get; set; }
		public int Quantity { get; set; }
		public string TargetName { get; set; }
		public ModuleStack TargetStack { get; set; }
		public Person TargetPerson { get; set; }

		public Faction Issuer
		{
			get { return this.Subject as Faction; }
		}

		public GrantOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.grant;
			this.Quantity = 1;
		}

		public override void Parse(string command)
		{
			string kindToken = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(kindToken))
			{
				throw new Exception("Bad syntax, GRANT kind expected (technology, skill, item, module).");
			}

			switch (kindToken.ToLowerInvariant())
			{
				case "technology":
					this.GrantKind = EGrantKind.technology;
					this.parseTechnology(ref command);
					break;
				case "skill":
					this.GrantKind = EGrantKind.skill;
					this.parseSkill(ref command);
					break;
				case "item":
					this.GrantKind = EGrantKind.item;
					this.parseItem(ref command);
					break;
				case "module":
					this.GrantKind = EGrantKind.module;
					this.parseModule(ref command);
					break;
				default:
					throw new Exception("Bad syntax, GRANT kind must be technology, skill, item, or module.");
			}

			this.parseToTarget(ref command);
		}

		private void parseTechnology(ref string command)
		{
			string token = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(token) || !Technology.All.Contains(token))
			{
				throw new Exception("Bad syntax or unknown technology.");
			}
			this.Technology = Technology.All[token];
		}

		private void parseSkill(ref string command)
		{
			string token = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(token) || !SkillType.All.ContainsKey(token))
			{
				throw new Exception("Bad syntax or unknown skill.");
			}
			this.SkillType = SkillType.All[token];
		}

		private void parseItem(ref string command)
		{
			string quantityToken = LineParser.GetToken(ref command);
			try
			{
				this.Quantity = Convert.ToInt32(quantityToken);
			}
			catch (Exception ex)
			{
				throw new Exception("Bad syntax, item quantity expected.", ex);
			}
			if (this.Quantity <= 0)
			{
				throw new Exception("Bad syntax, item quantity must be positive.");
			}

			string itemToken = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(itemToken) || !ItemType.All.ContainsKey(itemToken))
			{
				throw new Exception("Bad syntax or unknown item type.");
			}
			this.ItemType = ItemType.All[itemToken];
		}

		private void parseModule(ref string command)
		{
			string quantityToken = LineParser.GetToken(ref command);
			try
			{
				this.Quantity = Convert.ToInt32(quantityToken);
			}
			catch (Exception ex)
			{
				throw new Exception("Bad syntax, module quantity expected.", ex);
			}
			if (this.Quantity <= 0)
			{
				throw new Exception("Bad syntax, module quantity must be positive.");
			}

			string moduleToken = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(moduleToken) || !ModuleType.All.ContainsKey(moduleToken))
			{
				throw new Exception("Bad syntax or unknown module type.");
			}
			this.ModuleType = ModuleType.All[moduleToken];
		}

		private void parseToTarget(ref string command)
		{
			string toToken = LineParser.GetToken(ref command);
			if (toToken == null || toToken.ToLowerInvariant() != "to")
			{
				throw new Exception("Bad syntax, TO expected.");
			}

			this.TargetName = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(this.TargetName))
			{
				throw new Exception("Bad syntax, target id expected.");
			}
		}

		public int CostCredits()
		{
			switch (this.GrantKind)
			{
				case EGrantKind.technology:
					return GrantCost.TechnologyCredits(this.Technology);
				case EGrantKind.skill:
					return GrantCost.SkillCredits(this.SkillType);
				case EGrantKind.item:
					return GrantCost.ItemCredits(this.ItemType, this.Quantity);
				case EGrantKind.module:
					return GrantCost.ModuleCredits(this.ModuleType, this.Quantity);
				default:
					return 0;
			}
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

			int cost = this.CostCredits();
			if (cost > GrantCost.MaxCreditsPerLine)
			{
				issuer.EventReports.Add(
					week,
					string.Format("GRANT failed. Cost {0} exceeds single-line limit {1}.", cost, GrantCost.MaxCreditsPerLine));
				base.Execute(week);
				return;
			}

			if (cost > 0 && issuer.Bank.AvailableFunds < cost)
			{
				issuer.EventReports.Add(
					week,
					string.Format("GRANT failed. Insufficient bank funds (need {0}, available {1}).", cost, issuer.Bank.AvailableFunds));
				base.Execute(week);
				return;
			}

			if (!this.resolveTarget(issuer, week))
			{
				base.Execute(week);
				return;
			}

			if (!this.checkEligibility(issuer, week))
			{
				base.Execute(week);
				return;
			}

			if (!this.deliver(issuer, week))
			{
				base.Execute(week);
				return;
			}

			if (cost > 0)
			{
				issuer.Bank.Debit(week, cost, this.debitTitle());
			}

			this.Executed = true;
			base.Execute(week);
		}

		private bool resolveTarget(Faction issuer, int week)
		{
			if (this.GrantKind == EGrantKind.skill)
			{
				if (!Person.All.ContainsKey(this.TargetName))
				{
					issuer.EventReports.Add(week, string.Format("GRANT failed. Unknown person {0}.", this.TargetName));
					return false;
				}
				this.TargetPerson = Person.All[this.TargetName];
				if (this.TargetPerson.Owner != issuer)
				{
					issuer.EventReports.Add(week, string.Format("GRANT failed. Person {0} is not owned by faction.", this.TargetName));
					return false;
				}
				return true;
			}

			if (!ModuleStack.All.ContainsKey(this.TargetName))
			{
				issuer.EventReports.Add(week, string.Format("GRANT failed. Unknown modulestack {0}.", this.TargetName));
				return false;
			}
			this.TargetStack = ModuleStack.All[this.TargetName];
			if (this.TargetStack.Owner != issuer)
			{
				issuer.EventReports.Add(week, string.Format("GRANT failed. Modulestack {0} is not owned by faction.", this.TargetName));
				return false;
			}
			return true;
		}

		private bool checkEligibility(Faction issuer, int week)
		{
			switch (this.GrantKind)
			{
				case EGrantKind.technology:
					if (!this.factionKnowsTechnology(issuer, this.Technology))
					{
						issuer.EventReports.Add(
							week,
							string.Format("GRANT failed. Faction does not know {0} technology.", this.Technology.ReportName));
						return false;
					}
					if (this.TargetStack.ModuleType == null)
					{
						issuer.EventReports.Add(week, "GRANT failed. Target stack is not formed.");
						return false;
					}
					return true;
				case EGrantKind.skill:
					if (this.TargetPerson.Skills.Has(this.SkillType))
					{
						issuer.EventReports.Add(
							week,
							string.Format("GRANT failed. {0} already has {1}.", this.TargetPerson.ReportName, this.SkillType.ReportName));
						return false;
					}
					return true;
				case EGrantKind.item:
				case EGrantKind.module:
					if (this.TargetStack.ModuleType == null)
					{
						issuer.EventReports.Add(week, "GRANT failed. Target stack is not formed.");
						return false;
					}
					return true;
				default:
					return false;
			}
		}

		private bool deliver(Faction issuer, int week)
		{
			switch (this.GrantKind)
			{
				case EGrantKind.technology:
					return this.deliverTechnology(issuer, week);
				case EGrantKind.skill:
					return this.deliverSkill(issuer, week);
				case EGrantKind.item:
					return this.deliverItem(issuer, week);
				case EGrantKind.module:
					return this.deliverModule(issuer, week);
				default:
					return false;
			}
		}

		private bool deliverTechnology(Faction issuer, int week)
		{
			ModuleStack host = this.TargetStack.FindTechnologyCopyHost(this.Technology);
			if (host.ModuleType == null)
			{
				issuer.EventReports.Add(week, "GRANT failed. Target stack is not formed.");
				return false;
			}
			if (host.TechnologyCapacity < host.TechnologyCapacityUsed + this.Technology.Level)
			{
				issuer.EventReports.Add(week, "GRANT failed. Not enough technology capacity on target stack.");
				return false;
			}

			host.Technologies.Add(this.Technology);
			host.EventReports.Add(
				week,
				string.Format("received copy of {0} technology (GRANT).", this.Technology.ReportName));

			if (!issuer.TechnologiesSeen.Contains(this.Technology.Name)
				&& !issuer.TechnologiesToShow.Contains(this.Technology.Name))
			{
				issuer.TechnologiesToShow.Add(this.Technology);
			}

			issuer.EventReports.Add(
				week,
				string.Format("GRANT technology {0} to {1}.", this.Technology.Name, host.Name));
			return true;
		}

		private bool deliverSkill(Faction issuer, int week)
		{
			this.TargetPerson.Skills.Add(this.SkillType);
			this.TargetPerson.EventReports.Add(
				week,
				string.Format("received {0} skill (GRANT).", this.SkillType.ReportName));
			issuer.EventReports.Add(
				week,
				string.Format("GRANT skill {0} to {1}.", this.SkillType.Name, this.TargetPerson.Name));
			return true;
		}

		private bool deliverItem(Faction issuer, int week)
		{
			ItemStack grant = new ItemStack(this.ItemType, this.Quantity);
			if (this.TargetStack.Capacity - this.TargetStack.CapacityUsed < grant.Size)
			{
				issuer.EventReports.Add(
					week,
					string.Format(
						"GRANT failed. Not enough cargo capacity on {0} (need {1}, available {2}).",
						this.TargetStack.Name,
						grant.Size,
						this.TargetStack.Capacity - this.TargetStack.CapacityUsed));
				return false;
			}

			this.TargetStack.ItemStacks.Add(grant);
			this.TargetStack.EventReports.Add(
				week,
				string.Format("received {0} (GRANT).", grant.ReportName));
			issuer.EventReports.Add(
				week,
				string.Format("GRANT item {0} {1} to {2}.", this.Quantity, this.ItemType.Name, this.TargetStack.Name));
			return true;
		}

		private bool deliverModule(Faction issuer, int week)
		{
			if (this.TargetStack.ModuleType != null && this.TargetStack.ModuleType != this.ModuleType)
			{
				issuer.EventReports.Add(
					week,
					string.Format(
						"GRANT failed. Modulestack {0} holds {1}, not {2}.",
						this.TargetStack.Name,
						this.TargetStack.ModuleType.Name,
						this.ModuleType.Name));
				return false;
			}

			if (this.TargetStack.ModuleType == null)
			{
				this.TargetStack.ModuleType = this.ModuleType;
			}

			for (int i = 0; i < this.Quantity; i++)
			{
				this.TargetStack.AddModule();
			}

			this.TargetStack.EventReports.Add(
				week,
				string.Format(
					"received {0} {1} (GRANT).",
					this.Quantity,
					this.Quantity > 1 ? this.ModuleType.ReportNameMultiple : this.ModuleType.ReportName));
			issuer.EventReports.Add(
				week,
				string.Format("GRANT module {0} {1} to {2}.", this.Quantity, this.ModuleType.Name, this.TargetStack.Name));
			return true;
		}

		private bool factionKnowsTechnology(Faction faction, Technology technology)
		{
			if (technology == null || faction == null)
			{
				return false;
			}
			if (faction.TechnologiesSeen.Contains(technology.Name))
			{
				return true;
			}
			foreach (ModuleStack stack in ModuleStack.All.Values)
			{
				if (stack.Owner == faction && stack.HasTechnology(technology))
				{
					return true;
				}
			}
			return false;
		}

		private string debitTitle()
		{
			switch (this.GrantKind)
			{
				case EGrantKind.technology:
					return string.Format("GRANT technology {0} to {1}", this.Technology.Name, this.TargetName);
				case EGrantKind.skill:
					return string.Format("GRANT skill {0} to {1}", this.SkillType.Name, this.TargetName);
				case EGrantKind.item:
					return string.Format("GRANT item {0} {1} to {2}", this.Quantity, this.ItemType.Name, this.TargetName);
				case EGrantKind.module:
					return string.Format("GRANT module {0} {1} to {2}", this.Quantity, this.ModuleType.Name, this.TargetName);
				default:
					return "GRANT";
			}
		}

		public override void LoadXml(XmlElement elOrder)
		{
			XmlElement elGrant = (XmlElement)elOrder.SelectNodes("grant")[0];
			this.GrantKind = (EGrantKind)Enum.Parse(typeof(EGrantKind), elGrant.GetAttribute("kind"), true);
			this.TargetName = elGrant.GetAttribute("target");
			switch (this.GrantKind)
			{
				case EGrantKind.technology:
					this.Technology = Technology.All[elGrant.GetAttribute("technology")];
					break;
				case EGrantKind.skill:
					this.SkillType = SkillType.All[elGrant.GetAttribute("skill")];
					break;
				case EGrantKind.item:
					this.Quantity = this.XMLAssignInteger(elGrant.GetAttribute("quantity"), 1);
					this.ItemType = ItemType.All[elGrant.GetAttribute("item")];
					break;
				case EGrantKind.module:
					this.Quantity = this.XMLAssignInteger(elGrant.GetAttribute("quantity"), 1);
					this.ModuleType = ModuleType.All[elGrant.GetAttribute("module")];
					break;
			}
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			XmlElement elGrant = doc.CreateElement("grant");
			elGrant.SetAttribute("kind", this.GrantKind.ToString());
			elGrant.SetAttribute("target", this.TargetName);
			switch (this.GrantKind)
			{
				case EGrantKind.technology:
					elGrant.SetAttribute("technology", this.Technology.Name);
					break;
				case EGrantKind.skill:
					elGrant.SetAttribute("skill", this.SkillType.Name);
					break;
				case EGrantKind.item:
					elGrant.SetAttribute("quantity", this.Quantity.ToString());
					elGrant.SetAttribute("item", this.ItemType.Name);
					break;
				case EGrantKind.module:
					elGrant.SetAttribute("quantity", this.Quantity.ToString());
					elGrant.SetAttribute("module", this.ModuleType.Name);
					break;
			}
			this.xmlElement.AppendChild(elGrant);
			return this.xmlElement;
		}

		public override List<string> Report(Faction owner)
		{
			List<string> lines = new List<string>();
			string payload;
			switch (this.GrantKind)
			{
				case EGrantKind.technology:
					payload = string.Format("technology {0}", this.Technology != null ? this.Technology.Name : string.Empty);
					break;
				case EGrantKind.skill:
					payload = string.Format("skill {0}", this.SkillType != null ? this.SkillType.Name : string.Empty);
					break;
				case EGrantKind.item:
					payload = string.Format("item {0} {1}", this.Quantity, this.ItemType != null ? this.ItemType.Name : string.Empty);
					break;
				case EGrantKind.module:
					payload = string.Format("module {0} {1}", this.Quantity, this.ModuleType != null ? this.ModuleType.Name : string.Empty);
					break;
				default:
					payload = string.Empty;
					break;
			}

			string line = string.Format("{0}{1}grant {2} to {3}",
				this.Conditions,
				(this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")),
				payload,
				this.TargetName ?? string.Empty);
			lines.Add(line);
			return lines;
		}
	}
}
