using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class MoveOrder : LongOrder
	{
		private readonly MoveOrderSubject moveSubject;

		public MoveOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.move;
			this.moveSubject = MoveOrderSubject.From(subject);
		}

        private Location destination = null;
        public Location Destination
		{
			get { return this.destination; }
			set { this.destination = value; }
		}

        private EMoveMode moveMode = EMoveMode.ground;
        public EMoveMode MoveMode
        {
            get { return this.moveMode; }
            set { this.moveMode = value; }
        }

        private List<Location> route = new List<Location>();
        public List<Location> Route
		{
			get { return this.route; }
		}

		private bool moveBack = false;
		public bool MoveBack
		{
			get { return this.moveBack; }
		}

		public ModuleStack Mover
		{
			get { return this.moveSubject.Stack; }
		}

		public int TotalDuration
		{
			// if repeatable infinite
			get { return 0; }
		}

		public override void Parse(string command)
		{
			// MOVE destination
			// MOVE destination1 destination2 ...
			string token;
			if (string.IsNullOrEmpty(command.Trim()))
			{
				throw new Exception("Bad syntax or no destinations");
			}

			while (!string.IsNullOrEmpty(token = LineParser.GetToken(ref command)))
			{
				if (string.Equals(token, "back", StringComparison.OrdinalIgnoreCase))
				{
					this.moveBack = true;
					continue;
				}
				try 
				{
					this.route.Add(this.FindDestination(token));
				} catch (Exception ex) {
					throw new Exception("Bad syntax or unknown destination. Received: " + token, ex);
				}
			}
		}

		public void ApplyInTransitAssignment(MoveOrder assigned)
		{
			this.moveBack = assigned.moveBack;
			this.route.Clear();
			if (assigned.moveBack)
			{
				this.destination = this.moveSubject.Location;
			}
			else if (assigned.Route.Count > 0)
			{
				this.destination = assigned.Route[0];
			}
			if (this.moveSubject.IsInTransit)
			{
				this.applyInTransitRedirect(0);
			}
			this.moveBack = false;
		}

		private bool destinationsMatch(Location left, IHolder right)
		{
			if (left == null || right == null)
			{
				return false;
			}
			return left.Name == right.Name;
		}

		private bool destinationsMatch(IHolder left, IHolder right)
		{
			if (left == null || right == null)
			{
				return false;
			}
			return left.Name == right.Name;
		}

        private Location FindDestination(string token)
		{
			if (Region.All.ContainsKey(token))
			{
				return Region.All[token];
			}
			else if (Star.All.ContainsKey(token))
			{
				return Star.All[token].Orbit;
			}
			else if (Planet.All.ContainsKey(token))
			{
				return Planet.All[token].Orbit;
			}
			else if (Moon.All.ContainsKey(token))
			{
				return Moon.All[token].Orbit;
			}
			else if (Belt.All.ContainsKey(token))
			{
				return Belt.All[token];
			}
			else if (Alderson.All.ContainsKey(token))
			{
				return Alderson.All[token].Orbit;
			}
			else if (Anomaly.All.ContainsKey(token))
			{
				return Anomaly.All[token].Orbit;
			}
			else if (Orbit.All.ContainsKey(token))
			{
				return Orbit.All[token];
			}
					
			throw new Exception("The target location has not been found: " + token);
		}

		public override void LoadXml(XmlElement elOrder)
		{
            XmlElement elMove = (XmlElement)elOrder.SelectNodes("move")[0];
			
			foreach (XmlElement elDestination in elMove.SelectNodes("destination"))
			{
				this.route.Add(this.FindDestination(elDestination.GetAttribute("destination")));
			}
			this.compactConsecutiveRouteDuplicates();
			if (elOrder.HasAttribute("duration-left"))
			{
				this.DurationLeft = this.XMLAssignInteger(elOrder.GetAttribute("duration-left"), this.DurationLeft);
			}
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
			if (this.DurationLeft > 0 && this.DurationLeft < Int32.MaxValue)
			{
				this.xmlElement.SetAttribute("duration-left", this.DurationLeft.ToString());
			}

            XmlElement elMove = doc.CreateElement("move");   
			
            XmlElement elDestination;
			foreach (IHolder destination in this.destinationsForPersistence())
			{
                elDestination = doc.CreateElement("destination");   
				elDestination.SetAttribute("destination", destination.Name);
				elMove.AppendChild(elDestination);
			}
            this.xmlElement.AppendChild(elMove);
			return this.xmlElement;
		}

        public override List<string> Report(Faction owner)
        {
            List<string> lines = new List<string>();
            string line;
            line = string.Format("{0}{1}move",
                this.Conditions,
                (this.Repeat > 1) ? string.Concat(this.Repeat.ToString(), " ") : ((this.Repeat < 0) ? "@" : string.Empty));

			Moving traveling = this.moveSubject.Effects.Moving;
			if (this.moveSubject.IsInTransit && traveling != null && traveling.Origin != null && traveling.Destination != null)
			{
				line = string.Concat(
					line,
					" from ",
					traveling.Origin.Name,
					" to ",
					traveling.Destination.Name);
			}
			else if (this.moveBack)
			{
				line = string.Concat(line, " back");
			}
			else
			{
				foreach (IHolder destination in this.destinationsForPersistence())
				{
					line = string.Concat(line, " ", destination.Name);
				}
			}
            lines.Add(line);
            return lines;
        }

		private List<Location> destinationsForPersistence()
		{
			List<Location> destinations = new List<Location>();
			if (this.destination != null)
			{
				destinations.Add(this.destination);
			}
			foreach (Location location in this.route)
			{
				if (destinations.Count == 0 || destinations[destinations.Count - 1] != location)
				{
					destinations.Add(location);
				}
			}
			return destinations;
		}

		private void compactConsecutiveRouteDuplicates()
		{
			List<Location> compacted = new List<Location>();
			foreach (Location location in this.route)
			{
				if (compacted.Count == 0 || compacted[compacted.Count - 1] != location)
				{
					compacted.Add(location);
				}
			}
			this.route = compacted;
		}

        private Dictionary<EMoveMode, List<IMoveable>> moveModesRecursive
        {
            get
            {
                Dictionary<EMoveMode, List<IMoveable>> moveModes = new Dictionary<EMoveMode, List<IMoveable>>();
                IMoveable mover = this.moveSubject.Moveable;
                if (this.moveSubject.MoveModes.ContainsKey(EMoveMode.ground))
                {
                    List<IMoveable> ground = new List<IMoveable>
                    {
                        mover
                    };
                    moveModes.Add(EMoveMode.ground, ground);
                }
                if (!this.moveSubject.IsPerson && this.moveSubject.MoveModes.ContainsKey(EMoveMode.naval))
                {
                    List<IMoveable> naval = new List<IMoveable>
                    {
                        mover
                    };
                    moveModes.Add(EMoveMode.naval, naval);
                }

                List<IMoveable> space = this.spaceMoveableRecursive(mover);
                if (space.Count > 0)
                {
                    moveModes.Add(EMoveMode.space, space);
                }
                return moveModes;
            }
        }

        private List<IMoveable> spaceMoveableRecursive(IMoveable moveable)
        {
            List<IMoveable> spaceMoveables = new List<IMoveable>();

            if (moveable.MoveModes.ContainsKey(EMoveMode.space))
            {
                spaceMoveables.Add(moveable);
            }

            if (moveable.HasModuleStacks())
            {
                foreach (ModuleStack moduleStack in moveable.ModuleStacks.Values)
                {
                    foreach(IMoveable spaceMoveable in this.spaceMoveableRecursive(moduleStack))
                    {
                        spaceMoveables.Add(spaceMoveable);
                    }
                }
            }
            return spaceMoveables;
        }

        private List<IMoveable> spaceMoveablesForFuel()
        {
            List<IMoveable> fuelMoveables = new List<IMoveable>();
            IMoveable mover = this.moveSubject.Moveable;
            if (this.moveSubject.MoveModes.ContainsKey(EMoveMode.space) && this.moveSubject.Fuel.Count > 0)
            {
                fuelMoveables.Add(mover);
            }
            this.collectSpacePropulsionMoveables(mover, fuelMoveables);
            return fuelMoveables;
        }

        private void collectSpacePropulsionMoveables(IMoveable moveable, List<IMoveable> fuelMoveables)
        {
            if (!moveable.HasModuleStacks())
            {
                return;
            }

            foreach (ModuleStack nested in moveable.ModuleStacks.Values)
            {
                if (nested.MoveModes.ContainsKey(EMoveMode.space)
                    && nested.ModuleType.Group == EModuleTypesGroup.propulsion
                    && nested.Fuel.Count > 0)
                {
                    fuelMoveables.Add(nested);
                }
                this.collectSpacePropulsionMoveables(nested, fuelMoveables);
            }
        }


		private bool isWay(int week)
		{
            if (!this.moveSubject.IsFormed)
            {
                this.moveSubject.EventReports.Add(
                    week,
                    string.Format("MOVE failed. unformed units cannot move."));		
                return false;
            }

			if (this.moveSubject.IsPerson && !this.validatePersonRoute(week))
			{
				return false;
			}

			if (this.moveSubject.IsInTransit
				&& this.destination != null
				&& this.destinationsMatch(this.destination, this.moveSubject.Location))
			{
				return true;
			}

            if (!this.moveSubject.IsPerson && this.moveModesRecursive.ContainsKey(EMoveMode.space))
            {
                return true;
            }

            Location current = (Location)this.moveSubject.Location;
            if (!current.Exits.Contains(this.destination))
            {
                return false;
            }
            EMoveMode surfaceMode;
            if (!this.tryCompatibleSurfaceMode(current.Exits[this.destination], out surfaceMode))
            {
				this.reportMoveModeFailure(week, surfaceMode);
                return false;
            }
            return true;
		}

		private bool validatePersonRoute(int week)
		{
			if (!(this.destination is Region))
			{
				this.reportMoveModeFailure(week, EMoveMode.space);
				return false;
			}
			if (!(this.moveSubject.Location is Region))
			{
				this.reportMoveModeFailure(week, EMoveMode.ground);
				return false;
			}
			Region from = (Region)this.moveSubject.Location;
			Region to = (Region)this.destination;
			if (from.RegionHolder != to.RegionHolder)
			{
				this.moveSubject.EventReports.Add(
					week,
					"MOVE failed. Persons can only move between regions on the same surface.");
				return false;
			}
			return true;
		}

		private void reportMoveModeFailure(int week, EMoveMode mode)
		{
			if (mode == EMoveMode.ground)
			{
				this.moveSubject.EventReports.Add(
					week,
					"MOVE failed. Unit need to be movable by ground.");
			}
			else if (mode == EMoveMode.naval)
			{
				this.moveSubject.EventReports.Add(
					week,
					"MOVE failed. Unit need to be movable by sea.");
			}
			else
			{
				this.moveSubject.EventReports.Add(
					week,
					"MOVE failed. Unit need to be able to move in space.");
			}
		}

		private bool tryCompatibleSurfaceMode(Exit exit, out EMoveMode mode)
		{
			bool exitNaval = exit.ExitModes.ContainsKey(EMoveMode.naval);
			bool exitGround = exit.ExitModes.ContainsKey(EMoveMode.ground);
			bool unitGround = this.moveModesRecursive.ContainsKey(EMoveMode.ground) && exitGround;
			bool unitNaval = !this.moveSubject.IsPerson
				&& this.moveModesRecursive.ContainsKey(EMoveMode.naval)
				&& exitNaval;

			if (this.moveSubject.IsPerson)
			{
				if (unitGround)
				{
					mode = EMoveMode.ground;
					return true;
				}
				mode = exitNaval && !exitGround ? EMoveMode.naval : EMoveMode.ground;
				return false;
			}

			if (unitNaval && !unitGround)
			{
				mode = EMoveMode.naval;
				return true;
			}
			if (unitGround)
			{
				mode = EMoveMode.ground;
				return true;
			}
			if (unitNaval)
			{
				mode = EMoveMode.naval;
				return true;
			}
			mode = EMoveMode.ground;
			return false;
		}

		private int movementDuration()
		{
			Location current = (Location)this.moveSubject.Location;
			if (this.destination != null && this.destinationsMatch(this.destination, current))
			{
				return this.DurationLeft > 0 && this.DurationLeft < Int32.MaxValue
					? this.DurationLeft
					: 1;
			}

			if (this.moveSubject.Location is Region && this.destination is Region)
			{
				Region region1 = (Region)this.moveSubject.Location;
				Region region2 = (Region)this.destination;

				if (region1.RegionHolder == region2.RegionHolder)
				{
					Exit exit = region1.Exits[this.destination];
					EMoveMode surfaceMode;
					if (!this.tryCompatibleSurfaceMode(exit, out surfaceMode))
					{
						throw new InvalidOperationException("no compatible surface move mode");
					}
					this.moveMode = surfaceMode;
					ExitMode exitMode = exit.ExitModes[surfaceMode];
					double speed = this.moveSubject.MoveModes[surfaceMode].Speed;
					return Convert.ToInt32(Math.Ceiling(exitMode.Duration / speed));
				}
				if (this.hasSpaceExit(current))
				{
					return this.spaceExitDuration(current);
				}
				return this.spaceAuDuration();
			}
			if (this.destination is Belt || this.moveSubject.Location is Belt)
			{
				if (this.hasSpaceExit(current))
				{
					return this.spaceExitDuration(current);
				}
				return this.spaceAuDuration();
			}
			if ((this.moveSubject.Location is Region && this.destination is Orbit) || (this.moveSubject.Location is Orbit && this.destination is Region))
			{
				if (this.moveSubject.Location.LocationParent == this.destination.LocationParent)
				{
					this.moveMode = EMoveMode.space;
					return 1;
				}
				if (this.hasSpaceExit(current))
				{
					return this.spaceExitDuration(current);
				}
				return this.spaceAuDuration();
			}
			if (this.moveSubject.Location is Orbit && this.destination is Orbit)
			{
				return this.spaceAuDuration();
			}
			if (this.hasSpaceExit(current))
			{
				return this.spaceExitDuration(current);
			}
			return this.spaceAuDuration();
		}

		private bool hasSpaceExit(Location current)
		{
			return current.Exits.Contains(this.destination)
				&& current.Exits[this.destination].ExitModes.ContainsKey(EMoveMode.space);
		}

		private int spaceExitDuration(Location current)
		{
			this.moveMode = EMoveMode.space;
			ExitMode exitMode = current.Exits[this.destination].ExitModes[EMoveMode.space];
			int weeks = SpaceTransit.ExitDurationWeeks(exitMode.Duration, this.moveSubject.Stack);
			return SkillEffects.ApplyDurationPercent(weeks, SkillEffects.MoveDurationPercent(this.moveSubject.RootModuleStack));
		}

		private int spaceAuDuration()
		{
			this.moveMode = EMoveMode.space;
			double deltaAu = Math.Abs(SpaceTransit.BodyAu(this.moveSubject.Location) - SpaceTransit.BodyAu(this.destination));
			int weeks = SpaceTransit.DurationWeeks(deltaAu, SpaceTransit.EffectiveSpaceSpeed(this.moveSubject.Stack));
			return SkillEffects.ApplyDurationPercent(weeks, SkillEffects.MoveDurationPercent(this.moveSubject.RootModuleStack));
		}

		private bool applyPatrolRules(int week)
		{
			Region destinationRegion = this.destination as Region;
			if (destinationRegion == null)
			{
				return true;
			}

			ModuleStack blocker = PatrolGuard.FindBlocker(destinationRegion, this.moveSubject.Owner);
			if (blocker == null)
			{
				return true;
			}

			this.moveSubject.EventReports.Add(
				week,
				string.Format(
					"MOVE failed. {0} is patrolling {1}.",
					blocker.ReportName,
					destinationRegion.ReportName));
			return false;
		}

		private bool applyEnvironmentMoveRules(int week)
		{
			Location from = this.moveSubject.Location as Location;
			Location to = this.destination;
			if (!this.applyPatrolRules(week))
			{
				return false;
			}
			if (!BodyEnvironment.IsSurfaceOrbitHop(from, to))
			{
				return true;
			}
			if (this.moveSubject.IsPerson || this.moveSubject.Stack == null)
			{
				return true;
			}
			ModuleStack moverStack = this.moveSubject.Stack;
			if (BodyEnvironment.BansNonShuttleSurfaceHop(moverStack.ModuleType, from, to))
			{
				this.moveSubject.EventReports.Add(
					week,
					"MOVE failed. Spaceships cannot use solid-surface exits on atmospheres.");
				return false;
			}
			Location surface = from is Region ? from : to;
			int surcharge = BodyEnvironment.SurfaceOrbitSurcharge(surface);
			if (surcharge <= 0)
			{
				return true;
			}
			ItemType h2o2 = ItemType.All["h2o2"];
			ItemStacks need = new ItemStacks();
			need.Add(new ItemStack(h2o2, surcharge));
			if (!this.moveSubject.HasItemsAvailableTo(moverStack, need))
			{
				this.moveSubject.EventReports.Add(
					week,
					string.Format("MOVE failed. Not enough {0} for launch.", ItemType.All["h2o2"].ReportName));
				return false;
			}
			IItemStacksHolder holder = this.moveSubject.FindItemHolderAvailableTo(moverStack, need);
			if (holder == null)
			{
				this.moveSubject.EventReports.Add(
					week,
					string.Format("MOVE failed. Not enough {0} for launch.", ItemType.All["h2o2"].ReportName));
				return false;
			}
			holder.ItemStacks.Minus(need);
			holder.EventReports.Add(
				week,
				string.Format(
					"consumed {0} for launch{1}.",
					need.ReportList,
					holder == moverStack ? string.Empty : string.Concat(" for ", this.moveSubject.ReportName)));
			return true;
		}

		private bool canMove(int week, bool departInitiation)
		{
			if (departInitiation && this.Mover != null && !this.Mover.CanInitiateMove(this.moveMode))
			{
				this.moveSubject.EventReports.Add(
					week,
					string.Format(
						"MOVE failed. {0} cannot depart while modules are integrating or disabled.",
						this.moveSubject.ReportName));
				return false;
			}

			bool nestedMoverInitiation = this.Mover != null
				&& this.Mover.UsesNestedMoverForMoveInitiation(this.moveMode);
			if (!this.moveSubject.IsInTransit && !nestedMoverInitiation && !this.moveSubject.IsActive)
			{
				string inactiveState = this.moveSubject.IsPartiallyDisabled
					? (this.moveSubject.IsLivingUnit ? "partially routed" : "partially disabled")
					: (this.moveSubject.IsLivingUnit ? "routed" : "disabled");
				this.moveSubject.EventReports.Add(
					week,
					string.Format("MOVE failed. {0} is {1}.",
						this.moveSubject.ReportName,
						inactiveState));
				return false;
			}

			if (!this.moveModesRecursive.ContainsKey(this.moveMode))
            {
                if (this.moveMode == EMoveMode.ground)
                {
                    this.moveSubject.EventReports.Add(
                        week,
                        string.Format("MOVE failed. Unit need to be movable by ground."));
                }
                else if (this.moveMode == EMoveMode.naval)
                {
                    this.moveSubject.EventReports.Add(
                        week,
                        string.Format("MOVE failed. Unit need to be movable by sea."));
                }
                else
                {
                    this.moveSubject.EventReports.Add(
                           week,
                           string.Format("MOVE failed. Unit need to be able to move in space."));
                }
                return false;
            }
				
			// capacity, crew, borders

			return true; 
		}

        private IItemStacksHolder findFuelHolder(IItemStacksHolder holder, ItemStacks fuelItemStacks)
        {
            if (holder.ItemStacks.Has(fuelItemStacks))
            {
                return holder;
            } 
            if (holder.HasModuleStacks())
            {
                foreach (ModuleStack moduleStack in holder.ModuleStacks.Values)
                {
                    IItemStacksHolder subHolder = this.findFuelHolder(moduleStack, fuelItemStacks);
                    if (subHolder != null)
                    {
                        return subHolder;
                    }
                }
            }
            if (holder.HasPeople)
            {
                foreach (Person person in holder.People.Values)
                {
                    IItemStacksHolder subHolder = this.findFuelHolder(person, fuelItemStacks);
                    if (subHolder != null)
                    {
                        return subHolder;
                    }
                }
            }
            return null;
        }

        private bool consumeFuel(int week, List<IMoveable> moveables)
        {
            foreach (IMoveable moveable in moveables)
            {
                if (!moveable.Effects.IsFuelled)
                {
                    ModuleStack fuelRoot = this.moveSubject.Stack;
                    if (fuelRoot == null)
                    {
                        continue;
                    }
                    if (this.moveSubject.HasItemsAvailableTo(fuelRoot, moveable.Fuel))
                    {
                        IItemStacksHolder holder = this.moveSubject.FindItemHolderAvailableTo(fuelRoot, moveable.Fuel);

                        if (holder != null)
                        {
                            holder.ItemStacks.Minus(moveable.Fuel);
                            holder.EventReports.Add(
                                week,
                                string.Format("consumed {0} as fuel{1}.", 
                                moveable.Fuel.ReportList, 
                                (holder == moveable) ? string.Empty : string.Concat(" for ", moveable.ReportName)));

                            Fuelled fuelled = new Fuelled(moveable, moveable.FuelDuration);
                            fuelled.Use();
                        }
                    }
                    else
                    {
                        this.moveSubject.EventReports.Add(week, string.Format("is out of fuel for {0}.", moveable.ReportName));
                        return true;
                    }
                }
                else
                {
                    moveable.Effects.Fuelled.Use();
                }
            }
            return false;
        }

		private bool needFuel(int week)
		{
            Dictionary<EMoveMode, List<IMoveable>> moveModes = moveModesRecursive;

            ItemStacks fuelItemStacks = this.moveSubject.Fuel;

            if (this.moveMode == EMoveMode.ground || this.moveMode == EMoveMode.naval)
            {
                if (this.moveSubject.Fuel.Count == 0)
                    return false;

                return this.consumeFuel(week, moveModes[this.moveMode]);
            }
            else if (this.moveMode == EMoveMode.space)
            {
                List<IMoveable> spaceMoveables = this.spaceMoveablesForFuel();
                bool needFuelforSpace = false;
                foreach (IMoveable moveable in spaceMoveables)
                {
                    if (moveable.Fuel.Count > 0)
                        needFuelforSpace = true;
                }
                if (!needFuelforSpace)
                    return false;
                return this.consumeFuel(week, spaceMoveables);
            }
            return false;
		}

        private Moving moving = null;

		private bool applyInTransitRedirect(int week)
		{
			if (!this.moveSubject.IsInTransit)
			{
				return true;
			}
			if (this.moving == null)
			{
				this.moving = this.moveSubject.Effects.Moving;
			}
			if (this.moving == null || this.destination == null)
			{
				return true;
			}
			IHolder tripOrigin = this.moveSubject.Location as IHolder;
			if (this.destinationsMatch(this.destination as IHolder, this.moveSubject.MovingTo))
			{
				return true;
			}
			if (this.destinationsMatch(this.destination, tripOrigin))
			{
				if (!this.destinationsMatch(this.moveSubject.MovingTo, tripOrigin))
				{
					int weeksToOrigin = this.moving.ReturnLegWeeks;
					if (weeksToOrigin <= 0)
					{
						weeksToOrigin = this.moving.InitialDuration - this.DurationLeft;
					}
					IHolder previousDestination = this.moving.Destination;
					this.moving.Origin = previousDestination;
					this.moving.Destination = tripOrigin;
					this.destination = tripOrigin as Location;
					this.moveSubject.MovingTo = tripOrigin;
					this.DurationLeft = weeksToOrigin;
					this.moving.Duration = weeksToOrigin;
					this.moving.InitialDuration = weeksToOrigin;
					this.moving.ReturnLegWeeks = 0;
					this.moving.Destination = this.moveSubject.MovingTo;
				}
				return true;
			}
			this.moveSubject.EventReports.Add(
				week,
				string.Format(
					"MOVE failed. {0} is in transit from {1} to {2}; only continue to destination or move back to origin.",
					this.moveSubject.ReportName,
					tripOrigin != null ? tripOrigin.ReportName : this.moving.Origin.ReportName,
					this.moveSubject.MovingTo.ReportName));
			this.destination = this.moveSubject.MovingTo as Location;
			this.Executing = true;
			this.Executed = false;
			return true;
		}
        
		public override void Execute(int week)
		{
			try
			{
				base.Execute(week);

				this.moving = this.moveSubject.Effects.Moving;

				if (this.moveBack)
				{
					this.destination = this.moveSubject.Location;
					this.moveBack = false;
				}

				if (this.destination == null && this.route.Count > 0)
				{                    
					this.destination = this.route[0];
					this.route.RemoveAt(0);
					while (this.route.Count > 0 && this.route[0] == this.destination)
					{
						this.route.RemoveAt(0);
					}
					if (this.IsUnlimited || this.Repeat > 1)
					{
						this.route.Add(this.destination);
					}
				}

				if (this.destination == null && this.moveSubject.IsInTransit && this.moveSubject.MovingTo != null)
				{
					this.destination = this.moveSubject.MovingTo as Location;
				}
				else if (this.destination == null && this.moveSubject.IsInTransit && this.moveSubject.Effects.Moving != null)
				{
					this.destination = this.moveSubject.Effects.Moving.Destination as Location;
				}

				this.applyInTransitRedirect(week);

				if (this.isWay(week))
				{
					if (this.moving == null)
					{
						this.moving = this.moveSubject.Effects.Moving;
					}
					if ((this.DurationLeft <= 0 || this.DurationLeft >= Int32.MaxValue)
						&& this.moving != null && this.moving.Duration > 0)
					{
						this.DurationLeft = this.moving.Duration;
					}

					bool resume = this.moveSubject.MovingTo != null
						|| (this.DurationLeft > 0 && this.DurationLeft < Int32.MaxValue);

					if (this.moveSubject.MovingTo == null)
					{
						this.moveSubject.PrepareFootDeparture(week);
						this.moveSubject.CancelDeliveringPurchases(week);
						if (!this.applyEnvironmentMoveRules(week))
						{
							this.Executing = false;
							this.Executed = true;
							return;
						}
						if (!resume)
						{
							this.DurationLeft = this.movementDuration();
						}
						else
						{
							this.movementDuration();
						}
					}
					else
					{
						this.movementDuration();
					}

					int legDuration = this.movementDuration();
					bool firstStepOfLeg = !resume && this.DurationLeft == legDuration;
					bool departInitiation = this.moveSubject.MovingTo == null;
					if (this.canMove(week, firstStepOfLeg))
					{
						if (departInitiation && this.moving == null)
						{
							this.moving = new Moving(
								this.moveSubject.Effectable,
								this.MoveMode,
								this.moveSubject.Location,
								this.Destination,
								this.DurationLeft);
						}

						if (!this.needFuel(week))
						{
							if (departInitiation)
							{
								this.Executing = true;
								this.moveSubject.MovingTo = this.destination;
							}

							this.DurationLeft--;
							if (firstStepOfLeg)
							{
								this.moveSubject.EventReports.Add(
									week,
									string.Format(
										"departed from {0} to {1}, ETA {2}.",
										this.moveSubject.Location.ReportName,
										this.destination.ReportName,
										this.DurationLeft));
							}
							if (this.moving != null)
							{
								this.moving.Duration = this.DurationLeft;
								this.moving.ReturnLegWeeks = this.moving.InitialDuration - this.DurationLeft;
							}

							if ((this.DurationLeft + 1) != this.movementDuration() && this.DurationLeft != 0)
							{
								this.moveSubject.EventReports.Add(
									week,
									string.Format(
										"moving from {0} to {1}, ETA {2}.",
										this.moveSubject.Location.ReportName,
										this.destination.ReportName,
										this.DurationLeft));
							}
							if (this.moving == null)
							{
								this.moving = this.moveSubject.Effects.Moving;
							}
							if (this.moving != null)
							{
								this.moving.Use();
							}
						}
						else if (departInitiation)
						{
							if (this.moving != null)
							{
								this.moveSubject.Effects.Remove(this.moving);
								this.moving = null;
							}
							this.Executing = false;
							this.DurationLeft = Int32.MaxValue;
						}
					}
					else if (this.moveSubject.MovingTo == null)
					{
						this.Executing = false;
						this.DurationLeft = Int32.MaxValue;
					}

					// change location if arrived
					if (this.DurationLeft == 0)
					{
						this.moveSubject.EventReports.Add(
							week,
							string.Format(
								"arrived at {0} from {1}.",
								this.destination.ReportName,
								this.moveSubject.Location.ReportName));
						if (this.moveSubject.Parent != this.moveSubject.MovingTo)
						{
							this.moveSubject.Parent = this.moveSubject.MovingTo;
						}
						this.moveSubject.MovingTo = null;
						this.destination = null;
						if (this.moving != null)
						{
							this.moving.Duration = 0;
							this.moving.Executed = true;
							this.moveSubject.Effects.Remove(this.moving);
							this.moving = null;
						}
                        
                        this.Executed = true;

                        this.Executing = false;
                        if (this.Repeat > 0)
                        {
                            this.Repeat--;
                        }
                    }
                    else
                    {
                        this.Executed = false;
                        this.Executing = true;
                    }
				}				
			}
			catch (Exception ex)
			{
				throw new Exception("tried to move " + this.moveSubject.ToString() + " to " + this.Destination.ToString(), ex);
			}
		}
	}
}
