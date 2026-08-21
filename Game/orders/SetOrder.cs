using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class SetOrder : ImmediateOrder
	{
		public SetOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.set;
		}

		public SetOrder(ModuleStack setter, string flagName, bool flagValue)
			: base(setter)
		{
			this.type = EOrderType.set;
			this.flagName = flagName;
			this.flagValue = flagValue;
		}

		public ModuleStack Setter
		{
			get { return (ModuleStack)this.Subject; }
		}

		private string flagName = null;
		public string FlagName
		{
			get { return this.flagName; }
			set { this.flagName = value; }
		}
		
		private bool flagValue = false;
		public bool FlagValue
		{
			get { return this.flagValue; }
			set { this.flagValue = value; }
		}

		public override void Parse(string command)
		{
			// transfer a number of existing modules into receiver modulestack
			// SET AVOID TRUE
			// SET AVOID FALSE

			string token;
			if (string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax");
			}

			token = LineParser.GetToken(ref command);
			this.flagName = token;
			string flag = this.flagName.ToUpperInvariant();
			if (flag != "AVOID" && flag != "ONLINE")
			{
				throw new Exception("unknown name of the flag " + flagName);
			}
			this.flagName = flag;

			token = LineParser.GetToken(ref command);
			string value = token.ToUpperInvariant();
			if (value != "TRUE" && value != "FALSE")
			{
				throw new Exception("bad syntax TRUE or FALSE expected");
			}
			else 
			{
				this.flagValue = (value == "TRUE") ? true : false;
			}
		}

        public override void LoadXml(XmlElement elOrder)
        {
            XmlElement elSet = (XmlElement)elOrder.SelectNodes("set")[0];
            this.FlagName = elSet.GetAttribute("flag-name");
            if (elSet.HasAttribute("flag-value"))
            {
                this.FlagValue = this.XMLAssignBoolean(elSet.GetAttribute("flagValue"), true);
            }           
        }

        public override XmlElement SaveXml_core(XmlDocument doc, string subject)
        {
            XmlElement elSet = doc.CreateElement("set");

            elSet.SetAttribute("flag-name", this.FlagName);
            if (flagValue)
            {
                elSet.SetAttribute("flag-value", this.FlagValue.ToString());
            }
            xmlElement.AppendChild(elSet);
            return xmlElement;
        }

		public override void Execute(int week)
		{
			this.Executed = false;
			switch (this.flagName.ToUpperInvariant())
			{
				case "AVOID": 
					this.Setter.IsAvoiding = flagValue;
					this.Executed = true;
					break;
				case "ONLINE":
					this.Setter.SetOnline(flagValue);
					this.Executed = true;
					break;
			}
			base.Execute(week);
		}
	}
}
