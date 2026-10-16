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

		private int startingYear = 2156;

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
			this.DropStaleUnitAttitudes();
            #region execute orders
            for (this.week = 1; this.week <= 13; this.week++)
			{
				this.ClearExecutedLongOrder();
				//this.ClearFailedToExecuteImmediateOrders();
				this.ClearExecutedImmediateOrders();
				this.ExecuteOrders();
				this.ExecuteSickBayHeal();
				this.ExecuteMedicalConsume();
				Contract.All.Evaluate(this.week);
                this.ProcessBuyOffers();
				this.ExecuteBattles();
				this.DropStaleUnitAttitudes();
			}
			this.week = 13;
			this.ExecuteMaintenance();
			this.ExecuteQuarterlyWoundedOutcome();
			this.ClearExecutedLongOrder();
			//this.ClearFailedToExecuteImmediateOrders();
			this.ClearExecutedImmediateOrders();
			this.ClearUnformed();
            #endregion
            this.UpdateBankAccounts();
            this.UpdateRates();
            this.GenerateOffers();
			this.ApplyPendingFactionPasswords();
        }

		public void ApplyPendingFactionPasswords()
		{
			foreach (Faction faction in this.Factions.Values)
			{
				if (string.IsNullOrEmpty(faction.PendingPassword))
				{
					continue;
				}

				faction.Password = faction.PendingPassword;
				faction.PendingPassword = null;
				faction.EventReports.Add(this.week, "password changed.");
			}
		}

		public void ClearEventReports()
		{
			foreach (Faction faction in this.Factions.Values)
			{
				faction.EventReports.Clear();
				faction.ContractEventReports.Clear();
				faction.Bank.Transactions.Clear();
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
                    ResetImmediateOrder(order);
                }
            }
            foreach (ModuleStack moduleStack in this.ModuleStacks.Values)
            {
                foreach (ImmediateOrder order in moduleStack.Orders.Immediate)
                {
                    ResetImmediateOrder(order);
                }
            }
						foreach (Person person in this.People.Values)
						{
							foreach (ImmediateOrder order in person.Orders.Immediate)
							{
								ResetImmediateOrder(order);
							}
						}
        }

		private static void ResetImmediateOrder(ImmediateOrder order)
		{
			order.Executed = false;
			SynchroOrder synchro = order as SynchroOrder;
			if (synchro != null)
			{
				synchro.Armed = false;
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

			foreach (ModuleStack moduleStack in ModuleStack.All.Values)
			{
				moduleStack.ExecutedLongOrder = false;
			}
			foreach (Person person in this.People.Values)
			{
				person.ExecutedLongOrder = false;
			}

			this.ClearReceivedThisWeek();
		}

		public void ClearReceivedThisWeek()
		{
			foreach (ModuleStack moduleStack in ModuleStack.All.Values)
			{
				moduleStack.ClearReceivedThisWeek();
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

		public void ExecuteSickBayHeal()
		{
			List<ModuleStack> snapshot = new List<ModuleStack>(this.ModuleStacks.Values);
			foreach (ModuleStack moduleStack in snapshot)
			{
				moduleStack.ExecuteSickBayHeal(this.week);
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

		public void ExecuteMaintenance()
		{
			List<ModuleStack> snapshot = new List<ModuleStack>(this.ModuleStacks.Values);
			foreach (ModuleStack moduleStack in snapshot)
			{
				moduleStack.ExecuteMaintenance(this.week);
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

        public void GenerateOffers()
        {
            Market.GenerateOffers();
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

        public void UpdateRates()
        {
            Market.UpdateRates();
        }

        public void ProcessBuyOffers()
        {
            foreach (Region region in Region.All.Values)
            {
                region.Market.ProcessBuyClearing(this.week);
            }
            Offers buyOffers = this.Offers[EOffersType.Buy];
            foreach (Offer buyOffer in buyOffers)
            {
                if (buyOffer.OfferType == EOfferType.BuyTechnologies)
                {
                    buyOffer.Process(this.week);
                }
                else if (buyOffer.Everywhere || buyOffer.BuyInRegion != null)
                {
                    // Planetary / cross-region catalog buy — stub until planetary market (wishlist).
                }
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

		public void DropStaleUnitAttitudes()
		{
			foreach (Faction faction in this.Factions.Values)
			{
				faction.DropStaleUnitAttitudes();
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
			Belt.All.Clear();
			Alderson.All.Clear();
			Anomaly.All.Clear();
			Star.All.Clear();
			Faction.All.Clear();
		}
	}
}
