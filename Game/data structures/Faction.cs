using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

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
		// Baseline stances: neutral toward unspecified interests, hostile toward unknown affiliation.
		public FactionAttitude DefaultAttitude = FactionAttitude.Neutral;
		public FactionAttitude UnknownAttitude = FactionAttitude.Hostile;
		public FactionAttitudes Attitudes = new FactionAttitudes();     // per-faction declared stances (keyed by faction name)
		public FactionAttitudes UnitAttitudes = new FactionAttitudes(); // per-unit declared stances (keyed by modulestack name)

		// Stance toward a faction: an explicit declaration, else the default stance.
		public FactionAttitude AttitudeToward(Faction faction)
		{
			if (faction == null)
			{
				return this.UnknownAttitude;
			}
			if (this.Attitudes.ContainsKey(faction.Name))
			{
				return this.Attitudes[faction.Name];
			}
			return this.DefaultAttitude;
		}

		// Stance toward a specific unit: an explicit per-unit declaration, else the stance toward its owner.
		public FactionAttitude AttitudeTowardUnit(ModuleStack unit)
		{
			if (unit == null)
			{
				return this.UnknownAttitude;
			}
			if (this.UnitAttitudes.ContainsKey(unit.Name))
			{
				return this.UnitAttitudes[unit.Name];
			}
			return this.AttitudeToward(unit.Owner);
		}

		// Drop per-unit stances whose target is gone, empty after capture, or now owned by this faction.
		public void DropStaleUnitAttitudes()
		{
			List<string> stale = new List<string>();
			foreach (string key in this.UnitAttitudes.Keys)
			{
				if (!ModuleStack.All.ContainsKey(key))
				{
					stale.Add(key);
					continue;
				}
				ModuleStack unit = ModuleStack.All[key];
				if (unit.Owner == this || unit.Quantity < 1)
				{
					stale.Add(key);
				}
			}
			foreach (string key in stale)
			{
				this.UnitAttitudes.Remove(key);
			}
		}

		// Report of this faction's declared stances. Empty at the baseline (neutral
		// default, hostile unknown, no explicit declarations) so unchanged games are unaffected.
		public List<string> ReportDeclarations()
		{
			List<string> lines = new List<string>();
			if (this.Attitudes.Count == 0 && this.UnitAttitudes.Count == 0
				&& this.DefaultAttitude == FactionAttitude.Neutral
				&& this.UnknownAttitude == FactionAttitude.Hostile)
			{
				return lines;
			}

			lines.Add("Declared stances:");
			lines.Add(string.Format("  default: {0}, unknown: {1}.",
				this.DefaultAttitude.ToString().ToLower(),
				this.UnknownAttitude.ToString().ToLower()));

			foreach (KeyValuePair<string, FactionAttitude> declaration in this.Attitudes)
			{
				string target = Faction.All.ContainsKey(declaration.Key)
					? Faction.All[declaration.Key].ReportName
					: string.Concat("faction ", declaration.Key);
				lines.Add(string.Format("  {0} toward {1}.", declaration.Value.ToString().ToLower(), target));
			}
			foreach (KeyValuePair<string, FactionAttitude> declaration in this.UnitAttitudes)
			{
				string target = ModuleStack.All.ContainsKey(declaration.Key)
					? ModuleStack.All[declaration.Key].ReportName
					: string.Concat("unit ", declaration.Key);
				lines.Add(string.Format("  {0} toward {1}.", declaration.Value.ToString().ToLower(), target));
			}
			return lines;
		}

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
				//            Write("Events during turn:|˜˜˜˜˜˜˜ ˜˜˜˜˜ ˜˜˜˜:");
				//foreach (Event obj in f.Events) 
				//    Write(obj.ToString(lng));
				//Write("");

			
			// Technology reports are written after the market section (see ReportWriter).

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

			// declared diplomatic stances (before the bank report)
			List<string> declarations = this.ReportDeclarations();
			reportLines.AddRange(declarations);
			if (declarations.Count > 0)
			{
				reportLines.Add("");
			}

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

		public override void LoadXml(XmlElement elFaction)
		{
			base.LoadXml(elFaction);
			this.Password = elFaction.GetAttribute("password");
			this.Email = elFaction.GetAttribute("email");
			if (elFaction.HasAttribute("default-attitude"))
				this.DefaultAttitude = (FactionAttitude)Convert.ToInt32(elFaction.GetAttribute("default-attitude"));
			if (elFaction.HasAttribute("unknown-attitude"))
				this.UnknownAttitude = (FactionAttitude)Convert.ToInt32(elFaction.GetAttribute("unknown-attitude"));
			this.Options.TextReport = this.XMLAssignBoolean(elFaction.GetAttribute("text-report"), true);
			this.Options.ReportLineLength = this.XMLAssignInteger(elFaction.GetAttribute("text-report-line-length"), ReportLine.LineLength);
			this.Options.XmlReport = this.XMLAssignBoolean(elFaction.GetAttribute("xml-report"), true);

			this.Bank.Balance = this.XMLAssignDouble(elFaction.GetAttribute("balance"), 0);
			this.Bank.CreditLine = this.XMLAssignInteger(elFaction.GetAttribute("credit-line"), 0);
			this.Bank.CreditRate = this.XMLAssignDouble(elFaction.GetAttribute("credit-rate"), 0);
			this.Bank.DepositRate = this.XMLAssignDouble(elFaction.GetAttribute("deposit-rate"), 0);

			foreach (XmlElement elTechnology in elFaction.SelectNodes("technology"))
			{
				string technologyName = elTechnology.GetAttribute("name");
				if (Technology.All.Contains(technologyName))
				{
					this.TechnologiesSeen.Add(Technology.All[technologyName]);
				}
			}

			foreach (XmlElement elAttitude in elFaction.SelectNodes("attitude"))
			{
				FactionAttitude attitude = FactionAttitudeParser.Parse(elAttitude.GetAttribute("attitude"));
				if (elAttitude.HasAttribute("faction"))
				{
					this.Attitudes[elAttitude.GetAttribute("faction")] = attitude;
				}
				else if (elAttitude.HasAttribute("unit"))
				{
					this.UnitAttitudes[elAttitude.GetAttribute("unit")] = attitude;
				}
			}
		}

		public override XmlElement SaveXml(XmlDocument doc)
		{
			XmlElement elFaction = doc.CreateElement("faction");
			elFaction.SetAttribute("name", this.Name);
			elFaction.SetAttribute("name-en", this.FullName);
			elFaction.SetAttribute("password", this.Password);
			elFaction.SetAttribute("email", this.Email);
			elFaction.SetAttribute("default-attitude", ((int)this.DefaultAttitude).ToString());
			if (this.UnknownAttitude != FactionAttitude.Hostile)
			{
				elFaction.SetAttribute("unknown-attitude", ((int)this.UnknownAttitude).ToString());
			}
			elFaction.SetAttribute("text-report", this.Options.TextReport.ToString());
			elFaction.SetAttribute("text-report-line-length", this.Options.ReportLineLength.ToString());
			elFaction.SetAttribute("xml-report", this.Options.XmlReport.ToString());
			elFaction.SetAttribute("balance", this.Bank.Balance.ToString());
			elFaction.SetAttribute("credit-line", this.Bank.CreditLine.ToString());
			elFaction.SetAttribute("credit-rate", this.Bank.CreditRate.ToString());
			elFaction.SetAttribute("deposit-rate", this.Bank.DepositRate.ToString());

			foreach (Technology technology in this.TechnologiesSeen)
			{
				XmlElement elTechnology = doc.CreateElement("technology");
				elTechnology.SetAttribute("name", technology.Name);
				elFaction.AppendChild(elTechnology);
			}

			foreach (KeyValuePair<string, FactionAttitude> declaration in this.Attitudes)
			{
				XmlElement elAttitude = doc.CreateElement("attitude");
				elAttitude.SetAttribute("faction", declaration.Key);
				elAttitude.SetAttribute("attitude", FactionAttitudeParser.ToToken(declaration.Value));
				elFaction.AppendChild(elAttitude);
			}
			foreach (KeyValuePair<string, FactionAttitude> declaration in this.UnitAttitudes)
			{
				XmlElement elAttitude = doc.CreateElement("attitude");
				elAttitude.SetAttribute("unit", declaration.Key);
				elAttitude.SetAttribute("attitude", FactionAttitudeParser.ToToken(declaration.Value));
				elFaction.AppendChild(elAttitude);
			}

			return elFaction;
		}
	}
}
