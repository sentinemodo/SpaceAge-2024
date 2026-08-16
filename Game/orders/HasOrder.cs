using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class HasOrder : ImmediateOrder
	{
		public HasOrder (IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.has;
		}

		public HasOrder(IItemStacksHolder observer, ItemType itemType, int quantity)
			: base(observer)
		{
			this.type = EOrderType.has;
			this.itemType = itemType;
			this.quantity = quantity;
		}

		public HasOrder(IHolder observer, string personName)
			: base(observer)
		{
			this.type = EOrderType.has;
			this.personName = personName;
		}


		public IHolder Observer
		{
			get { return (IHolder)this.Subject; }
		}

        private EHasType hasType = EHasType.Itemstack;
        public EHasType HasType
        {
            get { return this.hasType; }
            set { this.hasType = value; }
        }

		private ItemType itemType = null;
		public ItemType ItemType
		{
			get { return this.itemType; }
			set { this.itemType = value; }
		}

		private ModuleType moduleType = null;
		public ModuleType ModuleType
		{
			get { return this.moduleType; }
			set { this.moduleType = value; }
		}

		private int quantity = 0;
		public int Quantity
		{
			get { return this.quantity; }
			set { this.quantity = value; }
		}

		private string personName = string.Empty;
		public string PersonName
		{
			get { return this.personName; }
			set { this.personName = value; }
		}

		private Person person = null;
		public Person Person
		{
			get { return this.person; }
			set { this.person = value; }
		}

		public override void Parse(string command)
		{
			//// check if active
			//testcommands.Add("has 20 iron");
			//testcommands.Add("has person 000001");
            //testcommands.Add("has modules 5"); 

			string token;
			if (string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax, quantity and itemtype or person and person id expected.");
			}

			token = LineParser.GetToken(ref command);
            if (token == "modules")
            {
                this.hasType = EHasType.Modules;
                token = LineParser.GetToken(ref command);
                if (string.IsNullOrEmpty(token))
                {
                    this.Quantity = -1;
                }
                else
                {
                    try
                    {
                        this.quantity = Convert.ToInt32(token);
                    }
                    catch
                    {
                        throw new Exception("bad syntax quantity expected. Received: " + token);
                    }
                }
            }
            else if (token == "person")
            {
                this.hasType = EHasType.Person;
                token = LineParser.GetToken(ref command);
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("Bad syntax person name expected");
                }
                else
                {
                    this.personName = token;
                    this.person = Person.All.GetOrCreateNewPerson(this.Observer.Owner, this.personName);
                }
            } else
			{
                try
				{
					this.quantity = Convert.ToInt32(token);
				}
				catch
				{
					throw new Exception("bad syntax quantity expected. Received: " + token);
				}

				token = LineParser.GetToken(ref command);
				if (ItemType.All.ContainsKey(token))
				{
					this.hasType = EHasType.Itemstack;
					this.itemType = ItemType.All[token];
				}
				else if (ModuleType.All.ContainsKey(token))
				{
					this.hasType = EHasType.ModuleType;
					this.moduleType = ModuleType.All[token];
				}
				else
				{
					throw new Exception("bad syntax, item type or module type expected. Received: " + token);
				}
			}
		}

        public override void LoadXml(XmlElement elOrder)
        {
            XmlElement elHas = (XmlElement)elOrder.SelectNodes("has")[0];
            foreach (XmlElement elHasType in elHas.ChildNodes)
            {
                switch (elHasType.Name)
                {
                    case "itemstack":
                        this.HasType = EHasType.Itemstack;
                        this.Quantity = this.XMLAssignInteger(elHasType.GetAttribute("quantity"), 0);
                        this.ItemType = ItemType.All[elHasType.GetAttribute("item")];
                        break;
                    case "person":
                        this.HasType = EHasType.Person;
                        this.Person = Person.All[elHasType.GetAttribute("name")];
                        break;
                    case "modules":
                        this.HasType = EHasType.Modules;
                        this.Quantity = this.XMLAssignInteger(elHasType.GetAttribute("quantity"), -1);
                        break;
                    case "moduletype":
                        this.HasType = EHasType.ModuleType;
                        this.Quantity = this.XMLAssignInteger(elHasType.GetAttribute("quantity"), 0);
                        this.ModuleType = ModuleType.All[elHasType.GetAttribute("module")];
                        break;
                    default:
                        throw new Exception("Unknown type for HAS");
                }
            }
        }

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
            XmlElement elHas = doc.CreateElement("has");

            switch (this.hasType)
            {
                case EHasType.Itemstack:
				    XmlElement elItemStack;
				    elItemStack = doc.CreateElement("itemstack");
				    elItemStack.SetAttribute("item", this.itemType.Name);
				    elItemStack.SetAttribute("quantity", this.quantity.ToString());
                    elHas.AppendChild(elItemStack);
                    break;
                case EHasType.Person:
				    XmlElement elPerson;
				    elPerson = doc.CreateElement("person");
				    elPerson.SetAttribute("name", this.person.Name);
                    elHas.AppendChild(elPerson);
                    break;
                case EHasType.Modules:
				    XmlElement elModules;
				    elModules = doc.CreateElement("modules");
				    elModules.SetAttribute("quantity", this.quantity.ToString());
                    elHas.AppendChild(elModules);
                    break;
                case EHasType.ModuleType:
				    XmlElement elModuleType;
				    elModuleType = doc.CreateElement("moduletype");
				    elModuleType.SetAttribute("module", this.moduleType.Name);
				    elModuleType.SetAttribute("quantity", this.quantity.ToString());
                    elHas.AppendChild(elModuleType);
                    break;
                default:
                    throw new Exception("Unknown type for HAS");
            }

            xmlElement.AppendChild(elHas);
			return this.xmlElement;
		}

        public override List<string> Report(Faction owner)
        {
            List<string> lines = new List<string>();
            string line = string.Empty;
            switch (this.hasType)
            {
                case EHasType.Itemstack:
                    line = string.Format("{0}{1}has {2}",
                        this.Conditions,
                        (this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")),
                        string.Concat(this.quantity.ToString(), " ", this.itemType.Name));
                    break;
                case EHasType.Person:
                    line = string.Format("{0}{1}has {2}",
                        this.Conditions,
                        (this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")),
                        string.Concat("person ", this.person.Name));
                    break;
                case EHasType.Modules:
                    line = string.Format("{0}{1}has {2}",
                        this.Conditions,
                        (this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")),
                        string.Concat("modules ", this.quantity.ToString()));
                    break;
                case EHasType.ModuleType:
                    line = string.Format("{0}{1}has {2}",
                        this.Conditions,
                        (this.Repeat == 1) ? string.Empty : ((this.Repeat < 0) ? "@" : string.Concat(this.Repeat.ToString(), " ")),
                        string.Concat(this.quantity.ToString(), " ", this.moduleType.Name));
                    break;
            }
            lines.Add(line);
            return lines;
        }


		public override void Execute(int week)
		{
			this.Executed = false;
            switch (this.hasType)
            {
                case EHasType.Itemstack:
                    IItemStacksHolder holder = (IItemStacksHolder)this.Observer;
                    ItemStack existing = holder.ItemStackSumRecursive(this.itemType);
                    if (existing.Quantity >= this.quantity) {
            		    this.Executed = true;
                        this.Observer.EventReports.Add(
                            week,
                            string.Format("has {0} (needed {1}).",
                                existing.ReportName,
                                this.quantity));
                    }	
                    break;
                case EHasType.Person:
                    if (Person.All[this.Observer, true].Contains(this.personName)) 
                    {
				        this.Executed = true;
                        this.Observer.EventReports.Add(
                            week,
                            string.Format("has {0}.",
                                this.person.ReportName));
                    }
                    break;
                case EHasType.Modules:
                    if ((((ModuleStack)this.Observer).Quantity > 0) &
                        (((ModuleStack)this.Observer).Quantity >= this.quantity))
                    {
                        this.Executed = true;
                        this.Observer.EventReports.Add(
                            week,
                            string.Format("has {0}modules in stack.",
                            (this.quantity == -1) ? string.Empty : string.Concat(this.Quantity, " ")));
                    }
                    break;
                case EHasType.ModuleType:
                    int moduleCount = ((ModuleStack)this.Observer).ModuleCountRecursive(this.moduleType);
                    if (moduleCount >= this.quantity)
                    {
                        this.Executed = true;
                        this.Observer.EventReports.Add(
                            week,
                            string.Format("has {0} {1} modules (needed {2}).",
                                moduleCount,
                                this.moduleType.Name,
                                this.quantity));
                    }
                    break;
            }
    		base.Execute(week);
		}
	}
}
