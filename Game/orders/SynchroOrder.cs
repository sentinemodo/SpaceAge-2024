using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public class SynchroOrder : ImmediateOrder
	{
		public SynchroOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.synchro;
		}

		public string Tag { get; set; }

		public bool Armed { get; set; }

		public override void Parse(string command)
		{
			if (string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax, synchro tag expected.");
			}

			string tag = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(tag))
			{
				throw new Exception("Bad syntax, synchro tag expected.");
			}
			if (!string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax, unexpected text after synchro tag: " + command);
			}

			this.Tag = tag;
		}

		public override void LoadXml(XmlElement elOrder)
		{
			XmlElement elSynchro = (XmlElement)elOrder.SelectNodes("synchro")[0];
			this.Tag = elSynchro.GetAttribute("tag");
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			XmlElement elSynchro = doc.CreateElement("synchro");
			elSynchro.SetAttribute("tag", this.Tag);
			this.xmlElement.AppendChild(elSynchro);
			return this.xmlElement;
		}

		public override List<string> Report(Faction owner)
		{
			List<string> lines = new List<string>();
			string line = string.Format("{0}{1}synchro {2}",
				this.Conditions,
				(this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")),
				this.Tag);
			lines.Add(line);
			return lines;
		}

		public override void Execute(int week)
		{
			this.Executed = false;
			if (string.IsNullOrEmpty(this.Tag))
			{
				return;
			}

			this.Armed = true;
			List<SynchroOrder> group = Matching(this.Tag);
			if (!Ready(group))
			{
				return;
			}

			foreach (SynchroOrder peer in group)
			{
				peer.Commit(week);
			}
		}

		private void Commit(int week)
		{
			if (this.Executed)
			{
				return;
			}

			this.Executed = true;
			this.Armed = false;
			this.FailedToExecute = false;
			if (this.Repeat > 0)
			{
				this.Repeat--;
			}

			this.Subject.EventReports.Add(week, string.Format("synchronized {0}.", this.Tag));
			this.Subject.Orders.RemoveConditions(this);
		}

		private static bool Ready(List<SynchroOrder> group)
		{
			if (group.Count < 2)
			{
				return false;
			}

			foreach (SynchroOrder peer in group)
			{
				if (!peer.Armed)
				{
					return false;
				}
			}

			return true;
		}

		private static List<SynchroOrder> Matching(string tag)
		{
			List<SynchroOrder> group = new List<SynchroOrder>();
			foreach (Faction faction in Faction.All.Values)
			{
				Collect(group, tag, faction.Orders);
			}
			foreach (ModuleStack moduleStack in ModuleStack.All.Values)
			{
				Collect(group, tag, moduleStack.Orders);
			}
			foreach (Person person in Person.All.Values)
			{
				Collect(group, tag, person.Orders);
			}
			return group;
		}

		private static void Collect(List<SynchroOrder> group, string tag, Orders orders)
		{
			foreach (Order order in orders)
			{
				SynchroOrder synchro = order as SynchroOrder;
				if (synchro == null || synchro.Executed)
				{
					continue;
				}
				if (!string.Equals(synchro.Tag, tag, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				group.Add(synchro);
			}
		}
	}
}
