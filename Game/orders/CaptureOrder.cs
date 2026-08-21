using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	// CAPTURE <unit-id>|all — persist capture tactic. A specific id is preferred as the
	// battle target. "all" means every Enemy unit at the location (no single preference).
	public class CaptureOrder : ImmediateOrder
	{
		public CaptureOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.capture;
		}

		public string TargetName { get; set; }

		public ModuleStack Unit
		{
			get { return (ModuleStack)this.Subject; }
		}

		public override void Parse(string command)
		{
			string token = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(token))
			{
				throw new Exception("Bad syntax, target unit id or ALL expected.");
			}
			this.TargetName = token;
		}

		public override void Execute(int week)
		{
			if (this.Unit.IsImmobile)
			{
				if (!this.Executed)
				{
					this.Unit.EventReports.Add(week, "CAPTURE failed. Immobile units may only use destroy.");
				}
				this.Executed = true;
				base.Execute(week);
				return;
			}

			this.Executed = false;

			this.Unit.ApplyTactic("capture");
			if (string.Equals(this.TargetName, "all", StringComparison.OrdinalIgnoreCase))
			{
				this.Unit.PreferredTargetName = "all";
				this.Unit.EventReports.Add(week, "set tactic to capture all enemy units.");
			}
			else
			{
				this.Unit.PreferredTargetName = this.TargetName;
				this.Unit.EventReports.Add(
					week,
					string.Format("set tactic to capture, preferring {0}.", this.TargetName));
			}

			if (ModuleStack.All.ContainsKey(this.TargetName))
			{
				this.Unit.Owner.UnitAttitudes[this.TargetName] = FactionAttitude.Enemy;
			}

			this.Executed = true;
			base.Execute(week);
		}

		public override void LoadXml(XmlElement elOrder)
		{
			XmlElement elCapture = (XmlElement)elOrder.SelectNodes("capture")[0];
			this.TargetName = elCapture.GetAttribute("unit");
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			XmlElement elCapture = doc.CreateElement("capture");
			elCapture.SetAttribute("unit", this.TargetName);
			this.xmlElement.AppendChild(elCapture);
			return this.xmlElement;
		}

		public override List<string> Report(Faction owner)
		{
			List<string> lines = new List<string>
            {
                string.Format("{0}{1}capture {2}",
                    this.Conditions,
                    (this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")),
                    this.TargetName)
            };
			return lines;
		}
	}
}
