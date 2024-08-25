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

			try
			{
				token = LineParser.GetToken(ref command);
				this.Receiver = ModuleStack.All[token];
			}
			catch (Exception ex)
			{
				throw new Exception("bad syntax or receiver does not exist", ex);
			}
		}

        public override void LoadXml(XmlElement elOrder)
        {
            XmlElement elCopy = (XmlElement)elOrder.SelectNodes("copy")[0];

            this.Receiver = ModuleStack.All[elCopy.GetAttribute("receiver")];
            this.Technology = Technology.All[elCopy.GetAttribute("technology")];
        }

        public override XmlElement SaveXml_core(XmlDocument doc, string subject)
        {
            XmlElement elCopy = doc.CreateElement("copy");

            elCopy.SetAttribute("technology", this.Technology.Name);
            elCopy.SetAttribute("receiver", this.Receiver.Name);

            xmlElement.AppendChild(elCopy);
            return xmlElement;
        }

		public override void Execute(int week)
		{
			this.Executed = false;

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
	}
}
