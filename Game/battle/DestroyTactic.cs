using System.Xml;

namespace SpaceAge
{
	public class DestroyTactic : Tactic
	{
		public DestroyTactic(ModuleStack subject)
			: base(subject)
		{
			this.name = "destroy";
		}

		public override XmlElement SaveXml(XmlDocument doc)
		{
			this.xmlElement = doc.CreateElement("tactic");
			this.xmlElement.SetAttribute("name", "destroy");
			return this.xmlElement;
		}
	}
}
