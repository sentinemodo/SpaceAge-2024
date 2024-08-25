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
				throw new Exception("Not implemented");
			}
		}

		public ProducingEffect(IEffectable producer, Technology technology, int duration)
			: base(producer, technology, duration)
		{
		}

		public override void Execute(int week)
		{
	    	throw new Exception("Not implemented");
		}

        public override XmlElement SaveXml(XmlDocument doc)
        {
            base.SaveXml(doc);
            this.xmlElement.SetAttribute("type", "producing-effect");
            return this.xmlElement;
        }

	}
}
