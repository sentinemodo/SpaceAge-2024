using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class Person : NamedObject, IItemStacksHolder, IOfferent, IReporting, IEventReporting, IEffectable, IMoveable
	{
        public static People All = new People();         
        public const int PersonNameLength = 6;

		public Faction Owner { get; set; }

		public Race Race { get; set; }

        public IHolder Parent { get; set; }

		public override Location Location
		{
			get 
            { 
                IHolder parent = this.Parent;
                if (parent == null)
                {
                    return (Location)parent;
                }
                else
                {
                    while (!parent.Parent.IsLocation)
                    {
                        parent = parent.Parent;
                    }
                    return (Location)parent.Parent;
                }
            }
		}

        public bool IsLocation
        {
            get { return false; }
        }

        public Person(IHolder parent, string name)
            : base(name)
        {
            if (Person.All.ContainsKey(name))
                throw new Exception("Person with name [" + name + "] already exists");

            Person.All.Add(this.name, this);
            this.Parent = parent;
        }

		public Person(ModuleStack parent, Faction owner, Race race, string name)
			: base(name)
		{
			if (Person.All.ContainsKey(name))
				throw new Exception("Person with name [" + name + "] already exists");
			
			Person.All.Add(this.name, this);

			this.alias = this.name; 
			this.Parent = parent;
			this.Owner = owner;
			this.Race = race;
		}

		public Person(Faction owner, string name)
			: base("")
		{
            if (name.StartsWith("new"))
            {
                // okay if it's a new alias we need to create new identifier            
                this.name = this.GenerateRandomIdentifier();
            }
            else
            {
                // if it's not a new alias we need to check if we don't try to create a duplicate
                if (Person.All.ContainsKey(name))
                    throw new Exception("Person with name [" + name + "] already exists");

                this.name = name;
            }

            // iterate if we randomly create a duplicate
            while (Person.All.ContainsKey(this.name))
            {
                this.name = this.GenerateRandomIdentifier();
            }           
			
			Person.All.Add(this.name, this);

			this.alias = name; 
			this.Owner = owner;
            this.Race = null;
        }

        public bool IsFormed
        {
            get
            {
                if (this.Race == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

		#region IItemStacksHolder Members

		private ItemStacks itemStacks = new ItemStacks();
		public ItemStacks ItemStacks
		{
			get { return this.itemStacks; }
		}

        public ItemStack ItemStackSumRecursive(ItemType itemType)
        {
            ItemStack stack = new ItemStack(itemType, 0);
            if (this.ItemStacks.ContainsKey(itemType))
            {
                stack.Quantity += this.ItemStacks[itemType].Quantity;
            }
            return stack;
        }

        public ItemStacks ItemStacksSumRecursive
        {
            get
            {
                ItemStacks stacks = new ItemStacks();
                stacks.Sum(this.ItemStacks);
                return stacks;
            }
        }

		public double Capacity
		{
			get { return this.Race.Capacity; }
		}

		public double CapacityUsed
		{
			get { return this.ItemStacks.Size(); }
		}

        public ItemStacks Fuel
        {
            get { return null; }
        }

        public int FuelDuration
        {
            get { return 0; }
        }
		#endregion

		#region skills
		private Skills skills = new Skills();
		public Skills Skills
		{
			get { return this.skills; }
		}

		public bool IsBattleSkilled
		{
			get
			{
				if (this.skills.CountBattleSkills > 0)
				{
					return true;
				}
				return false; 
			}
		}

		public int CombatAttack(ModuleStack root)
		{
			ModuleStack host = this.Parent as ModuleStack;
			if (host == null)
			{
				return 0;
			}
			return this.skills.CombatAttack(host, root);
		}

		public int CombatDefense(ModuleStack root)
		{
			ModuleStack host = this.Parent as ModuleStack;
			if (host == null)
			{
				return 0;
			}
			return this.skills.CombatDefense(host, root);
		}

		public int CureChance(ModuleStack root)
		{
			ModuleStack host = this.Parent as ModuleStack;
			if (host == null)
			{
				return 0;
			}
			return this.skills.CureChance(host, root);
		}

		public int ResearchOutputBonus(ModuleStack host)
		{
			return this.skills.ResearchOutputBonus(host, host);
		}

		public int CombatInitiative(ModuleStack root)
		{
			ModuleStack host = this.Parent as ModuleStack;
			if (host == null)
			{
				return 0;
			}
			return this.skills.CombatInitiative(host, root);
		}

		#endregion

		#region IOrderable Members

        public bool HasOrders
        {
            get
            {
                if (this.Orders.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }
        
        private bool executedLongOrder = false;
		public bool ExecutedLongOrder
		{
			get { return this.executedLongOrder; }
			set { this.executedLongOrder = value; }
		}
		
		private Orders orders = new Orders();
		public Orders Orders
		{
			get { return this.orders; }
		}

        public bool Execute(int week)
        {
            bool executed;

            executed = this.Orders.Execute(week);
            this.Orders.RemoveExecuted();
            //this.Effects.Execute(week);
            //this.Effects.RemoveExecuted();

            return executed;
        }

		#endregion

		#region IReporting Members

		public List<string> Report(Faction faction)
		{
			return this.Report(faction, 0);
		}

		public List<string> Report(Faction faction, int level)
		{
			ReportLines reportLines = new ReportLines
            {
                { this.ReportHeader(faction), level },
                { this.reportDetails(faction), level + 1 }
            };
            if (this.Owner == faction && this.Skills.Count > 0)
            {
                reportLines.Add(this.Skills.Report(faction, level + 1));
            }
            if (this.Owner == faction && this.ItemStacks.Count > 0)
			{
				reportLines.Add(this.itemStacks.Report(faction, level + 1));
			}

            if (this.Owner == faction && this.EventReports.Count > 0)
            {
                reportLines.Add(this.EventReports.Report(faction, level + 1));
            }

			return reportLines.IndentedLines;
		}

		private string reportDetails(Faction faction)
		{
			string line = "";
			bool started = false;

			#region size
			line = string.Format("{0}{1}size: {2}", line, (started) ? ", " : "", this.Race.Size);
			started = true;
			#endregion

			line = this.reportMass(faction, line);	
			line = this.reportCapacity(faction, line);

			line = this.reportUpkeep(faction, line);
			line = this.reportConsume(faction, line);
			return string.Concat(line, ".");
		}

		private string reportConsume(Faction faction, string line)
		{
			if (this.Owner == faction && this.Consume.Count > 0)
			{
				line = string.Format("{0}consume:",
					(line == string.Empty) ? string.Empty : string.Concat(line, ", "));

				int netto;
				bool firstAdded = false;
				foreach (ItemStack itemstack in this.Consume.Values)
				{
					netto = this.ConsumeNetto.Quantity(itemstack.ItemType);
					line = string.Format("{0} {1}{2}{3}",
						(firstAdded == true) ? string.Concat(line, ",") : line,
						(itemstack.Quantity > 1) ? string.Concat(itemstack.Quantity, " ") : "",
						(netto != itemstack.Quantity) ? string.Concat("(", netto, ") ") : "",
						(netto > 1 || itemstack.Quantity > 1) ? itemstack.ItemType.ReportNameMultiple : itemstack.ItemType.ReportName);

					firstAdded = true;
				}
			}
			return line;
		}

		private string reportUpkeep(Faction faction, string line)
		{
			if (this.Owner == faction && this.Upkeep.Count > 0)
			{
				line = string.Format("{0}upkeep:",
					(line == string.Empty) ? string.Empty : string.Concat(line, ", "));

				int netto;
				bool firstAdded = false;
				foreach (ItemStack itemstack in this.Upkeep.Values)
				{
					netto = this.UpkeepNetto.Quantity(itemstack.ItemType);
					line = string.Format("{0} {1}{2}{3}",
						(firstAdded == true) ? string.Concat(line, ",") : line,
						(itemstack.Quantity > 1) ? string.Concat(itemstack.Quantity, " ") : "",
						(netto != itemstack.Quantity) ? string.Concat("(", netto, ") ") : "",
						(netto > 1 || itemstack.Quantity > 1) ? itemstack.ItemType.ReportNameMultiple : itemstack.ItemType.ReportName);

					firstAdded = true;
				}
			}
			return line;
		}

		private string reportCapacity(Faction faction, string line)
		{
			if (this.Owner == faction)
			{
				line = string.Format("{0}capacity: {1}/{2}",
					(line == string.Empty) ? string.Empty : string.Concat(line, ", "),
					this.Capacity,
					this.CapacityUsed);
			}
			return line;
		}

		private string reportMass(Faction faction, string line)
		{
			if (this.Owner == faction)
			{
				line = string.Format("{0}mass: {1}", 
					(line == string.Empty) ? string.Empty : string.Concat(line, ", "),
					this.Mass);
	
				if (this.Mass > this.Race.Mass)
				{
					line = string.Format("{0} ({1})",
						line,
						this.MassNetto);
				}
			}
			return line;
		}

		public string ReportHeader(Faction faction)
		{
            string line;
            if (this.IsFormed)
            {
                line = string.Format("{0} {1}, {2}",
                    (this.Owner == faction) ? "+" : "-",
                    this.ReportName,
                    this.Race.ReportName);
            } else
            {
                line = string.Format("{0} {1}",
                    (this.Owner == faction) ? "+" : "-",
                    this.ReportName);
            }

            if (this.Owner != null & this.Owner != faction)
            {
                line = string.Concat(line, ", working for ", this.Owner.ReportName);
            }

            line = string.Concat(line, ".");

            return line;
		}

		public List<string> ReportOrdersTemplateHeader(Faction faction)
		{
			ReportLines reportLines = new ReportLines
            {
                this.ReportHeader(faction)
            };
			if (this.Skills.Count > 0)
			{
				reportLines.Add(string.Concat("skills: ", this.Skills.ReportList, "."));
			}
			if (this.ItemStacks.Count > 0)
			{
				reportLines.Add(string.Concat("items: ", this.ItemStacks.ReportList, "."));
			}
			if (this.Effects.Count > 0)
			{
				foreach (string line in this.Effects.Report(faction))
				{
					reportLines.Add(line);
				}
			}

			return reportLines.IndentedLines;
		}

		#endregion

		public List<string> BattleReport(Faction faction)
		{
			ReportLines lines = new ReportLines
            {
                this.ReportHeader(faction),
                { this.reportBattleDetails(faction), 1 }
            };
			return lines.IndentedLines;
		}

		private List<string> reportBattleDetails(Faction faction)
		{
			ReportLines lines = new ReportLines();

			string line;

			line = string.Empty;
			line = this.reportMass(faction, line);
			line = this.reportAttack(faction, line);
			line = this.reportDefense(faction, line);
			line = this.reportInitiative(faction, line);
			lines.Add(string.Concat(line, "."));

			if (this.Owner == faction)
			{
				line = string.Empty;
				line = this.reportBattleSkills(line);
				lines.Add(string.Concat(line, "."));
			}
			return lines.IndentedLines;
		}

		private string reportBattleSkills(string line)
		{
			ModuleStack host = this.Parent as ModuleStack;
			if (host == null)
			{
				return line;
			}

			line = string.Format("{0}skills: ",
			   (line == string.Empty) ? string.Empty : string.Concat(line, ", "));

			bool firstAdded = false;
			foreach (Skill skill in this.skills.Values)
			{
				if (skill.SkillType.IsBattleSkill && skill.Experience >= 1)
				{
					line = string.Format("{0}{1}", line, (firstAdded) ? ", " : "");
					firstAdded = true;
					line = string.Concat(line, skill.SkillType.ReportDetails(skill.Experience, host, host.RootModuleStack));
				}
			}
			return line;
		}

		private string reportAttack(Faction faction, string line)
		{
			ModuleStack host = this.Parent as ModuleStack;
			if (host == null)
			{
				return line;
			}
			int attack = this.CombatAttack(host.RootModuleStack);
			if (attack > 0)
			{
				line = string.Format("{0}attack: {1}",
				(line == string.Empty) ? string.Empty : string.Concat(line, ", "),
				attack);
			}
			return line;
		}

		private string reportDefense(Faction faction, string line)
		{
			ModuleStack host = this.Parent as ModuleStack;
			if (host == null)
			{
				return line;
			}
			int defense = this.CombatDefense(host.RootModuleStack);
			if (defense > 0)
			{
				line = string.Format("{0}defense: {1}",
				   (line == string.Empty) ? string.Empty : string.Concat(line, ", "),
				   defense);
			}
			return line;
		}

		private string reportInitiative(Faction faction, string line)
		{
			ModuleStack host = this.Parent as ModuleStack;
			if (host == null)
			{
				return line;
			}
			int initiative = this.CombatInitiative(host.RootModuleStack);
			if (initiative != 0)
			{
				line = string.Format("{0}initiative: {1}",
				   (line == string.Empty) ? string.Empty : string.Concat(line, ", "),
				   initiative);
			}
			return line;
		}

        protected string alias = string.Empty;
        public override string Alias
        {
            get { return string.Format("{0}_{1}", this.Owner.Name, this.alias); }
            set { this.alias = value; }
        }


		public override string ReportName
		{
			get
			{
                if (this.IsFormed)
                {
                    return string.Format("{0} [{1}]",
                        string.IsNullOrEmpty(this.FullName) ? string.Concat(this.Race.FullName + " officer") : this.FullName,
                        this.name);
                }
                else
                {
                    return string.Format("{0} [{1}]",
                        string.IsNullOrEmpty(this.FullName) ? "unformed officer" : this.FullName,
                        this.name);
                }
			}
		}

		public bool Visible(Faction faction)
		{
			return true;
		}

		public double MassNetto
		{
			get { return this.Race.Mass; }
		}

		public double Mass
		{
			get { return this.Race.Mass + this.ItemStacks.Mass(); }
		}


		public ItemStacks UpkeepNetto
		{
			get 
			{
				return this.Race.Upkeep;
			}
		}

		public ItemStacks Upkeep
		{
			get
			{
				ItemStacks upkeep = new ItemStacks();
				upkeep.Sum(this.UpkeepNetto);
				upkeep.Sum(this.itemStacks.Upkeep);
				return upkeep;
			}
		}

		public ItemStacks ConsumeNetto
		{
			get
			{
				return this.Race.Consume;
			}
		}

		public ItemStacks Consume
		{
			get 
			{
				ItemStacks counsume = new ItemStacks();
				counsume.Sum(this.ConsumeNetto);
				counsume.Sum(this.ItemStacks.Consume);
				return counsume;
			}
		}

		#region IEventReporting Members
		
		private EventReports eventReports = new EventReports();
		public EventReports EventReports
		{
			get { return this.eventReports; }
		}

		#endregion

        public string BattleReportName
        {
            get { throw new NotImplementedException(); }
        }

        public People People
        {
            get { return null; }
        }

        public bool HasPeople
        {
            get { return false; }
        }

        public ModuleStacks ModuleStacks
        {
            get { return null; }
        }

        public bool HasModuleStacks(ModuleType moduleType = null)
        {
            return false;
        }

        public Moving Moving
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        
        public MoveModes MoveModes
        {
            get
            {
                return this.Race.MoveModes;
            }
        }

        #region IEffectable Members

        private Effects effects = new Effects();
        public Effects Effects
        {
            get { return this.effects; }
        }

        #endregion

		#region economy

        public Offers Offers
        {
            get { return Offer.All[this]; }
        }

		public bool HasBankAccess
		{
			get { return true; }
		}

		#endregion


        public int TechnologyCapacity
        {
            get { return 0; }
        }

        public int TechnologyCapacityUsed
        {
            get { return 0; }
        }

        public override void LoadXml(XmlElement elPerson)
        {
            base.LoadXml(elPerson);
            this.Owner = Faction.All[elPerson.GetAttribute("faction")];
            this.Race = Race.All[elPerson.GetAttribute("race")];

            if (elPerson.HasAttribute("description"))
            {
                this.Description = elPerson.GetAttribute("description");
            }

            this.Skills.LoadXml(elPerson, this);
            this.ItemStacks.LoadXml(elPerson, this);
            this.Upkeep.LoadXml(elPerson, this, "upkeep");

            this.Offers.LoadXml(elPerson, this.Location.Market, this);
            this.Effects.LoadXml(elPerson, this);
            this.EventReports.LoadXml(elPerson, this);

        }

        public XmlElement SaveXml(XmlDocument doc, Faction faction = null)
        {
            base.SaveXml(doc, "person");

            this.xmlElement.SetAttribute("race", this.Race.Name);
            this.xmlElement.SetAttribute("faction", this.Owner.Name);

            if (this.Description != null & this.Description != string.Empty)
            {
                this.xmlElement.SetAttribute("description", this.Description);
            }

            this.Skills.SaveXml(doc, this.xmlElement, faction);
            this.ItemStacks.SaveXml(doc, this.xmlElement, faction);
            this.Upkeep.SaveXml(doc, this.xmlElement, faction, "upkeep");

            this.Offers.SaveXml(doc, this.xmlElement);
            this.Effects.SaveXml(doc, this.xmlElement, faction);
            this.EventReports.SaveXml(doc, this.xmlElement, faction);

            return this.xmlElement;
        }
    }
}
