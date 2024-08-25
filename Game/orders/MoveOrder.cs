using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public class MoveOrder : LongOrder
	{
		public MoveOrder(IOrderable subject)
			: base(subject)
		{
			this.type = EOrderType.move;
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

		public ModuleStack Mover
		{
			get { return (ModuleStack)this.Subject; }
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
				try 
				{
					this.route.Add(this.FindDestination(token));
				} catch (Exception ex) {
					throw new Exception("Bad syntax or unknown destination. Received: " + token, ex);
				}
			}
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
				this.route.Add(Region.All[elDestination.GetAttribute("destination")]);
			}								
		}

		public override XmlElement SaveXml_core(XmlDocument doc, string subject)
		{
            XmlElement elMove = doc.CreateElement("move");   
			
            XmlElement elDestination;
			foreach (IHolder destination in this.route)
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
            
            foreach (IHolder destination in this.route)
            {
                line = string.Concat(line, " ", destination.Name);
            }
            lines.Add(line);
            return lines;
        }

        private Dictionary<EMoveMode, List<IMoveable>> moveModesRecursive
        {
            get
            {
                Dictionary<EMoveMode, List<IMoveable>> moveModes = new Dictionary<EMoveMode, List<IMoveable>>();
                if (this.Mover.MoveModes.ContainsKey(EMoveMode.ground))
                {
                    List<IMoveable> ground = new List<IMoveable>();
                    ground.Add(this.Mover);
                    moveModes.Add(EMoveMode.ground, ground);
                }

                List<IMoveable> space = this.spaceMoveableRecursive(this.Mover);
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


		private bool isWay(int week)
		{
            if (!this.Mover.IsFormed)
            {
                this.Mover.EventReports.Add(
                    week,
                    string.Format("MOVE failed. unformed units cannot move."));		
                return false;
            }

            if (this.moveModesRecursive.ContainsKey(EMoveMode.space))
            {
                return true;
            }
            else
            {
                Location current = (Location)this.Mover.Location;

                if (current.Exits.Contains(this.destination))
                {
                    return true;
                }
            }

			return false;
		}

		private int movementDuration()
		{
            if (this.Mover.Location is Region && this.destination is Region)
            {
                Region region1 = (Region)this.Mover.Location;
                Region region2 = (Region)this.destination;

                if (region1.RegionHolder == region2.RegionHolder)
                {
                    // same planet, region->region, ground movement
                    this.moveMode = EMoveMode.ground;
                    ExitMode exitMode = region1.Exits[this.destination].ExitModes[EMoveMode.ground];
                    
                    // that's pretty rough :/
                    //double speed = this.moveModesRecursive[EMoveMode.ground][0].MoveModes[EMoveMode.ground].Speed;
                    return Convert.ToInt32(Math.Ceiling(exitMode.Duration / this.Mover.Speed));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if ((this.Mover.Location is Region && this.destination is Orbit) || (this.Mover.Location is Orbit && this.destination is Region)) 
            { // region -> orbit or orbit -> region, space movement
                if (this.Mover.Location.LocationParent == this.destination.LocationParent)
                {// same planet
                    this.moveMode = EMoveMode.space;
                    return 1;
                }
                else
                {// not the same planet
                    throw new NotImplementedException();
                }
            } 
            else if (this.Mover.Location is Orbit && this.destination is Orbit)
            {
                this.moveMode = EMoveMode.space;
                Orbit orbit1 = (Orbit)this.Mover.Location;
                Orbit orbit2 = (Orbit)this.destination;
                //double distance = orbit1.OrbitHolder.DistanceTo(orbit2);
                
                double N = 0;
                double M = 0;
                double distance = 0;
                double speed = 0;
                if (orbit1.OrbitHolder is Planet && orbit2.OrbitHolder is Moon)
                {// planet -> moon
                    if (((Moon)orbit2.OrbitHolder).Planet == (Planet)orbit1.OrbitHolder)
                    {// same planet
                        N = 0;
                        M = ((Moon)orbit2.OrbitHolder).AU;
                        distance = (M - N) * 50 + (M + N) * 5;
                        speed = (this.Mover.MassCapacity + this.Mover.ModuleStacks.MassCapacity) / this.Mover.Mass;
                    }
                    else
                    {// not the same planet
                        throw new NotImplementedException();
                    }
                }
                else if (orbit1.OrbitHolder is Moon && orbit2.OrbitHolder is Planet)
                {
                    // moon -> planet
                    if (((Moon)orbit1.OrbitHolder).Planet == (Planet)orbit2.OrbitHolder)
                    {// same planet
                        N = 0;
                        M = ((Moon)orbit1.OrbitHolder).AU;
                        distance = (M - N) * 50 + (M + N) * 5;
                        speed = (this.Mover.MassCapacity + this.Mover.ModuleStacks.MassCapacity) / this.Mover.Mass;
                    }
                    else
                    {// not the same planet
                        throw new NotImplementedException();
                    }
                }
                int duration = Convert.ToInt32(Math.Ceiling(distance / speed));

                return duration;
            
            } else {
                throw new NotImplementedException("nothing matches");
            }
		}

		private bool canMove(int week)
		{          
			if (!this.moveModesRecursive.ContainsKey(this.moveMode))
            {
                if (this.moveMode == EMoveMode.ground)
                {
                    this.Mover.EventReports.Add(
                        week,
                        string.Format("MOVE failed. Unit need to be movable by ground."));
                }
                else
                {
                    this.Mover.EventReports.Add(
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
                    if (this.Mover.ItemStacksSumRecursive.Has(moveable.Fuel))
                    {
                        IItemStacksHolder holder = this.findFuelHolder(this.Mover, moveable.Fuel);

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
                        this.Mover.EventReports.Add(week, string.Format("is out of fuel for {0}.", moveable.ReportName));
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

            ItemStacks fuelItemStacks = this.Mover.Fuel;

            if (this.moveMode == EMoveMode.ground)
            {
                if (this.Mover.Fuel.Count == 0)
                    return false;

                return this.consumeFuel(week, moveModes[EMoveMode.ground]);
            }
            else if (this.moveMode == EMoveMode.space)
            {
                bool needFuelforSpace = false;
                foreach (IMoveable moveable in moveModes[EMoveMode.space])
                {
                    if (moveable.Fuel.Count > 0)
                        needFuelforSpace = true;
                }
                if (!needFuelforSpace)
                    return false;
                return this.consumeFuel(week, moveModes[EMoveMode.space]);
            }
            return false;
		}

        private Moving moving = null;
        
		public override void Execute(int week)
		{
			try
			{
				base.Execute(week);

				if (this.destination == null)
				{                    
					this.destination = this.route[0];
					this.route.RemoveAt(0);
				}

				if (this.Repeat != 0)
				{
					this.route.Add(destination);
				}

				if (this.isWay(week))
				{
					// assign destination if not moving
					if (this.Mover.MovingTo == null)
					{
						this.Executing = true;
						this.Mover.MovingTo = this.destination;
						this.DurationLeft = this.movementDuration();
                        this.moving = new Moving(this.Mover, this.MoveMode, this.Destination, this.DurationLeft);
                        this.Mover.EventReports.Add(
                            week,
                            string.Format(
                                "departed from {0} to {1}, ETA {2}.",
                                this.Mover.Location.ReportName,
                                this.destination.ReportName,
                                this.DurationLeft - 1));
					}

					// move
					if (this.canMove(week) && !this.needFuel(week))
					{
                        this.DurationLeft--;

                        if ((this.DurationLeft + 1) != this.movementDuration() && this.DurationLeft != 0)
                        {
                            this.Mover.EventReports.Add(
                                week,
                                string.Format(
                                    "moving from {0} to {1}, ETA {2}.",
                                    this.Mover.Location.ReportName,
                                    this.destination.ReportName,
                                    this.DurationLeft));
                        }
                        this.moving.Use();
                        //this.moving.Execute(week);
                    }

					// change location if arrived
					if (this.DurationLeft == 0)
					{
						this.Mover.EventReports.Add(
							week,
							string.Format(
								"arrived at {0} from {1}.",
								this.destination.ReportName,
								this.Mover.Location.ReportName));                        
						this.Mover.Parent = this.Mover.MovingTo;
						this.Mover.MovingTo = null;
						this.destination = null;
                        
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
				throw new Exception("tried to move " + this.Mover.ToString() + " to " + this.Destination.ToString(), ex);
			}
		}
	}
}
