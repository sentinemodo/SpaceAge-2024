using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public abstract class Effect : XMLProcessing, IReporting, IProducable
	{
		virtual public string Description
		{
			get { throw new Exception("Not implemented"); }
		}

		public bool Permanent {get; set; }
		public int  Duration { get; set; }
		
        public bool ExecuteCondition { get; set; }
		public bool Executed { get; set; }
		abstract public void Execute(int week);

		public IEffectable Subject { get; set; }

		public Effect(IEffectable subject, int duration)
		{
			this.Subject = subject;
			this.Subject.Effects.Add(this);
			this.Duration = duration;
            this.ExecuteCondition = true;
            this.Executed = false;
		}

		#region IReporting Members

		public List<string> Report(Faction faction)
		{
			return this.Report(faction, 0);
		}

		public List<string> Report(Faction faction, int level)
		{
			ReportLine reportLine = new ReportLine(this.Description, level);
			return reportLine.IndentedLines;
		}

		public bool Visible(Faction faction)
		{
			return true;
		}

		#endregion

        public override void LoadXml(XmlElement elEffect)
        {
            if (elEffect.GetAttribute("duration") == "permanent")
            {
                this.Permanent = true;
            }
            else
            {
                this.Duration = this.XMLAssignInteger(elEffect.GetAttribute("duration"), 1);
            }

        }
        
        public override XmlElement SaveXml(XmlDocument doc)
        {
            this.xmlElement = doc.CreateElement("effect");
            this.xmlElement.SetAttribute("duration", (this.Permanent) ? "permanent" : this.Duration.ToString());
            return this.xmlElement;
        }
    }
}
