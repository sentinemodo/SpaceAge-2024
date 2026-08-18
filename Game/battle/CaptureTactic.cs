using System.Xml;

namespace SpaceAge
{
	public class CaptureTactic : Tactic
	{
		public CaptureTactic(ModuleStack subject)
			: base(subject)
		{
			this.name = "capture";
		}

		public override XmlElement SaveXml(XmlDocument doc)
		{
			this.xmlElement = doc.CreateElement("tactic");
			this.xmlElement.SetAttribute("name", "capture");
			return this.xmlElement;
		}
	}
}
