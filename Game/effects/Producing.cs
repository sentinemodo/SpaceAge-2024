using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public abstract class Producing : Effect
	{
		public ModuleStack Producer
		{
			get
			{
				return (ModuleStack)this.Subject;
			}
		}

		public ProduceOrder ProduceOrder { get; set; }
		public Technology Technology { get; set; }

		public Producing(IEffectable producer, Technology technology, int duration)
			: base(producer, duration)
		{
			this.Technology = technology;
			this.ExecuteCondition = true;
		}

		public Producing(IEffectable producer, int duration)
			: base(producer, duration)
		{
			this.ExecuteCondition = true;
		}

		public override void Execute(int week)
		{
			this.ExecuteCondition = false;				
			if (this.Duration == 0)
			{
				this.Executed = true;
			}
		}

		public void Use()
		{
			this.ExecuteCondition = true;
		}

        public override void LoadXml(XmlElement elProducingEffect)
        {
            base.LoadXml(elProducingEffect);
            if (elProducingEffect.HasAttribute("technology") && elProducingEffect.GetAttribute("technology") != string.Empty)
            {
                this.Technology = Technology.All[elProducingEffect.GetAttribute("technology")];
            }
            this.ExecuteCondition = false;
        }
    
        public override XmlElement SaveXml(XmlDocument doc)
        {
            // there probably will be a probalem with no linked produce order upon load xml
            // if that would be the case an unique order identifier will be necessary
            base.SaveXml(doc);
            if (this.Technology != null)
            {
                this.xmlElement.SetAttribute("technology", this.Technology.Name);
            }
            return this.xmlElement;
        }
	}
}
