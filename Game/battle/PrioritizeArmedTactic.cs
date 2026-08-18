using System.Xml;

namespace SpaceAge
{
	public class PrioritizeArmedTactic : Tactic
	{
		public PrioritizeArmedTactic(ModuleStack subject)
			: base(subject)
		{
			this.name = "prioritize armed";
		}

		public override XmlElement SaveXml(XmlDocument doc)
		{
			this.xmlElement = doc.CreateElement("tactic");
			this.xmlElement.SetAttribute("name", "prioritize armed");
			return this.xmlElement;
		}
	}
}
