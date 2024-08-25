using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class ActiveOrder : ImmediateOrder
	{
		public ActiveOrder (IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.active;
		}

		public ActiveOrder(IOrderable observer, ModuleStack observed)
			: base(observer)
		{
			this.type = EOrderType.active;
			this.observed = observed;
		}

		public IOrderable Observer
		{
			get { return (IOrderable)this.Subject; }
		}

		private ModuleStack observed = null;
		public ModuleStack Observed
		{
			get { return this.observed; }
			set { this.observed = value; }
		}

		public override void Parse(string command)
		{
			//// check if active
			//testcommands.Add("active 000006");
			//testcommands.Add("active new1");
			string token = LineParser.GetQuotedToken(ref command);
			if (string.IsNullOrEmpty(token))
			{
				throw new Exception("Bad syntax modulestack name or alias expected.");
			}
			else 
			{
				this.observed = ModuleStack.All.GetOrCreateNewModuleStack(this.Observer.Owner, token);

				if (this.observed == null) 
				{
					throw new Exception("No such modulestack. Received: " + token);
				}
			}				
		}

		public override List<string> Report(Faction owner)
		{
			List<string> lines = new List<string>
            {
                string.Format("{0}{1}active {2}",
                this.Conditions,
                (this.Repeat > 1) ? string.Concat(this.Repeat.ToString(), " ") : ((this.Repeat < 0) ? "@" : string.Empty),
                string.Concat(((ModuleStack)this.observed).IsFormed ? string.Empty : "new", this.observed.Name))
            };
			return lines;
		}

        public override void LoadXml(XmlElement elOrder)
        {
            XmlElement elActive = (XmlElement)elOrder.SelectNodes("active")[0];
            this.Observed = ModuleStack.All.GetOrCreateNewModuleStack(this.Observer.Owner, elActive.GetAttribute("observed"));
        }

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			XmlElement elActive = doc.CreateElement("active");
			elActive.SetAttribute("observed", this.Observed.Name); 
			xmlElement.AppendChild(elActive);			
			return this.xmlElement;
		}


		public override void Execute(int week)
		{
			this.Executed = false;
			if (this.observed.IsFormed & this.observed.IsActive)
			{
				this.Executed = true;			
			}
			base.Execute(week);
		}
	}
}
