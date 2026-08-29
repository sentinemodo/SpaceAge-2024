using System;
using System.Xml;

namespace SpaceAge
{
	public class JumpOrder : LongOrder
	{
		public JumpOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.jump;
		}

		public ModuleStack Jumper
		{
			get { return (ModuleStack)this.Subject; }
		}

		private Planet destinationGate;
		public Planet DestinationGate
		{
			get { return this.destinationGate; }
			set { this.destinationGate = value; }
		}

		public override void Parse(string command)
		{
			string token = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(token) || !Planet.All.ContainsKey(token))
			{
				throw new Exception("Bad syntax or unknown JUMP destination. Received: " + token);
			}
			this.destinationGate = Planet.All[token];
		}

		private Location JumpArrival()
		{
			if (this.destinationGate == null)
			{
				return null;
			}
			foreach (Region region in this.destinationGate.Regions.Values)
			{
				if (region.RegionType != null && region.RegionType.LocationType == ELocationType.orbit)
				{
					return region;
				}
			}
			return this.destinationGate.Orbit;
		}

		private Planet GateAt(Location location)
		{
			Planet planet;
			Moon moon;
			if (!BodyEnvironment.TryGetBody(location, out planet, out moon))
			{
				return null;
			}
			return planet;
		}

		private bool canJump(int week)
		{
			if (this.Jumper == null || this.Jumper.ModuleType == null)
			{
				return false;
			}
			if (!this.Jumper.ModuleType.IsShipHullType)
			{
				this.Jumper.EventReports.Add(week, "JUMP failed. Only ships can jump.");
				return false;
			}
			Planet here = this.GateAt(this.Jumper.Location);
			if (here == null || string.IsNullOrEmpty(here.PairName))
			{
				this.Jumper.EventReports.Add(week, "JUMP failed. Unit is not at an Alderson Gate.");
				return false;
			}
			if (this.destinationGate == null || here.PairName != this.destinationGate.Name)
			{
				this.Jumper.EventReports.Add(week, "JUMP failed. Destination is not the paired Gate.");
				return false;
			}
			return true;
		}

		public override void Execute(int week)
		{
			base.Execute(week);
			if (!this.canJump(week))
			{
				this.Executed = true;
				return;
			}
			if (this.DurationLeft <= 0 || this.DurationLeft >= int.MaxValue)
			{
				this.DurationLeft = 1;
				this.Executing = true;
				this.Jumper.EventReports.Add(week,
					string.Format("jumping to {0}, ETA 1.", this.destinationGate.ReportName));
			}
			this.DurationLeft--;
			if (this.DurationLeft <= 0)
			{
				Location arrival = this.JumpArrival();
				this.Jumper.Parent = arrival;
				this.Jumper.MovingTo = null;
				this.Jumper.EventReports.Add(week,
					string.Format("arrived at {0} via JUMP.", arrival.ReportName));
				this.Executed = true;
				this.Executing = false;
			}
		}

		public override void LoadXml(XmlElement elOrder)
		{
			XmlElement elJump = (XmlElement)elOrder.SelectSingleNode("jump");
			if (elJump != null && Planet.All.ContainsKey(elJump.GetAttribute("destination")))
			{
				this.destinationGate = Planet.All[elJump.GetAttribute("destination")];
			}
			if (elOrder.HasAttribute("duration-left"))
			{
				this.DurationLeft = this.XMLAssignInteger(elOrder.GetAttribute("duration-left"), this.DurationLeft);
			}
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			if (this.DurationLeft > 0 && this.DurationLeft < int.MaxValue)
			{
				this.xmlElement.SetAttribute("duration-left", this.DurationLeft.ToString());
			}
			XmlElement elJump = doc.CreateElement("jump");
			if (this.destinationGate != null)
			{
				elJump.SetAttribute("destination", this.destinationGate.Name);
			}
			this.xmlElement.AppendChild(elJump);
			return this.xmlElement;
		}
	}
}
