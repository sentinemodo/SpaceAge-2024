using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class EventReports : List<EventReport>, IReporting
	{
		public void Add(string description)
		{
			this.Add(new EventReport(0, description));
		}

		public void Add(int week, string description)
		{
			this.Add(new EventReport(week, description));
		}

		#region IReporting Members

		public static bool IsParsingWarning(EventReport eventReport)
		{
			return eventReport.Description != null
				&& (eventReport.Description.StartsWith("PARSING:")
					|| eventReport.Description.StartsWith("WARNING:"));
		}

		private bool anyVisible(Faction faction, bool excludeParsingWarnings)
		{
			foreach (EventReport eventReport in this)
			{
				if (excludeParsingWarnings && IsParsingWarning(eventReport))
				{
					continue;
				}
				if (eventReport.Visible(faction))
				{
					return true;
				}
			}
			return false;
		}

		public List<string> Report(Faction faction)
		{
			return this.Report(faction, 0);
		}

		public List<string> Report(Faction faction, int level)
		{
			return this.Report(faction, level, "events:", false);
		}

		public List<string> Report(
			Faction faction,
			int level,
			string sectionTitle,
			bool excludeParsingWarnings)
		{
			ReportLines reportLines = new ReportLines();
			if (this.anyVisible(faction, excludeParsingWarnings))
			{
				reportLines.Add(sectionTitle, level);

				foreach (EventReport eventReport in this)
				{
					if (excludeParsingWarnings && IsParsingWarning(eventReport))
					{
						continue;
					}
					if (eventReport.Visible(faction))
					{
						reportLines.Add(eventReport.Report(faction, level + 1));
					}
				}
			}
			return reportLines.IndentedLines;
		}

		#endregion

        public void LoadXml(XmlElement elHolder, IEventReporting holder)
        {
            EventReport eventReport;

            foreach (XmlElement elEvent in elHolder.SelectNodes("event"))
            {
                eventReport = new EventReport();
                eventReport.LoadXml(elEvent);
                holder.EventReports.Add(eventReport);
            }
        }

        public XmlElement SaveXml(XmlDocument doc, XmlElement elHolder, Faction faction = null)
        {
            XmlElement elEvent;
            foreach (EventReport eventReport in this)
            {
                if (!eventReport.Visible(faction))
                    continue;

                elEvent = eventReport.SaveXml(doc);
                elHolder.AppendChild(elEvent);
            }
    
            return elHolder;
        }
    }
}
