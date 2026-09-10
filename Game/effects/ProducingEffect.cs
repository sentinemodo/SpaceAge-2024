using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class ProducingEffect : Producing
	{
		public override string Description
		{
			get
			{
				return string.Format("repairing damage, {0} weeks to complete.", this.Duration);
			}
		}

		public ProducingEffect(IEffectable producer)
			: base(producer, 0)
		{
		}

		public ProducingEffect(IEffectable producer, Technology technology, int duration)
			: base(producer, technology, duration)
		{
		}

		public override void Execute(int week)
		{
			if (this.ExecuteCondition)
			{
				this.Duration--;
				if (this.Duration == 0)
				{
					ModuleStack scope = RepairScope.For(this.Producer);
					int repaired = this.applyProduceEffect(scope);
					this.Producer.EventReports.Add(week, string.Format("repaired {0} damage.", repaired));
				}
				base.Execute(week);
			}
		}

		private int applyProduceEffect(ModuleStack scope)
		{
			if (this.Technology.UseProduceEffectName == "repair"
				&& this.Technology.UseProduceTarget == "module-damage"
				&& this.Technology.UseProduceChange < 0)
			{
				return RepairScope.ApplyRepair(scope, -this.Technology.UseProduceChange);
			}

			throw new Exception(string.Format(
				"Unknown produce effect {0} on {1}.",
				this.Technology.UseProduceEffectName,
				this.Technology.UseProduceTarget));
		}

        public override XmlElement SaveXml(XmlDocument doc)
        {
            base.SaveXml(doc);
            this.xmlElement.SetAttribute("type", "producing-effect");
            return this.xmlElement;
        }

	}
}
