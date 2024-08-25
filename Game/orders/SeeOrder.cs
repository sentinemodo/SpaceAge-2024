using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class SeeOrder : ImmediateOrder
	{
		public SeeOrder (IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.see;
		}

        public SeeOrder(IHolder observer, string lookedForName)
			: base(observer)
		{
			this.type = EOrderType.see;
            this.lookedForName = lookedForName;
		}

		public IHolder Observer
		{
			get { return (IHolder)this.Subject; }
		}

        private ESeeType seeType = ESeeType.Modulestack;
        public ESeeType SeeType
        {
            get { return this.seeType; }
            set { this.seeType = value; }
        }

        private NamedObject lookedFor = null;
        public NamedObject LookedFor
        {
            get { return this.lookedFor; }
            set { this.lookedFor = value; }
        }

		private string lookedForName = string.Empty;
        public string LookedForName
		{
            get { return this.lookedForName; }
            set { this.lookedForName = value; }
		}

		public override void Parse(string command)
		{
			//// check if can see modulestack
			//testcommands.Add("see new1");
			//// check if can see person
			//testcommands.Add("see new1 person");

			string token;
			if (string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax");
			}

			token = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(token))
			{
				throw new Exception("Bad syntax looked for name expected");
			} else 
            {
                if (token == "person")
                {
                    this.lookedForName = token;
           			token = LineParser.GetToken(ref command);
                    if (string.IsNullOrEmpty(token))
                    {
                        throw new Exception("Bad syntax looked for person name expected");
                    }
                    else
                    {
                        this.lookedForName = token;
                        this.seeType = ESeeType.Person;
                        this.lookedFor = Person.All.GetOrCreateNewPerson(this.Observer.Owner, this.lookedForName);
                    }
                }
                else
                {
                    this.lookedForName = token;
                    this.seeType = ESeeType.Modulestack; 
                    this.lookedFor = ModuleStack.All.GetOrCreateNewModuleStack(this.Observer.Owner, this.lookedForName);
                }
            }
		}

        public override void LoadXml(XmlElement elOrder)
        {
            XmlElement elSee = (XmlElement)elOrder.SelectNodes("see")[0];
            
            switch (elSee.GetAttribute("see-type"))
            {
                case "modulestack":
                    this.SeeType = ESeeType.Modulestack;
                    this.LookedFor = ModuleStack.All.GetOrCreateNewModuleStack(
                        this.Observer.Owner, 
                        elSee.GetAttribute("modulestack"));
                    break;
                case "person":
                    this.SeeType = ESeeType.Person;
                    this.LookedFor = Person.All.GetOrCreateNewPerson(
                        this.Observer.Owner, 
                        elSee.GetAttribute("person"));
                    break;
                default:
                    throw new Exception("Unknown type for SEE");
            }
        }

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
            XmlElement elSee = doc.CreateElement("see");
            if (this.seeType == ESeeType.Modulestack)
			{
                elSee.SetAttribute("see-type", "modulestack");
                elSee.SetAttribute("modulestack", this.lookedFor.Name);
            } else 
            {
                elSee.SetAttribute("see-type", "person");
                elSee.SetAttribute("person", this.lookedFor.Name);
			}

            this.xmlElement.AppendChild(elSee);
			return this.xmlElement;
		}

        public override List<string> Report(Faction owner)
        {
            List<string> lines = new List<string>();
            string line;
            line = string.Format("{0}{1}see {2}",
					this.Conditions,
					(this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")),
					(this.seeType == ESeeType.Modulestack) ? 
						string.Concat(((ModuleStack)this.LookedFor).IsFormed ? 
							string.Empty : 
							"new", this.LookedFor.Name) : 
						string.Concat(((Person)this.LookedFor).IsFormed ? 
							string.Empty : 
							"new", this.LookedFor.Name, " person"));
            lines.Add(line);
            return lines;
		}

		public override void Execute(int week)
		{
			this.Executed = false;
            NamedObject found = null;
            if (this.Observer.Location != null)
            {
                if (this.seeType == ESeeType.Modulestack)
                {
                    if (ModuleStack.All[this.Observer.Location, true].Contains(this.lookedFor.Name))
                    {
                        found = this.lookedFor;
                        this.Executed = true;
                    }
                } else if (this.seeType == ESeeType.Person)                
                {
                    if (Person.All[this.Observer.Location, true].Contains(this.lookedFor.Name))
                    {
                        found = this.lookedFor;
                        this.Executed = true;
                    }
                }
            }
            if (this.Executed)
            {
                this.Observer.EventReports.Add(
                    week,
                    string.Format("saw {0} in {1}.",
                        found.ReportName,
                        this.Observer.Location.ReportName));
            }
            else
            {
                //this.Observer.EventReports.Add(
                //    week,
                //    string.Format("SEE failed: Didn't see {0} in {1}.",
                //        this.lookedFor.ReportName,
                //        this.Observer.Location.ReportName));
            }
			base.Execute(week);
		}
	}
}
