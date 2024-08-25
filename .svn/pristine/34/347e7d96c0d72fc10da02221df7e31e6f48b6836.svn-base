using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class FormOrder : ImmediateOrder
	{
		public FormOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.form;
		}

		public ModuleStack Former
		{
			get { return (ModuleStack)this.Subject; }
		}

		private int quantity = 0;
		public int Quantity
		{
			get { return this.quantity; }
			set { this.quantity = value; }
		}
		
		private string alias = string.Empty;
		public string Alias
		{
			get { return this.alias; }
			set { this.alias = value; }
		}

		private ModuleStack formed = null;
		public ModuleStack Formed
		{
			get { return this.formed; }
			set { this.formed = value; }
		}

		public override void Parse(string command)
		{
			// form new, empty modulestack
			// FORM NEW
			// form new modulestack using a number of existing modules
			// FORM NEW with 2
			// same as above and assign an alias
			// FORM NEW AS "alias"
			// FORM NEW	WITH 2 AS "alias"

			string token;
			
			token = LineParser.GetToken(ref command);
			if (token != "new")
			{
				throw new Exception("bad syntax: NEW expected");
			}

			token = LineParser.GetToken(ref command);			
			if (token != string.Empty & token != "with" & token != "as")
			{
				throw new Exception("bad syntax: WITH or AS expected");				
			} else {
				if (token == "with")
				{
					try
					{
						token = LineParser.GetToken(ref command);
						this.quantity = Convert.ToInt32(token);
					}
					catch (Exception ex)
					{
						throw new Exception("bad syntax: a NUMBER of modules expected", ex);
					}
					
					token = LineParser.GetToken(ref command);
					if (token != string.Empty & token != "as")
					{
						throw new Exception("Bad syntax an AS expected");
					}
					if (token != string.Empty)
					{
						this.alias = LineParser.GetQuotedToken(ref command);
					}
				}
				else if (token == "as")
				{
					this.alias = LineParser.GetQuotedToken(ref command);
				}
				if (this.alias != string.Empty)
				{
					this.formed = ModuleStack.All.GetOrCreateNewModuleStack(this.Former.Owner, this.alias);
				}
			}
		}

        public override List<string> Report(Faction owner)
        {
            List<string> lines = new List<string>();
			string line;
			line = string.Format("{0}{1}form new{2}{3}",
					this.Conditions,
					(this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")),
					(this.quantity == 0) ? string.Empty : string.Concat(" with ", this.quantity.ToString()),
					(this.alias == string.Empty) ? string.Empty : string.Concat(" as new", this.formed.Name));
            lines.Add(line);
            return lines;
		}

        public override void LoadXml(XmlElement elOrder)
        {
            XmlElement elForm = (XmlElement)elOrder.SelectNodes("form")[0];
            if (elForm.HasAttribute("quantity"))
            {
                this.quantity = this.XMLAssignInteger(elForm.GetAttribute("quantity"), 0);
            }
            if (elForm.HasAttribute("alias"))
            {
                this.alias = this.XMLAssignString(elForm.GetAttribute("alias"), string.Empty);
            }
        }

        public override XmlElement SaveXml_core(XmlDocument doc, string subject)
        {
            XmlElement elForm = doc.CreateElement("form");
            if (this.quantity != 0)
            {
                elForm.SetAttribute("quantity", this.quantity.ToString());
            }
            if (this.alias != string.Empty)
            {
                elForm.SetAttribute("alias", this.alias);
            }
            xmlElement.AppendChild(elForm);
            return xmlElement;
        }

		public override void Execute(int week)
		{
			this.Executed = false;
			if (this.quantity == 0 || this.Former.Quantity >= this.quantity)
			{
                this.Formed.Parent = this.Former.Parent;

				this.Former.EventReports.Add(week, string.Format("formed new modulestack: {0}.", this.Formed.ReportName));
				this.Formed.EventReports.Add(week, string.Format("formed by {0}.", this.Former.ReportName));			
				if (this.quantity > 0)
				{
                    TransferOrder transferModules = new TransferOrder(
                        this.Former,
                        this.Formed,
                        this.Former.ModuleType,
                        this.quantity,
                        0);
                    transferModules.Execute(week);
				}

				if (this.alias != string.Empty)
				{
					AliasOrder aliasStack = new AliasOrder(this.formed, this.alias);
					aliasStack.Execute(week);
				}
			} else if (this.Former.Quantity < this.quantity)
			{
				this.Former.EventReports.Add(week, "FORM failed. Not enough modules in stack.");			
			}
			this.Executed = true;
			base.Execute(week);
		}
	}
}
