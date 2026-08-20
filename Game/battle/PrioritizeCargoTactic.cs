using System.Xml;

namespace SpaceAge
{
	public class PrioritizeCargoTactic : Tactic
	{
		public PrioritizeCargoTactic(ModuleStack subject)
			: base(subject)
		{
			this.name = "prioritize storage";
		}

		public override XmlElement SaveXml(XmlDocument doc)
		{
			this.xmlElement = doc.CreateElement("tactic");
			this.xmlElement.SetAttribute("name", "prioritize storage");
			return this.xmlElement;
		}
	}
}
