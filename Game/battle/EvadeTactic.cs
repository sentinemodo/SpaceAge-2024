using System.Xml;

namespace SpaceAge
{
	public class EvadeTactic : Tactic
	{
		public EvadeTactic(ModuleStack subject)
			: base(subject)
		{
			this.name = "evade";
		}

		public override XmlElement SaveXml(XmlDocument doc)
		{
			this.xmlElement = doc.CreateElement("tactic");
			this.xmlElement.SetAttribute("name", "evade");
			return this.xmlElement;
		}
	}
}
