using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	// ATTACK <unit-id>: auto-declares the target unit an enemy (a one-way declaration is
	// enough to start a fight). The actual battle is resolved at the end of the week by the
	// combat trigger (added with the destroy/capture tactics).
	public class AttackOrder : ImmediateOrder
	{
		public AttackOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.attack;
		}

		public string TargetName { get; set; }

		// The declaring party is the owner of the ordering unit.
		public IHolder Attacker
		{
			get { return (IHolder)this.Subject; }
		}

		public override void Parse(string command)
		{
			string token = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(token))
			{
				throw new Exception("Bad syntax, target unit id expected.");
			}
			this.TargetName = token;
		}

		public override void Execute(int week)
		{
			this.Executed = false;
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
			this.TargetName = elAttack.GetAttribute("unit");
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			XmlElement elAttack = doc.CreateElement("attack");
			elAttack.SetAttribute("unit", this.TargetName);
			this.xmlElement.AppendChild(elAttack);
			return this.xmlElement;
		}

		public override List<string> Report(Faction owner)
		{
			List<string> lines = new List<string>
            {
                string.Format("{0}{1}attack {2}",
                    this.Conditions,
                    (this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")),
                    this.TargetName)
            };
			return lines;
		}
	}
}
