using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class DisableTactic : Tactic
	{
		public DisableTactic(ModuleStack subject)
			: base(subject)
		{
			this.name = "disable";
		}

		public override int Attack
		{
			get
			{
				return System.Convert.ToInt32(this.Subject.Attack / 2);
			}
		}

		public override Module ResolveHitLocation(ModuleStack target)
		{
			return null;
		}
        
        public override XmlElement SaveXml(XmlDocument doc)
        {
            this.xmlElement = doc.CreateElement("tactic");

            this.xmlElement.SetAttribute("name", "disable");

            return this.xmlElement;
        }
    }
}
