using System.Xml;

namespace SpaceAge
{
	public class PrioritizeCommandTactic : Tactic
	{
		public PrioritizeCommandTactic(ModuleStack subject)
			: base(subject)
		{
			this.name = "prioritize command";
		}

		public override XmlElement SaveXml(XmlDocument doc)
		{
			this.xmlElement = doc.CreateElement("tactic");
			this.xmlElement.SetAttribute("name", "prioritize command");
			return this.xmlElement;
		}
	}
}
