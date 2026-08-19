using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class ProducingEnergy : Producing
	{
		public override string Description
		{
			get
			{
				return string.Format("producing {0} energy{1}.",
					this.Producer.EnergyProduction,
					(this.Duration>1)? string.Concat(" for ", this.Duration, " more weeks ") : string.Empty);
			}
		}

		public ProducingEnergy(IEffectable producer)
			: base(producer, 0)
		{
		}

		public ProducingEnergy(IEffectable producer, Technology technology, int duration)
			: base(producer, technology, duration)
		{
		}

		public ProducingEnergy(ProduceOrder produceOrder)
			: this(produceOrder.Producer, produceOrder.Technology, produceOrder.DurationLeft)
		{
			this.ProduceOrder = produceOrder;
		}

		public override void Execute(int week)
		{
			if (this.ExecuteCondition)
			{
				//if (this.Duration == this.ProduceOrder.DurationInitial)
				//{
				//	this.Producer.EventReports.Add(week, string.Format("consumed {0} to produce {1} energy.",
				//		this.Producer.ProduceEnergyConsume.ReportList,
				//		this.Producer.EnergyProduction));
				//}
				this.Duration--; 
				base.Execute(week);
			}
		}

        public override XmlElement SaveXml(XmlDocument doc)
        {
            base.SaveXml(doc);
            this.xmlElement.SetAttribute("type", "producing-energy");
            return this.xmlElement;
        }

	}
}
