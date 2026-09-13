using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public abstract class ModuleActivationOrder : ImmediateOrder
	{
		protected ModuleActivationOrder(IOrderable subject)
			: base(subject)
		{
			this.quantity = -1;
		}

		protected int quantity = -1;
		public int Quantity
		{
			get { return this.quantity; }
		}

		public override void Parse(string command)
		{
			if (!(this.Subject is ModuleStack))
			{
				throw new Exception("Module activation orders can only be issued by a modulestack.");
			}

			command = command == null ? string.Empty : command.Trim();
			if (command.Length == 0)
			{
				this.quantity = -1;
				return;
			}

			string token = LineParser.GetToken(ref command);
			if (token.Equals("all", StringComparison.OrdinalIgnoreCase))
			{
				this.quantity = -1;
			}
			else
			{
				try
				{
					this.quantity = Convert.ToInt32(token);
				}
				catch (Exception ex)
				{
					throw new Exception("bad syntax ALL or quantity expected", ex);
				}
				if (this.quantity < 1)
				{
					throw new Exception("bad syntax, positive amount expected");
				}
			}

			command = command.Trim();
			if (command.Length == 0)
			{
				return;
			}

			token = LineParser.GetToken(ref command);
			if (!token.Equals("modules", StringComparison.OrdinalIgnoreCase))
			{
				throw new Exception("bad syntax MODULES expected");
			}
		}

		protected List<string> reportLines(string verb)
		{
			string quantityText = (this.quantity < 0)
				? string.Empty
				: string.Concat(this.quantity.ToString(), " modules ");
			return new List<string>
			{
				string.Format("{0}{1}{2}",
					this.Conditions,
					(this.Repeat > 1) ? string.Concat(this.Repeat.ToString(), " ") : ((this.Repeat < 0) ? "@" : string.Empty),
					string.Concat(quantityText, verb))
			};
		}

		protected void loadQuantityXml(XmlElement elOrder, string elementName)
		{
			XmlElement elActivation = (XmlElement)elOrder.SelectNodes(elementName)[0];
			if (elActivation.GetAttribute("quantity") == "all")
			{
				this.quantity = -1;
			}
			else
			{
				this.quantity = this.XMLAssignInteger(elActivation.GetAttribute("quantity"), -1);
			}
		}

		protected XmlElement saveQuantityXml(XmlDocument doc, string elementName)
		{
			XmlElement elActivation = doc.CreateElement(elementName);
			elActivation.SetAttribute("quantity", (this.quantity < 0) ? "all" : this.quantity.ToString());
			this.xmlElement.AppendChild(elActivation);
			return this.xmlElement;
		}
	}
}
