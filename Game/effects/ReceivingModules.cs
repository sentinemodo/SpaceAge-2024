using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class ReceivingModules : Receiving
	{

        public ModuleStack Transferer { get; set; }
        public ModuleStack ModuleStack { get; set; }

		public ReceivingModules(IEffectable receiver, ModuleStack transferer, ModuleStack moduleStack, int duration)
			: base(receiver, duration)
		{
			this.ModuleStack = moduleStack;
            this.Transferer = transferer;
		}

        public override string Description
        {
            get
            {
                return string.Format("receiving {0} from {1}, {2} weeks to complete.",
                    (this.ModuleStack.Quantity > 1) ? string.Format("{0} {1}",
                        this.ModuleStack.Quantity,
                        this.ModuleStack.ModuleType.ReportNameMultiple) :
                        this.ModuleStack.ModuleType.ReportName,                         
                    this.Transferer.ReportName,
                    this.Duration);
            }
        }

		#region IReporting Members

		new public  List<string> Report(Faction faction)
		{
			List<string> lines = new List<string>();
			return lines;
		}

		#endregion

		public override void Execute(int week)
		{
            // TODO: what if the receiver is moving and for example changed region?
            // update moduleType 
            this.Receiver.EventReports.Add(
                week, 
                string.Format("received {0} from {1}.",
                    (this.ModuleStack.Quantity > 1) ? string.Format("{0} {1}",
                        this.ModuleStack.Quantity,
                        this.ModuleStack.ModuleType.ReportNameMultiple) :
                        this.ModuleStack.ModuleType.ReportName,
                    this.Transferer.ReportName));

            if (this.Receiver.ModuleType == null)
            {
                this.Receiver.ModuleType = this.ModuleStack.ModuleType;
            }

            if (this.Receiver.ModuleType == this.ModuleStack.ModuleType)
            {
                Module module;
                int quantity = this.ModuleStack.Quantity;
                for (int i = 0; i < quantity; i++)
                {
                    module = this.ModuleStack.Modules[0];
                    this.ModuleStack.RemoveModule(0);                    
                    this.Receiver.AddModule(module);
                }

                // keeping in mind status of long order execution to avoid transfer/new order abuse
                if (this.ModuleStack.ExecutedLongOrder)
                {
                    // accepting modules to the stack that executed long order forfeits the ability to execute long order
                    this.Receiver.ExecutedLongOrder = true;
                }

                // since it has been moved to receiver, remove the transferred stack
                ModuleStack.All.Remove(this.ModuleStack);
            }
            else
            {
                this.ModuleStack.Parent = this.Receiver;
                this.ModuleStack.Owner = this.Receiver.Owner;
            }

            this.Executed = true;
            base.Execute(week);           
		}

        public override XmlElement SaveXml(XmlDocument doc)
        {
            base.SaveXml(doc);
            this.xmlElement.SetAttribute("type", "receiving-modules");
            this.xmlElement.SetAttribute("modulestack", this.ModuleStack.Name);
            this.xmlElement.SetAttribute("transferrer", this.Transferer.Name);
            return this.xmlElement;
        }
	}
}
