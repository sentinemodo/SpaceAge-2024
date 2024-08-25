using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class LightlyDamaged : Effect
	{
		public LightlyDamaged(IEffectable damaged)
			: base(damaged, 0)
		{
			this.ExecuteCondition = false;
		}

		#region IReporting Members

		new public  List<string> Report(Faction faction)
		{
			List<string> lines = new List<string>
            {
                this.Description
            };
			return lines;
		}

		#endregion

		public Module Damaged
		{
			get
			{
				return (Module)this.Subject;
			}
		}

		public override string Description
		{
			get
			{
				return string.Format("is lightly damaged.");
			}
		}

		public override void Execute(int week)
		{
		}

        public override XmlElement SaveXml(XmlDocument doc)
        {
            base.SaveXml(doc);
            this.xmlElement.SetAttribute("type", "lightly-damaged");
            return this.xmlElement;
        }
	}
}
