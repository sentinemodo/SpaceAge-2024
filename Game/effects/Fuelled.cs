using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class Fuelled : Effect
	{
        public Fuelled(IEffectable consumer)
            : base(consumer, 0)
        {
        }

        public Fuelled(IEffectable consumer, int duration)
			: base(consumer, duration)
		{
		}

		#region IReporting Members

		new public  List<string> Report(Faction faction)
		{
			List<string> lines = new List<string>();
			return lines;
		}

		#endregion

		public ModuleStack Consumer
		{
			get
			{
				return (ModuleStack)this.Subject;
			}
		}

		public void Use()
		{
			this.ExecuteCondition = true;
		}

		public override string Description
		{
			get
			{
				return string.Format("can operate for another {0} weeks without refueling.", this.Duration);
			}
		}

		public override void Execute(int week)
		{
			if (this.ExecuteCondition)
			{
				this.Duration--;
				if (this.Duration == 0)
				{
					this.Executed = true;
				}
				this.ExecuteCondition = false;
			}
		}

        public override XmlElement SaveXml(XmlDocument doc)
        {
            base.SaveXml(doc);
            this.xmlElement.SetAttribute("type", "fuelled");
            return this.xmlElement;
        }
	}
}
