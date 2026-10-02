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

		private ESeeScope seeScope = ESeeScope.CurrentLocation;
		public ESeeScope SeeScope
		{
			get { return this.seeScope; }
			set { this.seeScope = value; }
		}

		private Region atRegion = null;
		public Region AtRegion
		{
			get { return this.atRegion; }
			set { this.atRegion = value; }
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
						this.parseSeeScope(ref command);
                    }
                }
                else
                {
                    this.lookedForName = token;
                    string afterName = command;
                    string suffix = LineParser.GetToken(ref afterName);
                    if (suffix == "person")
                    {
                        this.seeType = ESeeType.Person;
                        this.lookedFor = Person.All.GetOrCreateNewPerson(this.Observer.Owner, this.lookedForName);
						this.parseSeeScope(ref afterName);
                    }
                    else
                    {
                        this.seeType = ESeeType.Modulestack; 
                        this.lookedFor = ModuleStack.All.GetOrCreateNewModuleStack(this.Observer.Owner, this.lookedForName);
						string scopePart = afterName;
						if (!string.IsNullOrEmpty(suffix))
						{
							scopePart = string.IsNullOrEmpty(afterName)
								? suffix
								: string.Concat(suffix, " ", afterName);
						}
						this.parseSeeScope(ref scopePart);
                    }
                }
            }
		}

		private void parseSeeScope(ref string command)
		{
			string token = LineParser.GetToken(ref command);
			if (string.IsNullOrEmpty(token))
			{
				this.seeScope = ESeeScope.CurrentLocation;
				return;
			}
			if (string.Equals(token, "ANYWHERE", StringComparison.OrdinalIgnoreCase))
			{
				this.seeScope = ESeeScope.Anywhere;
				return;
			}
			if (string.Equals(token, "AT", StringComparison.OrdinalIgnoreCase))
			{
				string regionName = LineParser.GetToken(ref command);
				if (string.IsNullOrEmpty(regionName) || !Region.All.ContainsKey(regionName))
				{
					throw new Exception("Bad syntax region id expected after AT");
				}
				this.seeScope = ESeeScope.AtRegion;
				this.atRegion = Region.All[regionName];
				return;
			}
			throw new Exception("Bad syntax for SEE scope");
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
			if (string.Equals(elSee.GetAttribute("anywhere"), "yes", StringComparison.OrdinalIgnoreCase))
			{
				this.seeScope = ESeeScope.Anywhere;
			}
			else if (elSee.HasAttribute("at-region"))
			{
				string regionName = elSee.GetAttribute("at-region");
				if (!Region.All.ContainsKey(regionName))
				{
					throw new Exception("Unknown at-region for SEE");
				}
				this.seeScope = ESeeScope.AtRegion;
				this.atRegion = Region.All[regionName];
			}
			else
			{
				this.seeScope = ESeeScope.CurrentLocation;
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
			if (this.seeScope == ESeeScope.Anywhere)
			{
				elSee.SetAttribute("anywhere", "yes");
			}
			else if (this.seeScope == ESeeScope.AtRegion && this.atRegion != null)
			{
				elSee.SetAttribute("at-region", this.atRegion.Name);
			}

            this.xmlElement.AppendChild(elSee);
			return this.xmlElement;
		}

        public override List<string> Report(Faction owner)
        {
            List<string> lines = new List<string>();
			string scopeSuffix = string.Empty;
			if (this.seeScope == ESeeScope.Anywhere)
			{
				scopeSuffix = " anywhere";
			}
			else if (this.seeScope == ESeeScope.AtRegion && this.atRegion != null)
			{
				scopeSuffix = string.Format(" at {0}", this.atRegion.Name);
			}
            string line;
            line = string.Format("{0}{1}see {2}{3}",
					this.Conditions,
					(this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")),
					(this.seeType == ESeeType.Modulestack) ? 
						string.Concat(((ModuleStack)this.LookedFor).IsFormed ? 
							string.Empty : 
							"new", this.LookedFor.Name) : 
						string.Concat(((Person)this.LookedFor).IsFormed ? 
							string.Empty : 
							"new", this.LookedFor.Name, " person"),
					scopeSuffix);
            lines.Add(line);
            return lines;
		}

		public override void Execute(int week)
		{
			this.Executed = false;
            NamedObject found = null;
			ModuleStack observerStack = this.resolveObserverStack();
            if (this.Observer.Location != null && observerStack != null)
            {
                if (this.seeType == ESeeType.Modulestack)
                {
					ModuleStack target = this.resolveModuleTarget();
					if (target != null
						&& StackVisibility.CanSeeModuleForSeeOrder(
							observerStack,
							target,
							this.seeScope,
							this.atRegion))
                    {
                        found = target;
                        this.Executed = true;
                    }
                } else if (this.seeType == ESeeType.Person)                
                {
					Person target = this.resolvePersonTarget();
					if (target != null
						&& StackVisibility.CanSeePersonForSeeOrder(
							observerStack,
							target,
							this.seeScope,
							this.atRegion))
                    {
                        found = target;
                        this.Executed = true;
                    }
                }
            }
            if (this.Executed)
            {
				Location reportLocation = found is ModuleStack
					? ((ModuleStack)found).Location
					: ((Person)found).Location;
                this.Observer.EventReports.Add(
                    week,
                    string.Format("saw {0} in {1}.",
                        found.ReportName,
                        reportLocation != null ? reportLocation.ReportName : this.Observer.Location.ReportName));
            }
			base.Execute(week);
		}

		private ModuleStack resolveModuleTarget()
		{
			if (ModuleStack.All.ContainsKey(this.lookedForName))
			{
				return ModuleStack.All[this.lookedForName];
			}
			return this.LookedFor as ModuleStack;
		}

		private Person resolvePersonTarget()
		{
			if (Person.All.ContainsKey(this.lookedForName))
			{
				return Person.All[this.lookedForName];
			}
			return this.LookedFor as Person;
		}

		private ModuleStack resolveObserverStack()
		{
			ModuleStack stack = this.Observer as ModuleStack;
			if (stack != null)
			{
			 return stack;
			}
			Person person = this.Observer as Person;
			if (person != null && person.Parent is ModuleStack)
			{
				return (ModuleStack)person.Parent;
			}
			return null;
		}
	}
}
