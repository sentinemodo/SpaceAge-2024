using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public class Faction : NamedObject, IOrderable, IReporting, IEventReporting
	{
		public static Factions All = new Factions();

		public Faction(string name, string fullName)
			: base(name)
		{
			if (Faction.All.ContainsKey(name))
				throw new Exception("Faction with name " + name + " already exists");

			Faction.All.Add(this.name, this);
			this.FullName = fullName;
			this.bank = new Bank(this);
		}

		// identification settings
		private string id;
		public string Id
		{
			get { return this.id; }
			set { this.id = value; }
		}

		private string password;
		public string Password
		{
			get { return this.password; }
			set { this.password = value; }
		}

		private string email;
		public string Email
		{
			get { return this.email; }
			set { this.email = value; }
		}

		public FactionOptions Options = new FactionOptions();
		
		// diplomatic settings
		public FactionAttitude DefaultAttitude = FactionAttitude.Neutral;
		public FactionAttitudes Attitudes = new FactionAttitudes();

		// property settings
		//public PersonList Persons = new PersonList();
		//public EventList Events = new EventList();

		#region IOrderable Members

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

		#region reporting

		private Technologies technologiesSeen = new Technologies();
		public Technologies TechnologiesSeen
		{
			get { return this.technologiesSeen; }
		}

		private Technologies technologiesToShow = new Technologies();
		public Technologies TechnologiesToShow
		{
			get { return this.technologiesToShow; }
		}

		private ModuleTypes moduleTypesSeen = new ModuleTypes();
		public ModuleTypes ModuleTypesSeen
		{
			get { return this.moduleTypesSeen; }
		}

		private ModuleTypes moduleTypesToShow = new ModuleTypes();
		public ModuleTypes ModuleTypesToShow
		{
			get { return this.moduleTypesToShow; }
		}

		#region IReporting Members

		public List<string> Report(Faction faction)
		{
			return this.Report();
		}

		public List<string> Report()
		{
			List<string> reportLines = new List<string>();
			// faction creditline

			// faction attitudes
			//this.Write(String.Format("Declared Attitudes (default {0}):",
			//        f.DefaultAttitude.ToString()));
			//    for (Attitude a = Attitude.Hostile; a <= Attitude.Ally; a++) 
			//        Write(a + " : " + AttitudeListString(f, a));
			//    Write("");

			// faction events reports
				//            Write("Events during turn:|События этого хода:");
				//foreach (Event obj in f.Events) 
				//    Write(obj.ToString(lng));
				//Write("");

			
			// Technology reports
			if (this.technologiesToShow.Count > 0) 
			{
				reportLines.Add("Technology reports:");
				reportLines.Add("");
				foreach (Technology technology in this.TechnologiesToShow) 
				{
					//this.WriteTechnologyReport(technology);
				}
			}

			// ModuleTypes reports
			if (this.moduleTypesToShow.Count > 0) 
			{
				reportLines.Add("Module reports:");
				reportLines.Add("");
				foreach (ModuleType moduletype in this.moduleTypesToShow.Values) 
				{
					//this.WriteModuleTypeReport(moduleType);
				}
			}

			// item types report
			// skill types report
			// space objects types report

			reportLines.AddRange(this.Bank.Report(this));

			// this.AllShown();
			return reportLines;
		}

		#endregion

		#endregion

		public void AllShown()
		{
			foreach (Technology technology in this.technologiesToShow)
			{
				if (!this.technologiesSeen.Contains(technology))
				{
					this.technologiesSeen.Add(technology);
				}
			}
			this.technologiesToShow.Clear();

			foreach (ModuleType moduleType in this.moduleTypesToShow.Values)
			{
				if (!this.moduleTypesSeen.ContainsKey(moduleType.Name))
				{
					this.moduleTypesSeen.Add(moduleType.Name, moduleType);
				}
			}
			this.moduleTypesToShow.Clear();
		}

		#region market
		public double NetWorth()
		{
			return 0;
		}

		private Bank bank = null;
		public Bank Bank
		{
			get { return this.bank; }
			set { this.bank = value; }
		}
        #endregion

        #region research
        
        public int MaxTechnologyLevel
        {
            get { return this.TechnologiesSeen.MaxTechnologyLevel; }
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

		public Faction Owner
		{
			get { return this; }
		}
        
        #endregion

		#region IEventReporting Members

		private EventReports eventReports = new EventReports();
		public EventReports EventReports
		{
			get { return this.eventReports; }
		}

		#endregion
	}
}
