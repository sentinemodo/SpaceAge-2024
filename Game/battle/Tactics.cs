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

		public bool ContainsName(string tacticName)
		{
			foreach (Tactic tactic in this)
			{
				if (tactic.ReportName == tacticName)
				{
					return true;
				}
			}
			return false;
		}

		public void RemoveByName(string tacticName)
		{
			for (int i = this.Count - 1; i >= 0; i--)
			{
				if (this[i].ReportName == tacticName)
				{
					this.RemoveAt(i);
				}
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
                    case "destroy":
                        tactic = new DestroyTactic(holder);
                        break;
                    case "capture":
                        tactic = new CaptureTactic(holder);
                        break;
                    case "evade":
                        tactic = new EvadeTactic(holder);
                        break;
                    case "prioritize armed":
                        tactic = new PrioritizeArmedTactic(holder);
                        break;
                    case "prioritize command":
                        tactic = new PrioritizeCommandTactic(holder);
                        break;
                    case "prioritize cargo":
                    case "prioritize storage":
                        tactic = new PrioritizeCargoTactic(holder);
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
