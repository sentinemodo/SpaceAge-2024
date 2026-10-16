using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceAge
{
	public partial class Battle : IReporting
	{
		private int week = 0;
		public int Week
		{
			get { return this.week; }
			set { this.week = value; }
		}

		public static Battles All = new Battles();


		public const int MaxRounds = 10;

		private int round = 1;
		public int Round
		{
			get { return this.round; }
			set { this.round = value; }
		}

		private ModuleStack attacker = null;
		private ModuleStack battleDefender = null;
		public ModuleStack Attacker
		{
			get { return this.attacker; }
			set { this.attacker = value; }
		}

		private ModuleStacks attackers = null;
		public ModuleStacks Attackers
		{
			get { return this.attackers; }
			set { this.attackers = value; }
		}

		private ModuleStacks defenders = null;
		public ModuleStacks Defenders
		{
			get { return this.defenders; }
			set { this.defenders = value; }
		}

		private List<Faction> participants = new List<Faction>();
		

		private Dictionary<ModuleStack, int> unhitRounds = new Dictionary<ModuleStack, int>();
		private List<ModuleStack> hitThisRound = new List<ModuleStack>();
		private HashSet<string> pluralFireHeadersThisRound = new HashSet<string>();
		private List<Module> pendingCaptures = new List<Module>();
		private List<ModuleStack> launchedHangarCraft = new List<ModuleStack>();
		private List<ModuleStack> hangarLaunchCarriers = new List<ModuleStack>();


		public void executeMovement()
		{

		}

		private bool concluded;

		public void Execute(int week)
		{
			this.concluded = false;
			this.week = week;
			this.resetCaptureDamage();

			this.prepareBattleReports();
			this.announceLocationEvent(week);

			this.attacker.EventReports.Add(week, string.Format("initiated battle against {0}", defenders.ReportList));
			this.attackers.AddEvent(week, "engaged in battle");
			this.defenders.AddEvent(week, "engaged in battle");

			string locationName = this.attacker.Location != null ? this.attacker.Location.Name : string.Empty;
			string defenderName = this.battleDefender != null ? this.battleDefender.Name : string.Empty;
			int battleSeed = Sequence.ComputeBattleSeed(week, locationName, this.attacker.Name, defenderName);
			Sequence.EnterBattleScope(battleSeed);
			try
			{
			this.round = 0;

			while (!this.concluded)
			{
				this.round++;

				this.report(string.Format ("Round {0}:", this.round));
				this.report("------------------------------------------------------------");
				
				foreach (Faction faction in this.participants) 
				{
					// attackers list
					this.report(faction, "Attackers:");
					foreach (ModuleStack modulestack in this.attackers.Values)
					{
						this.report(faction, modulestack.BattleReport(faction));
					}
					// defenders list
					this.report(faction, "Defenders:");
					foreach (ModuleStack modulestack in this.defenders.Values)
					{
						this.report(faction, modulestack.BattleReport(faction));
					}
				}
				if (this.round == 1)
				{
					this.launchHangarCraft();
				}
				this.report("------------------------------------------------------------");
				this.hitThisRound.Clear();
				this.pluralFireHeadersThisRound.Clear();
				
				// decide superiorities and bonuses
				// validate participating modulestacks (remove destroyed, disabled)
				SortedList<int, List<ModuleStack>> sorted = new SortedList<int, List<ModuleStack>>();
				foreach (ModuleStack moduleStack in this.attackers.Values)
				{
					if (!sorted.ContainsKey(moduleStack.Initiative)) 
					{
						sorted.Add(moduleStack.Initiative, new List<ModuleStack>());
					}
					sorted[moduleStack.Initiative].Add(moduleStack);
				}
				foreach (ModuleStack moduleStack in this.defenders.Values)
				{
					if (!sorted.ContainsKey(moduleStack.Initiative))
					{
						sorted.Add(moduleStack.Initiative, new List<ModuleStack>());
					}
					sorted[moduleStack.Initiative].Add(moduleStack);
				}
				foreach (List<ModuleStack> modulestacks in sorted.Values)
				{
					// resolve individual modulestacks initiative
					foreach (ModuleStack modulestack in modulestacks)
					{
						if (modulestack.HasOperationalModules)
						{
							this.executeAttack(modulestack);
							this.considerRetreat(modulestack);
						}
					}
				}
				this.report("------------------------------------------------------------");
				this.processEvadeLeaves();
				// execute orders from battle sequence - mainly move, attack, activate, deactivate, 
				//   but also use of tactical technologies or personal equipment
				// resolve all movements
				if (this.round >= Battle.MaxRounds | this.attackers.Count == 0 | this.defenders.Count == 0)
				{
					this.concluded = true;
					if (this.attackers.Count > 0 & this.defenders.Count == 0)
					{
						this.report("Battle won by attackers.");
					}
					else if (this.attackers.Count == 0 & this.defenders.Count > 0)
					{
						this.report("Battle won by defenders.");
					}
					else
					{
						this.report("Battle ended indecisively.");					
					}
				}
			}
			this.ApplyCaptures(week);
			this.ApplyVictoryResolution(week);
			}
			finally
			{
				Sequence.ExitBattleScope();
			}
		}




		private Dictionary<Faction, ReportLines> battleReports = new Dictionary<Faction, ReportLines>();
		private Dictionary<Faction, ReportLines> observerReports = new Dictionary<Faction, ReportLines>();

	}
}
