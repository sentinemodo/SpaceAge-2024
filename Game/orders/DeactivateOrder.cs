using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public class DeactivateOrder : ModuleActivationOrder
	{
		public DeactivateOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.deactivate;
		}

		public override void Execute(int week)
		{
			this.Executed = false;
			if (!(this.Subject is ModuleStack))
			{
				throw new Exception("DEACTIVATE can only be issued by a modulestack.");
			}

			ModuleStack stack = (ModuleStack)this.Subject;
			int deactivated = stack.DeactivateModules(this.Quantity);
			if (deactivated > 0)
			{
				this.Executed = true;
			}
			base.Execute(week);
		}

		public override List<string> Report(Faction owner)
		{
			return this.reportLines("deactivate");
		}

		public override void LoadXml(XmlElement elOrder)
		{
			this.loadQuantityXml(elOrder, "deactivate");
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			return this.saveQuantityXml(doc, "deactivate");
		}
	}
}
