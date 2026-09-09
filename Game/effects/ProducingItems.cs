using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class ProducingItems : Producing
	{
		public ItemStacks ProducedItemStacks { get; set; }

		public bool hasCapacity
		{
			get
			{
				return true;
			}
		}

		public override string Description
		{
			get
			{
				return string.Format("producing {0}, {1} weeks to complete{2}.",
					this.Technology.UseProduceItems.ReportList,
					this.Duration,
					(this.Producer.ModuleType.UseCondition_EfficiencyMultiplier == 1) ? string.Empty : string.Concat(
						" (",
						this.Producer.ModuleType.UseCondition_EfficiencyMultiplier,
						" efficiency multiplier)"));
			}
		}

		public ProducingItems(IEffectable producer)
			: base(producer, 0)
		{
			this.ProducedItemStacks = new ItemStacks();
		}

		public ProducingItems(IEffectable producer, Technology technology, int duration)
			: base(producer, technology, duration)
		{
			ModuleStack Producer = (ModuleStack)producer;

			this.ProducedItemStacks = new ItemStacks();
            this.ProducedItemStacks.Sum(technology.UseProduceItems);
            this.ProducedItemStacks.Multiply(Producer.QuantityOperational);
		}

		public ProducingItems(ProduceOrder produceOrder)
			: base(produceOrder.Producer, produceOrder.DurationLeft)
		{
			this.ProduceOrder = produceOrder;
            this.ProducedItemStacks = produceOrder.ItemStacks;
		}


		public override void Execute(int week)
		{
			if (this.ExecuteCondition)
			{
				if (this.hasCapacity) 
				{
                    this.Duration--;
                    if (this.Duration == 0) 
					{
						ModuleStack producer = (ModuleStack)this.Producer;
						ItemStacks output = this.ProducedItemStacks;
						SkillEffects.ApplyOutputPercent(output, SkillEffects.ItemOutputPercent(producer));
						this.Producer.ItemStacks.Sum(output);
                        this.Producer.EventReports.Add(week, string.Format("produced {0}.",
							this.ProducedItemStacks.ReportList));							
					}
				} else 
				{
					this.Producer.EventReports.Add(week, "production halted, not enough capacity.");		
				}
				base.Execute(week);
			}
		}

        public override void LoadXml(XmlElement elProducingItemsEffect)
        {
            base.LoadXml(elProducingItemsEffect);
            this.ProducedItemStacks = new ItemStacks();
            this.ProducedItemStacks.LoadXml(elProducingItemsEffect, this.Producer, "in-production");
        }

        public override XmlElement SaveXml(XmlDocument doc)
        {
            // there might be an issue with always visible items production in xml
            base.SaveXml(doc);
            this.xmlElement.SetAttribute("type", "producing-items");
            this.ProducedItemStacks.SaveXml(doc, this.xmlElement, this.Producer.Owner, "in-production");
            return this.xmlElement;
        }

	}
}
