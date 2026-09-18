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

		private int holdQuantity = 0;
		public int HoldQuantity
		{
			get { return this.holdQuantity; }
			set { this.holdQuantity = value; }
		}

		private ItemType holdItemType = null;
		public ItemType HoldItemType
		{
			get { return this.holdItemType; }
			set { this.holdItemType = value; }
		}

		public override void Parse(string command)
		{
			// SET AVOID TRUE
			// SET HOLD 20 terran
			// SET HOLD 0 terran

			string token;
			if (string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax");
			}

			token = LineParser.GetToken(ref command);
			string flag = token.ToUpperInvariant();
			if (flag == "ALLOW")
			{
				token = LineParser.GetToken(ref command);
				if (token.ToUpperInvariant() != "BANK")
				{
					throw new Exception("unknown name of the flag " + token);
				}
				this.flagName = "ALLOW BANK";
			}
			else if (flag == "HOLD")
			{
				this.flagName = flag;
				token = LineParser.GetToken(ref command);
				this.holdQuantity = System.Convert.ToInt32(token);
				token = LineParser.GetToken(ref command);
				this.holdItemType = ItemType.All[token];
				return;
			}
			else if (flag == "AVOID" || flag == "ONLINE" || flag == "SHARING" || flag == "PATROL")
			{
				this.flagName = flag;
			}
			else
			{
				throw new Exception("unknown name of the flag " + token);
			}

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
                this.FlagValue = this.XMLAssignBoolean(elSet.GetAttribute("flag-value"), true);
            }
            if (elSet.HasAttribute("hold-quantity"))
            {
                this.HoldQuantity = this.XMLAssignInteger(elSet.GetAttribute("hold-quantity"), 0);
            }
            if (elSet.HasAttribute("hold-item-type"))
            {
                this.HoldItemType = ItemType.All[elSet.GetAttribute("hold-item-type")];
            }
        }

        public override XmlElement SaveXml_core(XmlDocument doc, string subject)
        {
            XmlElement elSet = doc.CreateElement("set");

            elSet.SetAttribute("flag-name", this.FlagName);
            if (this.FlagName.ToUpperInvariant() == "HOLD")
            {
                elSet.SetAttribute("hold-quantity", this.HoldQuantity.ToString());
                elSet.SetAttribute("hold-item-type", this.HoldItemType.Name);
            }
            else if (flagValue)
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
				case "ALLOW BANK":
					this.Setter.AllowBank = flagValue;
					this.Executed = true;
					break;
				case "SHARING":
					this.Setter.Sharing = flagValue;
					this.Executed = true;
					break;
				case "PATROL":
					this.Setter.IsPatrolling = flagValue;
					this.Executed = true;
					break;
				case "HOLD":
					this.Setter.SetItemHold(this.HoldItemType, this.HoldQuantity);
					this.Executed = true;
					break;
			}
			base.Execute(week);
		}
	}
}
