using System;
using System.Xml;

namespace SpaceAge
{
	public partial class ModuleStack
	{
        public override void LoadXml(XmlElement elModuleStack)
        {
            base.LoadXml(elModuleStack);
            this.Owner = Faction.All[elModuleStack.GetAttribute("faction")];
            this.ModuleType = ModuleType.All[elModuleStack.GetAttribute("type")];
            
            if (elModuleStack.HasAttribute("description"))
            {
                this.Description = elModuleStack.GetAttribute("description");
            }

            int modules = this.XMLAssignInteger(elModuleStack.GetAttribute("quantity"), 1);
            XmlNodeList moduleNodes = elModuleStack.SelectNodes("module");
            if (moduleNodes.Count > 0)
            {
                this.Modules.LoadXml(elModuleStack, this);
            }
            else
            {
                for (int i = 0; i < modules; i++)
                {
                    this.AddModule();
                }
            }

            this.People.LoadXml(elModuleStack, this);                
            this.ItemStacks.LoadXml(elModuleStack, this);
            this.Upkeep.LoadXml(elModuleStack, this, "upkeep");

            this.ModuleStacks.LoadXml(elModuleStack, this);
            this.Technologies.LoadXml(elModuleStack, this);
			this.ResearchPoints = this.XMLAssignInteger(elModuleStack.GetAttribute("research-points"), 0);

            this.Tactics.LoadXml(elModuleStack, this);

            this.Offers.LoadXml(elModuleStack, this.Location.Market, this);
            this.Effects.LoadXml(elModuleStack, this);
            this.EventReports.LoadXml(elModuleStack, this);
			this.SickBayUnmedicatedWeeks = this.XMLAssignInteger(elModuleStack.GetAttribute("sick-bay-weeks"), 0);

        }

        public XmlElement SaveXml(XmlDocument doc, Faction faction = null)
        {
            base.SaveXml(doc, "modulestack");
            
            this.xmlElement.SetAttribute("type", this.ModuleType.Name);
            this.xmlElement.SetAttribute("quantity", this.Quantity.ToString());
            this.xmlElement.SetAttribute("faction", this.Owner.Name);

            if (this.Description != null & this.Description != string.Empty)                
            {
                this.xmlElement.SetAttribute("description", this.Description);
            }

            this.Modules.SaveXml(doc, this.xmlElement);

            this.People.SaveXml(doc, this.xmlElement, faction);
            this.Technologies.SaveXml(doc, this.xmlElement);
			if (this.ResearchPoints > 0)
			{
				this.xmlElement.SetAttribute("research-points", this.ResearchPoints.ToString());
			}
            this.ItemStacks.SaveXml(doc, this.xmlElement, faction);
            this.Upkeep.SaveXml(doc, this.xmlElement, faction, "upkeep");
            this.Tactics.SaveXml(doc, this.xmlElement);

            this.Offers.SaveXml(doc, this.xmlElement);
            this.Effects.SaveXml(doc, this.xmlElement, faction);
            this.EventReports.SaveXml(doc, this.xmlElement, faction);
			if (this.SickBayUnmedicatedWeeks > 0)
			{
				this.xmlElement.SetAttribute("sick-bay-weeks", this.SickBayUnmedicatedWeeks.ToString());
			}
            return this.xmlElement;
        }
	}
}
