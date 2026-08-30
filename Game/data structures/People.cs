using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class People : Dictionary<string, Person>
	{
        public new Person this[string token]
        {
            get
            {
                Person found = null;
                foreach (Person person in this.Values)
                {
                    if (person.Name == token | person.Alias == token)
                    {
                        if (found != null)
                        {
                            throw new Exception("Double find of person. Tried to lookup by name or alias [" + token + "].");
                        }
                        found = person;
                    }
                }
                return found;
            }
        }

        public People this[IHolder holder, bool recursive = false]
        {
            get
            {
                People list = new People();
                foreach (Person person in this.Values)
                {
                    if (person.Parent == holder)
                    {
                        list.Add(person.Name, person);
                    }
                }
                if (recursive)
                {
                    foreach (ModuleStack moduleStack in holder.ModuleStacks.Values)
                    {
                        list.Add(Person.All[moduleStack, true]);
                    }
                }
                return list;
            }
        }

		public People this[Faction faction]
		{
			get
			{
				People list = new People();
				foreach (Person person in this.Values)
				{
					if (person.Owner == faction)
					{
						list.Add(person.Name, person);
					}
				}
				return list;
			}
		}

        // when searching by alias a format of new1 is expected not 2_new1
        public Person this[Faction faction, string name, bool byAlias = false]
        {
            get
            {
                Person found = null;
                if (!byAlias)
                {
                    foreach (Person person in this[faction].Values)
                    {
                        if (person.Name == name)
                        {
                            found = person;
                        }
                    }
                }
                else
                {
                    foreach (Person person in this[faction].Values)
                    {
                        if (person.Alias == string.Format("{0}_{1}", faction.Name, name))
                        {
                            found = person;
                        }
                    }
                }
                return found;
            }
        }

        public Person this[string name, bool byAlias = false]
        {
            get
            {
                Person found = null;
                if (!byAlias)
                {
                    foreach (Person person in this.Values)
                    {
                        if (person.Name == name)
                        {
                            found = person;
                        }
                    }
                }
                else
                {
                    foreach (Person person in this.Values)
                    {
                        if (person.Alias == name)
                        {
                            found = person;
                        }
                    }
                }
                return found;
            }
        }

        public Person GetOrCreateNewPerson(Faction owner, string name)
        {
            return this.GetOrCreateNewPerson(owner, name, false);
        }

        public Person GetOrCreateNewPerson(Faction owner, string name, bool onlyOwned = false)
        {
            // try to find exact match
            Person person = this[owner, name];

            // okay, try to find a name match for other faction
            if (!onlyOwned & person == null)
            {
                person = this[name];
            }

            // okay we still don't have it, lets check if it wasn't a modulestack not a person
            if (person == null)
            {
                ModuleStack moduleStack = ModuleStack.All[owner, name];
                if (moduleStack != null)
                {
                    throw new TypeInitializationException(
                        string.Concat(
                            "Tried to find person with name ", name, " while there is already modulestack with same name."), null);
                }

                moduleStack = ModuleStack.All[owner, name, true];
                if (moduleStack != null)
                {
                    throw new TypeInitializationException(
                        string.Concat(
                            "Tried to find person with name ", name, " while there is already modulestack with same name."), null);
                }
            }

            // okay, we still don't have it, but perhaps it was an alias
            if (person == null)
            {
                // faction match
                person = this[owner, name, true];

                if (person == null)
                {
                    // let's check a no faction alias match
                    person = this[name, true];
                }
            }

            // okay, no choice, we need to create a new one
            if (person == null)
            {
                person = new Person(owner, name);
            }

            // we have to have it now
            return person;
        }

        public People HavingOrders
        {
            get
            {
                People havingOrders = new People();

                foreach (Person person in this.Values)
                {
                    if (person.HasOrders)
                    {
                        havingOrders.Add(person.Name, person);
                    }
                }
                return havingOrders;
            }
        }
        
        public double Mass()
		{
			double mass = 0;
			foreach (Person person in this.Values)
			{
				mass += person.Mass;
			}
			return mass;
		}

		public double Size()
		{
			double size = 0;
			foreach (Person person in this.Values)
			{
				size += person.Race.Size;
			}
			return size;
		}

		public int CombatAttack(ModuleStack root)
		{
			int attack = 0;
			foreach (Person person in this.Values)
			{
				attack += person.CombatAttack(root);
			}
			return attack;
		}

		public int CombatDefense(ModuleStack root)
		{
			int defense = 0;
			foreach (Person person in this.Values)
			{
				defense += person.CombatDefense(root);
			}
			return defense;
		}

		public int CombatInitiative(ModuleStack root)
		{
			int initiative = 0;
			foreach (Person person in this.Values)
			{
				initiative += person.CombatInitiative(root);
			}
			return initiative;
		}

		public int CureChance(ModuleStack root)
		{
			int max = 0;
			foreach (Person person in this.Values)
			{
				int chance = person.CureChance(root);
				if (chance > max)
				{
					max = chance;
				}
			}
			return max;
		}

		public int ResearchOutputBonus(ModuleStack host)
		{
			int bonus = 0;
			foreach (Person person in this.Values)
			{
				bonus += person.ResearchOutputBonus(host);
			}
			return bonus;
		}

		public int CountBattleSkilled
		{
			get
			{
				int count = 0;
				foreach (Person person in this.Values)
				{
					if (person.IsBattleSkilled)
					{
						count++;
					}
				}
				return count;
			}
		}

		public ItemStacks Upkeep
		{
			get
			{
				ItemStacks upkeepItemStacks = new ItemStacks();
				foreach (Person person in this.Values)
				{
					upkeepItemStacks.Sum(person.UpkeepNetto);
					upkeepItemStacks.Sum(person.ItemStacks.Upkeep);
				}
				return upkeepItemStacks;
			}
		}

		public ItemStacks Consume
		{
			get
			{
				ItemStacks consumeItemStacks = new ItemStacks();
				foreach (Person person in this.Values)
				{
					consumeItemStacks.Sum(person.ConsumeNetto);
					consumeItemStacks.Sum(person.ItemStacks.Consume);
				}
				return consumeItemStacks;
			}
		}

        public void Add(People list)
        {
            foreach (Person person in list.Values)
            {
                if (!this.ContainsKey(person.Name))
                {
                    this.Add(person.Name, person);
                }
            }
        }

		public void Remove(Person person)
		{
			if (!this.ContainsKey(person.Name))
			{
				throw new Exception("People do not contain " + person.ReportName);
			}
			else
			{
				base.Remove(person.Name);
			}
		}

		public void RemoveUnformed()
		{
			foreach (Person person in this.Values)
			{
				if (person.IsFormed)
				{
					this.Remove(person);
				}
			}
		}

        public bool Contains(string name)
        {
            bool found = false;
            foreach (Person person in this.Values)
            {
                if (person.Name == name)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }

        public static int CompareByNames(Person person1, Person person2)
        {
            return String.Compare(person1.Name, person2.Name);
        }

        public void LoadXml(XmlElement elHolder, IHolder holder)
        {
            Person person;

            foreach (XmlElement elPerson in elHolder.SelectNodes("person"))
            {
                person = Person.All.GetOrCreateNewPerson(
                    Faction.All[elPerson.GetAttribute("faction")], 
                    elPerson.GetAttribute("name"));
                person.Parent = holder;
                person.LoadXml(elPerson);
            }
        }

        public XmlElement SaveXml(XmlDocument doc, XmlElement elHolder, Faction faction = null)
        {
            XmlElement elPerson;             

            foreach (Person person in this.Values)
            {
                if (!person.Visible(faction))
                    continue;

                elPerson = person.SaveXml(doc);
                elHolder.AppendChild(elPerson);
            }

            return elHolder;
        }
	}
}
