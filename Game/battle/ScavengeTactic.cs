using System.Xml;

namespace SpaceAge
{
	public class ScavengeTactic : Tactic
	{
		public ScavengeTactic(ModuleStack subject)
			: base(subject)
		{
			this.name = "scavenge";
		}

		public override XmlElement SaveXml(XmlDocument doc)
		{
			this.xmlElement = doc.CreateElement("tactic");
			this.xmlElement.SetAttribute("name", "scavenge");
			return this.xmlElement;
		}
	}
}
