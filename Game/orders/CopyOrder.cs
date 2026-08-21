using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class CopyOrder : ImmediateOrder
	{
		public CopyOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.copy;
		}

		public CopyOrder(ModuleStack transferer, ModuleStack receiver, Technology technology)
			: base(transferer)
		{
			this.type = EOrderType.copy;
			this.Receiver = receiver;
			this.ReceiverName = receiver != null ? receiver.Name : null;
            this.TransferTime = 1;
            this.Technology = technology;
		}

		public ModuleStack Transferer
		{
			get { return (ModuleStack)this.Subject; }
		}

        public int TransferTime     { get; set; }

        public Technology Technology    { get; set; }
		public ModuleStack Receiver     { get; set; }
		public string ReceiverName      { get; set; }

		public override void Parse(string command)
		{
            // TODO: implement remainign transfer syntax
			// copy technology to receiver modulestack m00001
            // COPY ctypln TO m00001
            // transfer all technologies into receiver modulestack
            // COPY all TO m00001

			string token;
			if (string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax");
			}

			try
			{
				token = LineParser.GetToken(ref command);
				this.Technology = Technology.All[token];
			}
			catch (Exception ex)
			{
				throw new Exception("bad syntax or technology does not exist", ex);
			}

			token = LineParser.GetToken(ref command);
			if (token != "to")
			{
				throw new Exception("bad syntax TO expected");
			}

			token = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(token))
			{
				throw new Exception("bad syntax or receiver does not exist");
			}
			this.ReceiverName = token;
			this.Receiver = ModuleStack.All.GetOrCreateNewModuleStack(this.Transferer.Owner, token);
		}

        public override void LoadXml(XmlElement elOrder)
        {
            XmlElement elCopy = (XmlElement)elOrder.SelectNodes("copy")[0];

            this.ReceiverName = elCopy.GetAttribute("receiver");
            this.Receiver = ModuleStack.All.GetOrCreateNewModuleStack(this.Transferer.Owner, this.ReceiverName);
            this.Technology = Technology.All[elCopy.GetAttribute("technology")];
        }

        public override XmlElement SaveXml_core(XmlDocument doc, string subject)
        {
            XmlElement elCopy = doc.CreateElement("copy");

            elCopy.SetAttribute("technology", this.Technology.Name);
            string receiver = this.Receiver != null ? this.Receiver.Name : this.ReceiverName;
            if (!string.IsNullOrEmpty(receiver))
            {
                elCopy.SetAttribute("receiver", receiver);
            }

            xmlElement.AppendChild(elCopy);
            return xmlElement;
        }

		public override void Execute(int week)
		{
			this.Executed = false;

			if (this.Receiver == null || this.Receiver.ModuleType == null)
			{
				this.Transferer.EventReports.Add(
					week,
					"COPY failed. Receiver is not formed.");
				base.Execute(week);
				return;
			}

            if (this.Receiver.TechnologyCapacity < this.Receiver.TechnologyCapacityUsed + this.Technology.Level)
            {
                this.Transferer.EventReports.Add(
                    week,
                    "COPY failed. Not enough capacity to copy technology.");								
            } else if (this.Transferer.Location != this.Receiver.Location)
            {
                this.Transferer.EventReports.Add(
                    week,
                    "COPY failed. Tried to copy technology to receiver that is not in the same location.");								
            } else 
            {
			    this.Transferer.EventReports.Add(
				    week,
				    string.Format("copied {0} technology to {1}.",						
                        this.Technology.ReportName,
                        this.Receiver.ReportName));
        
                // create receive effect
                ReceivingTechnology effect = new ReceivingTechnology(
                    this.Receiver, 
                    this.Transferer, 
                    this.Technology, 
                    this.TransferTime);
                effect.Execute(week);

                // if it is instantenously executed, remove it
                if (effect.Executed)
                {
                    this.Receiver.Effects.Remove(effect);
                }

			this.Executed = true;
			}
            base.Execute(week);
        }

		public override List<string> Report(Faction owner)
		{
			List<string> lines = new List<string>();
			string receiverToken = this.reportReceiverToken();
			string line = string.Format("{0}{1}copy {2} to {3}",
				this.Conditions,
				(this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")),
				this.Technology != null ? this.Technology.Name : string.Empty,
				receiverToken);
			lines.Add(line);
			return lines;
		}

		private string reportReceiverToken()
		{
			if (!string.IsNullOrEmpty(this.ReceiverName))
			{
				return this.ReceiverName;
			}
			if (this.Receiver == null)
			{
				return string.Empty;
			}
			return this.Receiver.Name;
		}
	}
}
