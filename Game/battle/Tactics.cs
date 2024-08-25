using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class Tactics : List<Tactic>
	{
		public string ReportList
		{
			get
			{
				string line = "";
				bool firstAdded = false;
				foreach (Tactic tactic in this)
				{
					line = string.Format("{0}{1}", line, (firstAdded) ? ", " : "");
					firstAdded = true;

					line = string.Format("{0}{1}", line, tactic.ReportName);
				}
				return line;
			}
		}

        public void LoadXml(XmlElement elHolder, ModuleStack holder)
        {
            Tactic tactic;

            foreach (XmlElement elTactic in elHolder.SelectNodes("tactic"))
            {
                switch (elTactic.GetAttribute("name"))
                {
                    case "disable":
                        tactic = new DisableTactic(holder);
                        break;
                    default:
                        throw new Exception("Unknown tactics. Received: " + elTactic.GetAttribute("name"));
                }
                holder.Tactics.Add(tactic);
            }
        }

        public XmlElement SaveXml(XmlDocument doc, XmlElement elHolder)
        {
            XmlElement elTactic;

            foreach (Tactic tactic in this)
            {
                elTactic = tactic.SaveXml(doc);
                elHolder.AppendChild(elTactic);
            }

            return elHolder;
        }

    }
	
}
