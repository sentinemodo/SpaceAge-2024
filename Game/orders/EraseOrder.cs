using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public class EraseOrder : ImmediateOrder
	{
		public EraseOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.erase;
		}

		public EraseOrder(ModuleStack stack, Technology technology)
			: base(stack)
		{
			this.type = EOrderType.erase;
			this.Technology = technology;
		}

		public ModuleStack Stack
		{
			get { return (ModuleStack)this.Subject; }
		}

		public Technology Technology { get; set; }

		public override void Parse(string command)
		{
			if (string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax");
			}

			string token = LineParser.GetToken(ref command);
			try
			{
				this.Technology = Technology.All[token];
			}
			catch (Exception ex)
			{
				throw new Exception("bad syntax or technology does not exist", ex);
			}
		}

		public override void LoadXml(XmlElement elOrder)
		{
			XmlElement elErase = (XmlElement)elOrder.SelectNodes("erase")[0];
			this.Technology = Technology.All[elErase.GetAttribute("technology")];
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			XmlElement elErase = doc.CreateElement("erase");
			elErase.SetAttribute("technology", this.Technology.Name);
			xmlElement.AppendChild(elErase);
			return xmlElement;
		}

		public override void Execute(int week)
		{
			this.Executed = false;

			if (this.Technology == null)
			{
				base.Execute(week);
				return;
			}

			if (!this.Stack.RemoveTechnologyCopy(this.Technology))
			{
				this.Stack.EventReports.Add(
					week,
					string.Format(
						"ERASE failed. Stack does not hold {0} technology.",
						this.Technology.ReportName));
			}
			else
			{
				this.Stack.EventReports.Add(
					week,
					string.Format("erased {0} technology.", this.Technology.ReportName));
				this.Executed = true;
			}

			base.Execute(week);
		}

		public override List<string> Report(Faction owner)
		{
			List<string> lines = new List<string>();
			string line = string.Format("{0}{1}erase {2}",
				this.Conditions,
				(this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")),
				this.Technology != null ? this.Technology.Name : string.Empty);
			lines.Add(line);
			return lines;
		}
	}
}
