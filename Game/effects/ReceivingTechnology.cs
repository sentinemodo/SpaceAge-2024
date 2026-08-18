using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class ReceivingTechnology : Receiving
	{
        public ModuleStack Transferer   { get; set; }
        public Technology Technology    { get; set; }

        public ReceivingTechnology(IEffectable receiver, ModuleStack transferer, Technology technology, int duration)
			: base(receiver, duration)
		{
			this.Technology = technology;
            this.Transferer = transferer;
		}

		#region IReporting Members

		new public  List<string> Report(Faction faction)
		{
			List<string> lines = new List<string>();
			return lines;
		}

		#endregion

		public override void Execute(int week)
		{
			string from = (this.Transferer != null)
				? string.Concat(" from ", this.Transferer.ReportName)
				: string.Empty;
			this.Receiver.ReceiveTechnologyCopy(
				this.Technology,
				week,
				string.Format("received copy of {0} technology{1}.",
					this.Technology.ReportName,
					from));

			this.Executed = true;
			base.Execute(week);
		}

        public override XmlElement SaveXml(XmlDocument doc)
        {
            base.SaveXml(doc);
            this.xmlElement.SetAttribute("type", "receiving-technology");
            this.xmlElement.SetAttribute("technology", this.Technology.Name);
            this.xmlElement.SetAttribute("transferrer", this.Transferer.Name);
            return this.xmlElement;
        }
	}
}
