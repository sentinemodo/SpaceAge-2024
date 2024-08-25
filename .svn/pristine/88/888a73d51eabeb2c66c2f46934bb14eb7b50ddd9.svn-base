using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class AliasOrder : ImmediateOrder
	{
		public AliasOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.alias;
		}

		public AliasOrder(ModuleStack aliaser, string alias)
			: base(aliaser)
		{
			this.type = EOrderType.alias;
			this.Alias = alias;
		}

		public ModuleStack Aliaser
		{
			get { return (ModuleStack)this.Subject; }
		}

		public string Alias { get; set; }
		
		public override void Parse(string command)
		{
			// set alias of the modulestack
			// ALIAS "alias"
		
			if (string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax");
			}
			
			this.Alias = LineParser.GetQuotedToken(ref command);
		}

        public override void LoadXml(XmlElement elOrder)
        {
            XmlElement elAlias = (XmlElement)elOrder.SelectNodes("alias")[0];

            this.Alias = elAlias.GetAttribute("alias");
        }

        public override XmlElement SaveXml_core(XmlDocument doc, string subject)
        {
            XmlElement elAlias = doc.CreateElement("alias");

            elAlias.SetAttribute("alias", this.Alias);

            xmlElement.AppendChild(elAlias);
            return xmlElement;
        }

		public override void Execute(int week)
		{
			this.Executed = false;
			this.Aliaser.Alias = this.Alias;
			this.Executed = true;
			base.Execute(week);
		}
	}
}
