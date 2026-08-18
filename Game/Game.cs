using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Threading;

namespace SpaceAge
{
	public class Game
	{
		public Game()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
		}

		#region configuration

		private StarTypes starTypes = new StarTypes();
		public StarTypes StarTypes 
		{
			get { return this.starTypes; }
		}

		private PlanetTypes planetTypes = new PlanetTypes();
		public PlanetTypes PlanetTypes
		{
			get { return this.planetTypes; }
		}

		private MoonTypes moonTypes = new MoonTypes();
		public MoonTypes MoonTypes
		{
			get { return this.moonTypes; }
		}

        private RegionTypes regionTypes = new RegionTypes();
        public RegionTypes RegionTypes
		{
            get { return this.regionTypes; }
		}

		public ItemTypes ItemTypes
		{
			get { return ItemType.All; }
		}

		public Races Races
		{
			get { return Race.All; }
		}

		public Technologies Technologies
		{
			get { return Technology.All; }
		}
		
		public SkillTypes SkillTypes
		{
			get { return SkillType.All; }
		}

		public ModuleTypes ModuleTypes
		{
			get { return ModuleType.All; }
		}

		#endregion

		#region turn data

		private int startingYear = 2020;

		private int turn;
		public int Turn
		{
			get { return this.turn; }
			set { this.turn = value; }
		}

		private int week = 1;
		public int Week
		{
			get { return this.week; }
			set { this.week = value; }
		}

		public string Date
		{
			get 
			{
				string month = string.Empty;
				switch (this.turn % 4) 
				{
					case 1:
						month = "January";
						break;
					case 2:
						month = "April";
						break;
					case 3:
						month = "July";
						break;
					default:
						month = "September";
						break;
				}

				return string.Format("Year {0}, {1} 1", Convert.ToInt32(this.startingYear + this.turn / 4), month);
			}
		}

		private Events events = new Events();
		public Events Events
		{
			get { return this.events; }
		}

		public Factions Factions
		{
			get { return Faction.All; }
		}

		private Galaxy galaxy = new Galaxy();
		public Galaxy Galaxy
		{
			get { return this.galaxy; }
		}

		public Regions Regions
		{
			get { return Region.All; }
		}

		public ModuleStacks ModuleStacks
		{
			get { return ModuleStack.All; }
		}

		public People People
		{
			get { return Person.All; }
		}

        public Offers Offers
        {
            get { return Offer.All; }
        }
        
        #endregion

		public void Execute()
		{			
			this.ClearEventReports();
			this.turn++;
            #region execute orders
            for (this.week = 1; this.week <= 13; this.week++)
			{
				this.ClearExecutedLongOrder();
				//this.ClearFailedToExecuteImmediateOrders();
				this.ClearExecutedImmediateOrders();
				this.ExecuteOrders();
				this.ExecuteMedicalConsume();
				Contract.All.Evaluate(this.week);
                this.ProcessBuyOffers();
				this.ExecuteBattles();
			}
			this.ExecuteQuarterlyWoundedOutcome();
			this.ClearExecutedLongOrder();
			//this.ClearFailedToExecuteImmediateOrders();
			this.ClearExecutedImmediateOrders();
			this.ClearUnformed();
            #endregion
            this.UpdateBankAccounts();
            this.UpdateRates();
            this.GenerateOffers();
        }

		public void ClearEventReports()
		{
			foreach (Faction faction in this.Factions.Values)
			{
				faction.EventReports.Clear();
			}
			foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
			{
				moduleStack.EventReports.Clear();
			}
			foreach (Person person in this.People.Values)
			{
				person.EventReports.Clear();
			}
			foreach (Region region in this.Regions.Values)
			{
				region.EventReports.Clear();
			}
			foreach (Orbit orbit in Orbit.All.Values)
			{
				orbit.EventReports.Clear();
			}
		}

		public void ClearExecutedImmediateOrders()
        {
            foreach (Faction faction in this.Factions.Values)
            {
                foreach (ImmediateOrder order in faction.Orders.Immediate)
                {
                    order.Executed = false;
                }
            }
            foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
            {
                foreach (ImmediateOrder order in moduleStack.Orders.Immediate)
                {
                    order.Executed = false;
                }
            }
						foreach (Person person in this.People.Values)
						{
							foreach (ImmediateOrder order in person.Orders.Immediate)
							{
								order.Executed = false;
							}
						}
        }

        public void ClearFailedToExecuteImmediateOrders()
        {
            foreach (Faction faction in this.Factions.Values)
            {
                foreach (ImmediateOrder order in faction.Orders.Immediate)
                {
                    order.FailedToExecute = false;
                }
            }
            foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
            {
                foreach (ImmediateOrder order in moduleStack.Orders.Immediate)
                {
                    order.FailedToExecute = false;
                }
            }
						foreach (Person person in this.People.Values)
						{
							foreach (ImmediateOrder order in person.Orders.Immediate)
							{
								order.FailedToExecute = false;
							}
						}
        }

		public void ClearUnformed()
		{
			//Person.All.RemoveUnformed();
			ModuleStack.All.RemoveNonReporting();
		}

		public void ClearExecutedLongOrder()
		{
			foreach (Faction faction in this.Factions.Values)
			{
				faction.ExecutedLongOrder = false;
			}

			foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
			{
				moduleStack.ExecutedLongOrder = false;
			}
			foreach (Person person in this.People.Values)
			{
				person.ExecutedLongOrder = false;
			}
		}

		public void ExecuteBetweenTurnOrders()
		{
			foreach (Faction faction in this.Factions.Values)
			{
				foreach (ImmediateOrder order in faction.Orders.Immediate)
				{
					if (order.AllowedBetweenTurns && !order.Executed)
					{
						order.Execute(this.week);
					}
				}
				faction.Orders.RemoveExecuted();
			}
		}

		public void ExecuteMedicalConsume()
		{
			List<ModuleStack> snapshot = new List<ModuleStack>(this.ModuleStacks.Values);
			foreach (ModuleStack moduleStack in snapshot)
			{
				moduleStack.ExecuteMedicalConsume(this.week);
			}
		}

		public void ExecuteQuarterlyWoundedOutcome()
		{
			List<ModuleStack> snapshot = new List<ModuleStack>(this.ModuleStacks.Values);
			foreach (ModuleStack moduleStack in snapshot)
			{
				moduleStack.ExecuteQuarterlyWoundedOutcome(13);
			}
		}

		public void ExecuteOrders() 
		{
            bool executedOrderByModuleStack;
            bool executedOrderByPerson;

            foreach (Faction faction in this.Factions.Values)
			{
				faction.Execute(this.Week);
			}

			executedOrderByModuleStack = this.ExecuteOrdersByModuleStack();
            executedOrderByPerson = this.ExecuteOrdersByPerson();
		}

		public bool ExecuteOrdersByPerson()
		{
			return this.ExecuteOrdersByPerson(this.People.HavingOrders);
		}

		public bool ExecuteOrdersByPerson(People people)
		{
			bool executedOrderByPerson;
			executedOrderByPerson = true;

			People peopleHavingOrders;
			while (executedOrderByPerson)
			{
				executedOrderByPerson = false;
				peopleHavingOrders = people.HavingOrders;
				foreach (Person person in peopleHavingOrders.Values)
				{
					if (person.Execute(this.Week))
					{
						executedOrderByPerson = true;
					}
				}
			}
			return executedOrderByPerson;
		}

		public bool ExecuteOrdersByModuleStack()
		{
			return this.ExecuteOrdersByModuleStack(this.ModuleStacks.HavingOrders);
		}

		public bool ExecuteOrdersByModuleStack(ModuleStacks moduleStacks)
		{
			bool executedOrderByModuleStack;
			executedOrderByModuleStack = true;

			ModuleStacks moduleStacksHavingOrders;
			while (executedOrderByModuleStack)
			{
				executedOrderByModuleStack = false;
				moduleStacksHavingOrders = moduleStacks.HavingOrders;
				foreach (ModuleStack moduleStack in moduleStacksHavingOrders.Values)
				{
					if (moduleStack.Execute(this.Week))
					{
						executedOrderByModuleStack = true;
					}
				}
			}
			return executedOrderByModuleStack;
		}

		#region economy

        private void GenerateOffers()
        {
            // foreach neutral modulestack generate trade offer
            // neutral faction cities cannot simultaneously sell and buy the same things
            // only offer for sale stuff that is actually available
        }

        private void UpdateBankAccounts()
        {
            // update deposit/credit lines
            // how about totaling the interests? if positive, deposits rate goes down 
            // (banks don't like to pay more interests then they receive) 
            foreach (Faction faction in this.Factions.Values)
            {
                faction.Bank.AddQuarterlyInterest(this.week);                
            }

        }

        private void UpdateRates()
        {
            // update deposit lines
            // estimate credit mass
            // estimate networth
            // estimate cost of money
            // update rates for the next quarter
            // estimate value of local trades
            // estimate value of external trades (like sum of receiving items effects)
            // estimate total costs of delivery service 
            // update costs of service
        }

        private void ProcessBuyOffers()
        {
            // foreach offer check if matching offer exists if it does, process and update prices
            Offers buyOffers;
            buyOffers = this.Offers[EOffersType.Buy];
            foreach (Offer buyOffer in buyOffers)
            {
                buyOffer.Process(week);
            }
        }

		#endregion

		#region battles
		public Battles Battles
		{
			get { return Battle.All; }
		}

		public void ExecuteBattles()
		{
			List<Battle> started = Battle.StartAtLocations(this.week);
			foreach (Battle battle in started)
			{
				battle.Execute(this.week);
			}
		}
		#endregion

		public void ClearDictionaries()
		{
			Battle.All.Clear();
			Offer.All.Clear();
			Contract.All.Clear();
			PressRelease.All.Clear();
			Technology.All.Clear();
			Race.All.Clear();
			ItemType.All.Clear();
			ModuleType.All.Clear();
			ModuleStack.All.Clear();
			SkillType.All.Clear();
			Person.All.Clear();
			Region.All.Clear();
			Orbit.All.Clear();
			Moon.All.Clear();
			Planet.All.Clear();
			Anomaly.All.Clear();
			Star.All.Clear();
			Faction.All.Clear();
		}
	}
}
