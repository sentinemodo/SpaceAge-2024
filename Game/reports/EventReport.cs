using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class EventReport : XMLProcessing, IReporting
	{
        public EventReport()
        {
        }

        public EventReport(int week, string description)
		{
			this.Week = week;
			this.Description = description;
		}

		public string Description { get; set; }

		public int Week { get; set; }

		#region IReporting Members

		public List<string> Report(Faction faction)
		{
			return this.Report(faction, 0);
		}

		public List<string> Report(Faction faction, int level)
		{
			ReportLine reportLine;
			if (this.Week > 0)
			{
				reportLine = new ReportLine(string.Format("week {0}: {1}", this.Week, this.Description), level);
			}
			else
			{
				reportLine = new ReportLine(this.Description, level);
			}
			return reportLine.IndentedLines;
		}

		public bool Visible(Faction faction)
		{
			return true;
		}
		#endregion

        public override void LoadXml(XmlElement elEvent)
        {
            this.Week = this.XMLAssignInteger(elEvent.GetAttribute("week"), 1);
            this.Description = elEvent.GetAttribute("description");
        }

        public override XmlElement SaveXml(XmlDocument doc)
        {
            this.xmlElement = doc.CreateElement("event");
            this.xmlElement.SetAttribute("week", this.Week.ToString());
            this.xmlElement.SetAttribute("description", this.Description);
            return this.xmlElement;
        }
    }
}
