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

        public override XmlElement SaveXml(XmlDocument doc)
        {
            base.SaveXml(doc);
            this.xmlElement.SetAttribute("type", "receiving-items");
            this.xmlElement.SetAttribute("transferrer", this.Transferer.Name);
            this.ItemStack.SaveXml(doc, "receiving");
            return this.xmlElement;
        }
	}
}
