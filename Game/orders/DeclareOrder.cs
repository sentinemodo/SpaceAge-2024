using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	// DECLARE FACTION <id> <attitude>
	// DECLARE UNIT <id> <attitude>
	// DECLARE DEFAULT <attitude>
	// DECLARE UNKNOWN <attitude>
	// One-way stance on the ordering party's owner. Enemy is the combat stance
	// (same effect as ATTACK, but faction-wide when targeting a faction).
	public class DeclareOrder : ImmediateOrder
	{
		public const string TargetFaction = "faction";
		public const string TargetUnit = "unit";
		public const string TargetDefault = "default";
		public const string TargetUnknown = "unknown";

		public string TargetKind { get; set; }
		public string TargetName { get; set; }
		public FactionAttitude Attitude { get; set; }

		public DeclareOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.declare;
		}

		public override void Parse(string command)
		{
			string token = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(token))
			{
				throw new Exception("Bad syntax, FACTION, UNIT, DEFAULT or UNKNOWN expected.");
			}

			this.TargetKind = token.ToLowerInvariant();
			switch (this.TargetKind)
			{
				case TargetFaction:
				case TargetUnit:
					this.TargetName = LineParser.GetToken(ref command);
					if (string.IsNullOrEmpty(this.TargetName))
					{
						throw new Exception("Bad syntax, id expected.");
					}
					this.Attitude = FactionAttitudeParser.Parse(LineParser.GetToken(ref command));
					break;
				case TargetDefault:
				case TargetUnknown:
					this.Attitude = FactionAttitudeParser.Parse(LineParser.GetToken(ref command));
					break;
				default:
					throw new Exception("Bad syntax, FACTION, UNIT, DEFAULT or UNKNOWN expected. Received: " + token);
			}
		}

		public override void Execute(int week)
		{
			this.Executed = false;
			Faction owner = this.Subject.Owner;
			if (owner == null)
			{
				base.Execute(week);
				return;
			}

			switch (this.TargetKind)
			{
				case TargetFaction:
					owner.Attitudes[this.TargetName] = this.Attitude;
					this.Subject.EventReports.Add(
						week,
						string.Format("declared {0} toward faction {1}.",
							FactionAttitudeParser.ToToken(this.Attitude),
							this.TargetName));
					break;
				case TargetUnit:
					owner.UnitAttitudes[this.TargetName] = this.Attitude;
					this.Subject.EventReports.Add(
						week,
						string.Format("declared {0} toward unit {1}.",
							FactionAttitudeParser.ToToken(this.Attitude),
							this.TargetName));
					break;
				case TargetDefault:
					owner.DefaultAttitude = this.Attitude;
					this.Subject.EventReports.Add(
						week,
						string.Format("declared default stance {0}.",
							FactionAttitudeParser.ToToken(this.Attitude)));
					break;
				case TargetUnknown:
					owner.UnknownAttitude = this.Attitude;
					this.Subject.EventReports.Add(
						week,
						string.Format("declared unknown stance {0}.",
							FactionAttitudeParser.ToToken(this.Attitude)));
					break;
				default:
					base.Execute(week);
					return;
			}

			this.Executed = true;
			base.Execute(week);
		}

		public override void LoadXml(XmlElement elOrder)
		{
			XmlElement elDeclare = (XmlElement)elOrder.SelectNodes("declare")[0];
			this.TargetKind = elDeclare.GetAttribute("target");
			if (elDeclare.HasAttribute("name"))
			{
				this.TargetName = elDeclare.GetAttribute("name");
			}
			this.Attitude = FactionAttitudeParser.Parse(elDeclare.GetAttribute("attitude"));
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			XmlElement elDeclare = doc.CreateElement("declare");
			elDeclare.SetAttribute("target", this.TargetKind);
			if (!string.IsNullOrEmpty(this.TargetName))
			{
				elDeclare.SetAttribute("name", this.TargetName);
			}
			elDeclare.SetAttribute("attitude", FactionAttitudeParser.ToToken(this.Attitude));
			this.xmlElement.AppendChild(elDeclare);
			return this.xmlElement;
		}

		public override List<string> Report(Faction owner)
		{
			string prefix = string.Format("{0}{1}",
				this.Conditions,
				(this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")));
			List<string> lines = new List<string>();
			if (this.TargetKind == TargetDefault || this.TargetKind == TargetUnknown)
			{
				lines.Add(string.Format("{0}declare {1} {2}",
					prefix,
					this.TargetKind,
					FactionAttitudeParser.ToToken(this.Attitude)));
			}
			else
			{
				lines.Add(string.Format("{0}declare {1} {2} {3}",
					prefix,
					this.TargetKind,
					this.TargetName,
					FactionAttitudeParser.ToToken(this.Attitude)));
			}
			return lines;
		}
	}
}
