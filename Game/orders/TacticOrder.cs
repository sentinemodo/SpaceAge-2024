using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	// TACTIC destroy|capture|evade|prioritize armed|prioritize command|prioritize storage — modulestack only.
	// Destroy and capture are exclusive. Evade and prioritize may coexist with firing tactics.
	// Immobile stacks may only use destroy.
	public class TacticOrder : ImmediateOrder
	{
		public TacticOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.tactic;
		}

		public string TacticName { get; set; }

		public ModuleStack Unit
		{
			get { return (ModuleStack)this.Subject; }
		}

		public override void Parse(string command)
		{
			string token = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(token))
			{
				throw new Exception("Bad syntax, tactic name expected.");
			}
			this.TacticName = token.ToLowerInvariant();
			if (this.TacticName == "prioritize")
			{
				string kind = LineParser.GetToken(ref command).ToLowerInvariant();
				if (kind != "armed" && kind != "command" && kind != "storage")
				{
					throw new Exception("Unknown prioritize tactic. Received: " + kind);
				}
				this.TacticName = string.Concat("prioritize ", kind);
				return;
			}
			if (this.TacticName != "destroy" && this.TacticName != "capture" && this.TacticName != "evade")
			{
				throw new Exception("Unknown tactic. Received: " + token);
			}
		}

		public override void Execute(int week)
		{
			this.Executed = false;
			if (this.TacticName.StartsWith("prioritize "))
			{
				this.Unit.ApplyPrioritizeTactic(this.TacticName);
				this.Unit.EventReports.Add(week, string.Format("set tactic to {0}.", this.TacticName));
				this.Executed = true;
				base.Execute(week);
				return;
			}
			if (this.Unit.IsImmobile && this.TacticName != "destroy")
			{
				this.Unit.EventReports.Add(week, "TACTIC failed. Immobile units may only use destroy.");
				base.Execute(week);
				return;
			}

			this.Unit.ApplyTactic(this.TacticName);
			this.Unit.EventReports.Add(week, string.Format("set tactic to {0}.", this.TacticName));
			this.Executed = true;
			base.Execute(week);
		}

		public override void LoadXml(XmlElement elOrder)
		{
			XmlElement elTactic = (XmlElement)elOrder.SelectNodes("tactic")[0];
			this.TacticName = elTactic.GetAttribute("name");
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			XmlElement elTactic = doc.CreateElement("tactic");
			elTactic.SetAttribute("name", this.TacticName);
			this.xmlElement.AppendChild(elTactic);
			return this.xmlElement;
		}

		public override List<string> Report(Faction owner)
		{
			List<string> lines = new List<string>
            {
                string.Format("{0}{1}tactic {2}",
                    this.Conditions,
                    (this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")),
                    this.TacticName)
            };
			return lines;
		}
	}
}
