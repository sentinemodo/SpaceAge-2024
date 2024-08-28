using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class ProducingModule : Producing
	{
		public override string Description
		{
			get
			{
				return string.Format("producing {0}{1}{2}, {3} weeks to complete{4}.",
					this.Technology.UseProduceModules.ReportName,
                    (this.ReceiverParent == this.Producer) ? string.Empty : string.Concat(" for ", this.ReceiverParent.ReportName),
                    string.Concat(" into ", this.Receiver.ReportName),
                    this.Duration,
                    (this.Producer.ModuleType.UseCondition_EfficiencyMultiplier == 1) ? string.Empty : string.Concat(
                        " (x", 
                        this.Producer.ModuleType.UseCondition_EfficiencyMultiplier, 
                        " efficiency multiplier)"));
			}
		}

        public ProducingModule(IEffectable producer)
            : base(producer, 0)
        {
        }


        public ProducingModule(
			IEffectable producer, 
			Technology technology, 
			int duration, 
			IHolder receiver, 
			IHolder receiverParent)
			: base(producer, technology, duration)
		{
			if (receiver == null)
			{
				this.Receiver = this.Producer.Parent;
			} else
			{
				this.Receiver = receiver;
			}
			if (receiverParent == null)
			{
				this.ReceiverParent = this.Producer;
			} else 
			{
				this.ReceiverParent = receiverParent;
			}
		}

		public ProducingModule(UseOrder useOrder)
			: this(useOrder.Producer, 
				useOrder.Technology, 
				useOrder.DurationLeft, 
				useOrder.Receiver, 
				useOrder.ReceiverParent)
		{
			this.UseOrder = useOrder;
		}

		public UseOrder UseOrder  { get; set; }

		public IHolder Receiver { get; set; }

        public IHolder ReceiverParent { get; set; }

		private ModuleStack produced = null;
		public ModuleStack Produced
		{
			get { return this.produced; }
		}

		public override void Execute(int week)
		{
			if (this.ExecuteCondition)
			{
				// production starting
				if (this.Duration == this.UseOrder.DurationInitial)
				{
                    if (this.Receiver is ModuleStack)
                    {
                        if (((ModuleStack)this.Receiver).ModuleType == null)
                        {
                            this.produced = (ModuleStack)this.Receiver;
                            this.produced.Parent = this.Producer.Parent;
                            this.produced.ModuleType = this.Technology.UseProduceModules;

                            this.produced.EventReports.Add(
                                week,
                                string.Format("formed by {0} with {1}.",
                                this.Producer.ReportName,
                                this.Technology.UseProduceModules.ReportName));
                        }
                        else
                        {
                            this.produced = (ModuleStack)this.Receiver;
                        }
                    }

                    this.Producer.EventReports.Add(week, string.Format("consumed {0} to produce {1} module.",
						this.Technology.UseConsumeItems.ReportList,
						this.Technology.UseProduceModules.ReportName));
				}

				// production
				this.Duration--; 

				// production complete
				if (this.Duration == 0)
				{
					// producing modules for something that exists already (ie. we want to add modules to the existing stack
                    this.produced.AddModule();
                    this.produced.ExecutedLongOrder = true;

                    this.Producer.EventReports.Add(
                        week,
                        string.Format("produced {0} into {1}.",
                        this.Technology.UseProduceModules.ReportName,
                        this.produced.ReportName));
                    this.produced.EventReports.Add(
                        week,
                        string.Format("received {0} produced by {1}.",
                            this.Technology.UseProduceModules.ReportName,
                            this.Producer.ReportName));

					// if there is a parent specified and it is not producer and the produced stack isn't already stacked under specified parent - stack under it
                    if (this.ReceiverParent != null 
                        & this.ReceiverParent != this.Producer 
                        & this.ReceiverParent != this.Produced.Parent)
                    {
                        StackOrder stackModule = new StackOrder(this.Produced, this.ReceiverParent);
                        stackModule.Execute(week);
                    }

					if (this.UseOrder != null)
					{
						this.UseOrder.Receiver = this.Receiver;
					}
				}
				base.Execute(week);
			}
		}

        public override void LoadXml(XmlElement elProducingModuleEffect)
        {
            base.LoadXml(elProducingModuleEffect);
            this.Receiver = ModuleStack.All.GetOrCreateNewModuleStack(this.Producer.Owner, elProducingModuleEffect.GetAttribute("receiver"));
            this.ReceiverParent = ModuleStack.All.GetOrCreateNewModuleStack(this.Producer.Owner, elProducingModuleEffect.GetAttribute("receiver-parent"));
        }

        public override XmlElement SaveXml(XmlDocument doc)
        {
            // there might be an issue with always visible items production in xml
            // there might be an issue with no linked order
            base.SaveXml(doc);
            this.xmlElement.SetAttribute("type", "producing-modules");
            this.xmlElement.SetAttribute("module", this.Technology.UseProduceModules.Name);
            this.xmlElement.SetAttribute("receiver-parent", this.ReceiverParent.Name);
            this.xmlElement.SetAttribute("receiver", this.Receiver.Name);

            return this.xmlElement;
        }
	}
}
