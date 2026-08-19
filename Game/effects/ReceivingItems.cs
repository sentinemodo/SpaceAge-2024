using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class ReceivingItems : Receiving
	{
        public IItemStacksHolder Transferer { get; set; }
        public ItemStack ItemStack { get; set; }

		public ReceivingItems(IEffectable receiver)
			: base(receiver, 0)
		{
		}

		public ReceivingItems(IEffectable receiver, IItemStacksHolder transferer, ItemStack itemStack, int duration)
			: base(receiver, duration)
		{
			this.ItemStack = itemStack;
            this.Transferer = transferer;
		}

		#region IReporting Members

		new public  List<string> Report(Faction faction)
		{
			List<string> lines = new List<string>();
			return lines;
		}

        public override string Description
        {
            get
            {
                return string.Format("receiving {0} from {1}, {2} weeks to complete.",
                    this.ItemStack.ReportName,
                    this.Transferer.ReportName,
                    this.Duration);
            }
        }

		#endregion

		public override void Execute(int week)
		{
			base.Execute(week);
			if (this.Duration == 0)
			{
				this.Receiver.ItemStacks.Add(this.ItemStack);
			}
		}

        public override void LoadXml(XmlElement elReceivingItems)
        {
            base.LoadXml(elReceivingItems);
            this.Transferer = this.findItemStacksHolder(elReceivingItems.GetAttribute("transferrer"));
            XmlElement elReceiving = (XmlElement)elReceivingItems.SelectSingleNode("receiving");
            if (elReceiving != null)
            {
                this.ItemStack = new ItemStack(ItemType.All[elReceiving.GetAttribute("type")]);
                this.ItemStack.LoadXml(elReceiving);
            }
        }

        public override XmlElement SaveXml(XmlDocument doc)
        {
            base.SaveXml(doc);
            this.xmlElement.SetAttribute("type", "receiving-items");
            this.xmlElement.SetAttribute("transferrer", this.Transferer.Name);
            this.xmlElement.AppendChild(this.ItemStack.SaveXml(doc, "receiving"));
            return this.xmlElement;
        }

        private IItemStacksHolder findItemStacksHolder(string name)
        {
            Person person = Person.All[name];
            if (person != null)
            {
                return person;
            }
            return ModuleStack.All.GetOrCreateNewModuleStack(this.Receiver.Owner, name);
        }
	}
}
