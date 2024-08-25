using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class ModuleStacks : Dictionary<string, ModuleStack>, IReporting
	{

		public ModuleStacks this[Faction faction]
		{
			get
			{
				ModuleStacks list = new ModuleStacks();
				foreach (ModuleStack moduleStack in this.Values)
				{
					if (moduleStack.Owner == faction)
					{
						list.Add(moduleStack.Name, moduleStack);
					}
				}
				return list;
			}
		}


        // when searching by alias a format of new1 is expected not 2_new1
		public ModuleStack this[Faction faction, string name, bool byAlias = false]
		{
			get
			{
                ModuleStack found = null;
                if (!byAlias)
                {
                    foreach (ModuleStack moduleStack in this[faction].Values)
                    {
                        if (moduleStack.Name == name)
                        {
                            found = moduleStack;
                        }
                    }
                } else
                {
                    foreach (ModuleStack moduleStack in this[faction].Values)
                    {
                        if (moduleStack.Alias == string.Format("{0}_{1}", faction.Name, name))
                        {
                            found = moduleStack;
                        }
                    }
                }
                return found;                
			}
		}

		public ModuleStack this[string name, bool byAlias = false]
		{
			get
			{
				ModuleStack found = null;
                if (!byAlias)
                {
                    foreach (ModuleStack moduleStack in this.Values)
                    {
                        if (moduleStack.Name == name)
                        {
                            found = moduleStack;
                        }
                    }
                } else
                {
                    foreach (ModuleStack moduleStack in this.Values)
                    {
                        if (moduleStack.Alias == name)
                        {
                            found = moduleStack;
                        }
                    }
                }
                return found;
			}
		}

		// this applies to ModuleStacks, Regions in planet, atmosphere, moon, Orbits, Sectors in SpaceSystems and Quadrants in Galaxy
		public ModuleStacks this[IHolder holder, bool recursive = false]
		{
			get
			{
				ModuleStacks list = new ModuleStacks();
				foreach (ModuleStack moduleStack in this.Values)
				{
					if (moduleStack.Parent == holder)
					{
						list.Add(moduleStack.Name, moduleStack);
					}
				}
                if (recursive)
                {
                    foreach (ModuleStack moduleStack in holder.ModuleStacks.Values)
                    {
                        list.Add(ModuleStack.All[moduleStack, true]);
                    }
                }
				return list;
			}
		}

        public ModuleStack GetOrCreateNewModuleStack(Faction owner, string name)
        {
            return this.GetOrCreateNewModuleStack(owner, name, false);
        }

        public ModuleStack GetOrCreateNewModuleStack(Faction owner, string name, bool onlyOwned = false)
        {
            // try to find exact match
            ModuleStack moduleStack = this[owner, name];

            // okay, try to find a name match for other faction
            if (!onlyOwned & moduleStack == null)
            {
                moduleStack = this[name];
            }

            // okay we still don't have it, lets check if it wasn't a person not a modulestack
            if (moduleStack == null)
            {
                Person person = Person.All[owner, name];
                if (person != null)
                {
                    throw new TypeInitializationException(
                        string.Concat(
                            "Tried to find modulestack with name ", name, " while there is already person with same name."), null);
                }

                person = Person.All[owner, name, true];
                if (person != null)
                {
                    throw new TypeInitializationException(
                        string.Concat(
                            "Tried to find modulestack with name ", name, " while there is already person with same name."), null);
                }

            }

            // okay, we still don't have it, but perhaps it was an alias
            if (moduleStack == null)
            {
                // faction match
                moduleStack = this[owner, name, true];

                if (moduleStack == null)
                {
                    // let's check a no faction alias match
                    moduleStack = this[name, true];
                }
            }

            // okay, no choice, we need to create a new one
            if (moduleStack == null)
            {
                moduleStack = new ModuleStack(owner, name);
            }

            // we have to have it now
            return moduleStack;
        }

		public List<Faction> Owners
		{
			get
			{
				List<Faction> owners = new List<Faction>();
				foreach (ModuleStack moduleStack in this.Values)
				{
					foreach (Faction faction in moduleStack.Owners)
					{
						if (!owners.Contains(faction))
						{
							owners.Add(faction);
						}
					}
				}
				return owners;
			}
		}

		public ModuleStacks HavingOrders
		{
			get
			{
				//List<ModuleStack> havingOrders = new List<ModuleStack>();
				ModuleStacks havingOrders = new ModuleStacks();

				foreach (ModuleStack moduleStack in this.Values)
				{
					if (moduleStack.HasOrders)
					{
						havingOrders.Add(moduleStack.Name, moduleStack);
					}
				}
				return havingOrders;
			}
		}

		public int Quantity(EModuleTypesGroup group)
		{
			int count = 0;
			foreach (ModuleStack moduleStack in this.Values)
			{
				if (moduleStack.ModuleType.Group == group)
				{
					count += moduleStack.Quantity;
				}				
				count += moduleStack.ModuleStacks.Quantity(group);
			}
			return count;
		}


		public bool Contains(Faction faction)
		{
			bool found = false;
			foreach (ModuleStack moduleStack in this.Values)
			{
				if (moduleStack.Owner == faction || moduleStack.ModuleStacks.Contains(faction))
				{
					found = true;
					break;
				}
			}
			return found;
		}

		public bool Contains(string name)
		{
			bool found = false;
			foreach (ModuleStack moduleStack in this.Values)
			{
				if (moduleStack.Name == name)
				{
					found = true;
					break;
				}
			}
			return found;
		}

		public bool Contains(Faction faction, string name)
		{
			bool found = false;
			foreach (ModuleStack moduleStack in this.Values)
			{
				if (moduleStack.Name == name)
				{
					found = true;
					break;
				}
				if (moduleStack.Owner == faction)
				{
					if (moduleStack.Alias == string.Format("{0}_{1}", faction.Name, name))
					{
						found = true;
						break;
					}
				}
			}
			return found;
		}

		public double Size()
		{
			double size = 0;
			foreach (ModuleStack moduleStack in this.Values)
			{
				size += moduleStack.Size;
				size += moduleStack.ModuleStacks.Size();
			}
			return size;
		}

		public bool Contains(Faction faction, EModuleTypesGroup group)
		{
			bool found = false;
			foreach (ModuleStack moduleStack in this.Values)
			{
				if ((moduleStack.Owner == faction && moduleStack.ModuleType.Group == group) 
					|| moduleStack.ModuleStacks.Contains(faction, group))
				{
					found = true;
					break;
				}
			}
			return found;
		}

		public bool Contains(EModuleTypesGroup group)
		{
			bool found = false;
			foreach (ModuleStack moduleStack in this.Values)
			{
				if (moduleStack.ModuleType.Group == group | moduleStack.ModuleStacks.Contains(group))
				{
					found = true;
					break;
				}
			}
			return found;
		}

		public int EnergyProduction()
		{
			int energyProduction = 0;
			foreach (ModuleStack moduleStack in this.Values)
			{
				energyProduction += moduleStack.EnergyProduction;
				energyProduction += moduleStack.ModuleStacks.EnergyProduction();
			}
			return energyProduction;
		}

		public int EnergyRequired()
		{
			int energyRequired = 0;
			foreach (ModuleStack moduleStack in this.Values)
			{
				energyRequired += moduleStack.EnergyRequired;
				energyRequired += moduleStack.ModuleStacks.EnergyRequired();
			}
			return energyRequired;
		}


		public int CrewCurrent()
		{
			int crewCurrent = 0;
			foreach (ModuleStack moduleStack in this.Values)
			{
				crewCurrent += moduleStack.CrewCurrent;
				crewCurrent += moduleStack.ModuleStacks.CrewCurrent();
			}
			return crewCurrent;
		}

		public int CrewRequired()
		{
			int crewRequired = 0;
			foreach (ModuleStack moduleStack in this.Values)
			{
				crewRequired += moduleStack.CrewRequired;
				crewRequired += moduleStack.ModuleStacks.CrewRequired();
			}
			return crewRequired;
		}

		public double Mass
		{
            get
            {
                double mass = 0;
                foreach (ModuleStack moduleStack in this.Values)
                {
                    mass += moduleStack.Mass;
                    mass += moduleStack.ModuleStacks.Mass;
                }
                return mass;
            }
		}

        // total value of Mass Capacity for space move mode in the stack
        public double MassCapacity
        {
            get
            {
                double massCapacity = 0;
                foreach (ModuleStack moduleStack in this.Values)
                {
                    if (moduleStack.MoveModes.ContainsKey(EMoveMode.space))
                    {
                        massCapacity += moduleStack.MoveModes[EMoveMode.space].MassCapacity;
                    }
                    massCapacity += moduleStack.ModuleStacks.MassCapacity;
                }
                return massCapacity;
            }
        }

		public int HitPoints()
		{
			int hitPoints = 0;
			foreach (ModuleStack moduleStack in this.Values)
			{
				hitPoints += moduleStack.HitPoints;
				hitPoints += moduleStack.ModuleStacks.HitPoints();
			}
			return hitPoints;
		}

		public int Damage()
		{
			int damage = 0;
			foreach (ModuleStack moduleStack in this.Values)
			{
				damage += moduleStack.Damage;
				damage += moduleStack.ModuleStacks.Damage();
			}
			return damage;
		}

		public int Attack()
		{
			int attack = 0;
			foreach (ModuleStack moduleStack in this.Values)
			{
				attack += moduleStack.Attack;
				attack += moduleStack.ModuleStacks.Attack();
			}
			return attack;
		}

		public int Defense()
		{
			int defense = 0;
			foreach (ModuleStack moduleStack in this.Values)
			{
				defense += moduleStack.Defense;
				defense += moduleStack.ModuleStacks.Defense();
			}
			return defense;
		}

		public int InitiativeBonus()
		{
			int initiativeBonus = 0;
			foreach (ModuleStack moduleStack in this.Values)
			{
				initiativeBonus += moduleStack.InitiativeBonus;
				initiativeBonus += moduleStack.ModuleStacks.InitiativeBonus();
			}
			return initiativeBonus;
		}

		public ItemStacks Upkeep
		{
			get
			{
				ItemStacks upkeepItemStacks = new ItemStacks();
				foreach (ModuleStack moduleStack in this.Values)
				{					
					upkeepItemStacks.Sum(moduleStack.Upkeep);				
				}
				return upkeepItemStacks;
			}
		}

		public ItemStacks Consume
		{
			get
			{
				ItemStacks consumeItemStacks = new ItemStacks();
				foreach (ModuleStack moduleStack in this.Values)
				{
					consumeItemStacks.Sum(moduleStack.Consume);
				}
				return consumeItemStacks;
			}
		}
		#region IReporting Members

		public List<string> Report(Faction faction)
		{
			return this.Report(faction, 0);
		}

		public List<string> Report(Faction faction, int level)
		{
			List<string> lines = new List<string>();
			foreach (ModuleStack moduleStack in this.Values)
			{					
				if (moduleStack.Quantity > 1)
				{
					lines.Add(string.Format("(0) {1}, {2} {3}.",
						(moduleStack.Owner == faction) ? "+" : "-",
						moduleStack.ReportName,
						moduleStack.Quantity,
						moduleStack.ModuleType.ReportNameMultiple));
				}
				else
				{
					lines.Add(string.Format("(0) {1}, {2} {3}.",
						(moduleStack.Owner == faction) ? "+" : "-",
						moduleStack.ReportName,
						moduleStack.ModuleType.ReportName));
				}
			}				
			ReportLines reportLines = new ReportLines();
			reportLines.Add(lines, level);
			return reportLines.IndentedLines;
		}

		public string ReportList
		{
			get
			{
				string line = "";
				bool firstAdded = false;
				foreach (ModuleStack moduleStack in this.Values)
				{
					line = string.Format("{0}{1}", line, (firstAdded) ? ", " : "");
					firstAdded = true;
					
					line = string.Format("{0}{1}", line, moduleStack.ReportName);
				}
				return line;
			}
		}

		#endregion

		public void AddEvent(int week, string eventDescription)
		{
			foreach (ModuleStack moduleStack in this.Values)
			{
				moduleStack.EventReports.Add(week, eventDescription);
			}
		}

		public void Add(ModuleStacks list)
		{
			foreach (ModuleStack moduleStack in list.Values)
			{
				this.Add(moduleStack);
			}
		}

		public void Add(ModuleStack moduleStack)
		{
			if (!this.ContainsKey(moduleStack.Name))
			{
				this.Add(moduleStack.Name, moduleStack);
			}
		}

		public void Remove(ModuleStack moduleStack)
		{
			if (!this.ContainsKey(moduleStack.Name))
			{
				throw new Exception("Modulestacks do not contain " + moduleStack.ReportName);
			}
			else
			{
				base.Remove(moduleStack.Name);
			}
		}

		private ModuleStack getAvoiding()
		{
			foreach (ModuleStack moduleStack in this.Values)
			{
				if (moduleStack.IsAvoiding)
				{
					return moduleStack;
				}
			}
			return null;
		}

		public void RemoveAvoiding()
		{
			ModuleStack modulestack = this.getAvoiding();
			while (modulestack != null)
			{
				this.Remove(modulestack);
				modulestack = this.getAvoiding();
			}
		}

		private ModuleStack getUnformed()
		{
			foreach (ModuleStack moduleStack in this.Values)
			{
				if (!moduleStack.IsFormed)
				{
					return moduleStack;
				}
			}
			return null;
		}

		public void RemoveUnformed()
		{	
			ModuleStack modulestack = this.getUnformed();
			while (modulestack != null)
			{
				this.Remove(modulestack);
				modulestack = this.getUnformed();
			}
		}

        public static int CompareByNames(ModuleStack moduleStack1, ModuleStack moduleStack2)
        {
            return String.Compare(moduleStack1.Name, moduleStack2.Name);
        }


        public void LoadXml(XmlElement elHolder, IHolder holder)
        {
            ModuleStack moduleStack;

            foreach (XmlElement elModuleStack in elHolder.SelectNodes("modulestack"))
            {
                moduleStack = ModuleStack.All.GetOrCreateNewModuleStack(
                    Faction.All[elModuleStack.GetAttribute("faction")],
                    elModuleStack.GetAttribute("name"),
					true);
                moduleStack.Parent = holder;
                moduleStack.LoadXml(elModuleStack);
            }
        }

        public XmlElement SaveXml(XmlDocument doc, XmlElement elHolder, Faction faction = null)
        {
            XmlElement elModuleStack;
            foreach (ModuleStack moduleStack in this.Values)
            {
                if (!moduleStack.Visible(faction))
                    continue;

                elModuleStack = moduleStack.SaveXml(doc, faction);
                elHolder.AppendChild(elModuleStack);

                moduleStack.ModuleStacks.SaveXml(doc, elModuleStack, faction);
            }

            return elHolder;
        }
	}
}
