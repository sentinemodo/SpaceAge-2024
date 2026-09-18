using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	// ATTACK <unit-id>: auto-declares the target unit an enemy (a one-way declaration is
	// enough to start a fight). The actual battle is resolved at the end of the week by the
	// combat trigger (added with the destroy/capture tactics).
	// ATTACK REGION <region-id>: declare enemies at the region and move there if needed.
	public class AttackOrder : ImmediateOrder
	{
		public AttackOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.attack;
		}

		public string TargetName { get; set; }

		public Region TargetRegion { get; set; }

		public bool IsRegionTarget { get; set; }

		// The declaring party is the owner of the ordering unit.
		public ModuleStack Attacker
		{
			get { return (ModuleStack)this.Subject; }
		}

		public override void Parse(string command)
		{
			string token = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(token))
			{
				throw new Exception("Bad syntax, target unit id expected.");
			}

			if (string.Equals(token, "region", StringComparison.OrdinalIgnoreCase))
			{
				token = LineParser.GetToken(ref command);
				if (string.IsNullOrEmpty(token) || !Region.All.ContainsKey(token))
				{
					throw new Exception("Bad syntax, region id expected.");
				}
				this.IsRegionTarget = true;
				this.TargetRegion = Region.All[token];
				return;
			}

			this.TargetName = token;
		}

		public override void Execute(int week)
		{
			this.Executed = false;
			if (this.IsRegionTarget)
			{
				if (this.TargetRegion == null)
				{
					base.Execute(week);
					return;
				}

				PatrolGuard.DeclareRegionEntryEnemies(this.Attacker, this.TargetRegion);
				PatrolGuard.QueueMoveToRegion(this.Attacker, this.TargetRegion);
				this.Attacker.EventReports.Add(
					week,
					string.Format("attacking region {0}.", this.TargetRegion.ReportName));
				this.Executed = true;
				base.Execute(week);
				return;
			}

			if (!string.IsNullOrEmpty(this.TargetName))
			{
				this.Attacker.Owner.UnitAttitudes[this.TargetName] = FactionAttitude.Enemy;
				this.Attacker.EventReports.Add(
					week,
					string.Format("declared {0} an enemy and will attack.", this.TargetName));
				this.Executed = true;
			}
			base.Execute(week);
		}

		public override void LoadXml(XmlElement elOrder)
		{
			XmlElement elAttack = (XmlElement)elOrder.SelectNodes("attack")[0];
			if (elAttack.HasAttribute("region"))
			{
				string regionName = elAttack.GetAttribute("region");
				if (Region.All.ContainsKey(regionName))
				{
					this.IsRegionTarget = true;
					this.TargetRegion = Region.All[regionName];
					return;
				}
			}
			this.TargetName = elAttack.GetAttribute("unit");
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			XmlElement elAttack = doc.CreateElement("attack");
			if (this.IsRegionTarget && this.TargetRegion != null)
			{
				elAttack.SetAttribute("region", this.TargetRegion.Name);
			}
			else
			{
				elAttack.SetAttribute("unit", this.TargetName);
			}
			this.xmlElement.AppendChild(elAttack);
			return this.xmlElement;
		}

		public override List<string> Report(Faction owner)
		{
			string target = this.IsRegionTarget && this.TargetRegion != null
				? string.Concat("region ", this.TargetRegion.Name)
				: this.TargetName;
			List<string> lines = new List<string>
            {
                string.Format("{0}{1}attack {2}",
                    this.Conditions,
                    (this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")),
                    target)
            };
			return lines;
		}
	}
}
