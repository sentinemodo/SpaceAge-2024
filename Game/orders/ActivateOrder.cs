using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public class ActivateOrder : ModuleActivationOrder
	{
		public ActivateOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.activate;
		}

		public override void Execute(int week)
		{
			this.Executed = false;
			if (!(this.Subject is ModuleStack))
			{
				throw new Exception("ACTIVATE can only be issued by a modulestack.");
			}

			ModuleStack stack = (ModuleStack)this.Subject;
			int activated = stack.ActivateModules(this.Quantity);
			if (activated > 0)
			{
				this.Executed = true;
			}
			base.Execute(week);
		}

		public override List<string> Report(Faction owner)
		{
			return this.reportLines("activate");
		}

		public override void LoadXml(XmlElement elOrder)
		{
			this.loadQuantityXml(elOrder, "activate");
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			return this.saveQuantityXml(doc, "activate");
		}
	}
}
