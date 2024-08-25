using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Location : NamedObject, IHolder
	{
		public Location(string name)
			: base(name) 
		{
		}

        public virtual string BattleReportName
        {
            get
            {
                return string.Format("{0} of {1}", this.ReportName, this.Parent.BattleReportName);
            }
        }

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
        
        public Orders Orders
		{
			get { throw new NotImplementedException(); }
		}

		public Faction Owner
		{
			get
            {
                // TODO: implement location ownership 
                return null;
            }
		}

		public bool ExecutedLongOrder
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

        public bool Execute(int week)
        {
            bool executed;

            executed = this.Orders.Execute(week);
            this.Orders.RemoveExecuted();
            //this.Effects.Execute(week);
            //this.Effects.RemoveExecuted();

            return executed;
        }

        protected ILocationsHolder locationParent = null;
        public virtual ILocationsHolder LocationParent
        {
            get { return this.locationParent; }
            set { this.locationParent = value; }
        }

        
        protected IHolder parent = null;
        public virtual IHolder Parent
        {
            get { return this.parent; }
            set { this.parent = value; }
        }

        public virtual bool IsLocation
        {
            get { return true; }
        }

        public virtual ELocationType LocationType
        {
            get { return ELocationType.solidSurface; }
        }

        private Exits exits = new Exits();
        public Exits Exits
        {
            get { return this.exits; }
        }


        public virtual double Capacity
        {
            get { throw new NotImplementedException(); }
        }

        public virtual double CapacityUsed
        {
            get { throw new NotImplementedException(); }
        }


        public ModuleStacks ModuleStacks
        {
            get { return ModuleStack.All[this]; }
        }

        public bool HasModuleStacks(ModuleType moduleType = null)
        {
            if (this.ModuleStacks.Count > 0)
            {
                foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
                {
                    return moduleStack.HasModuleStacks(moduleType);
                }
            }
            return false;
        }

        public People People
        {
            get { return Person.All[this]; }
        }

        public bool HasPeople
        {
            get
            {
                if (this.People.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }

        private EventReports eventReports = new EventReports();
        public EventReports EventReports
        {
            get { return this.eventReports; }
        }

        public virtual List<string> Report(Faction faction)
        {
            throw new NotImplementedException();
        }

		public bool HasPresence(Faction faction)
		{
			foreach (Person person in this.People.Values)
			{
				if (person.Owner == faction)
				{
					return true;
				}
			}

			foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
			{
				if (moduleStack.HasPresence(faction) == true)
				{
					return true;
				}
			}

			return false;
		}

		private Market market = new Market();
		public Market Market
		{
			get { return this.market; }
		}


        public int TechnologyCapacity
        {
            get { return 0; }
        }

        public int TechnologyCapacityUsed
        {
            get { return 0; }
        }

    }
}
