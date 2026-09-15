using System;
using System.Collections.Generic;
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
			this.ResearchOutputRemainder = this.XMLAssignInteger(elModuleStack.GetAttribute("research-output-remainder"), 0);

            this.Tactics.LoadXml(elModuleStack, this);

            this.Offers.LoadXml(elModuleStack, this.Location.Market, this);
            this.Effects.LoadXml(elModuleStack, this);
            this.EventReports.LoadXml(elModuleStack, this);
			this.SickBayUnmedicatedWeeks = this.XMLAssignInteger(elModuleStack.GetAttribute("sick-bay-weeks"), 0);
			if (elModuleStack.HasAttribute("allow-bank"))
			{
				this.AllowBank = this.XMLAssignBoolean(elModuleStack.GetAttribute("allow-bank"), true);
			}
			if (elModuleStack.HasAttribute("sharing"))
			{
				this.Sharing = this.XMLAssignBoolean(elModuleStack.GetAttribute("sharing"), true);
			}
			foreach (XmlElement elHold in elModuleStack.SelectNodes("hold"))
			{
				ItemType itemType = ItemType.All[elHold.GetAttribute("item-type")];
				int quantity = this.XMLAssignInteger(elHold.GetAttribute("quantity"), 0);
				this.SetItemHold(itemType, quantity);
			}

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
			if (this.ResearchOutputRemainder > 0)
			{
				this.xmlElement.SetAttribute("research-output-remainder", this.ResearchOutputRemainder.ToString());
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
			if (!this.AllowBank)
			{
				this.xmlElement.SetAttribute("allow-bank", "false");
			}
			if (!this.Sharing)
			{
				this.xmlElement.SetAttribute("sharing", "false");
			}
			foreach (KeyValuePair<ItemType, int> hold in this.itemHolds)
			{
				XmlElement elHold = doc.CreateElement("hold");
				elHold.SetAttribute("item-type", hold.Key.Name);
				elHold.SetAttribute("quantity", hold.Value.ToString());
				this.xmlElement.AppendChild(elHold);
			}
            return this.xmlElement;
        }
	}
}
