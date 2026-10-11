namespace SpaceAge
{
	public partial class ModuleStack
	{
		#region movement

        private IHolder movingTo = null;
        public IHolder MovingTo
		{
			get { return this.movingTo; }
			set { this.movingTo = value; }
		}

		public bool IsInTransit
		{
			get { return this.movingTo != null; }
		}

		public Location EffectiveOrderLocation()
		{
			Moving traveling = this.Effects.Moving;
			if (this.IsInTransit && traveling != null)
			{
				return this.TravelEffectiveRegion(traveling);
			}
			return this.Location;
		}

		public Location TravelEffectiveRegion(Moving moving)
		{
			if (moving == null || !this.IsInTransit)
			{
				return this.Location;
			}
			int initial = moving.InitialDuration;
			if (initial <= 0)
			{
				return this.Location;
			}
			int elapsed = initial - moving.Duration;
			Location origin = moving.Origin as Location;
			Location destination = moving.Destination as Location;
			if (origin == null || destination == null)
			{
				return this.Location;
			}
			if (elapsed * 2 > initial)
			{
				return destination;
			}
			return origin;
		}

		public bool IsRoot
		{
			get 
			{ 
				if (this.parent is Region) 
					return true; 
				else
					return false;  
			}
		}

        public MoveModes MoveModes
        {
            get
            {
                MoveModes moveModes = new MoveModes();
                if (this.IsFormed)
                {
                    MoveMode multipliedMoveMode;
                    foreach (MoveMode moveMode in this.moduleType.MoveModes.Values)
                    {
                        multipliedMoveMode = new MoveMode();
                        multipliedMoveMode.Mode = moveMode.Mode;
                        multipliedMoveMode.Speed = moveMode.Speed;
                        multipliedMoveMode.MassCapacity = this.Quantity * moveMode.MassCapacity;

                        moveModes.Add(multipliedMoveMode.Mode, multipliedMoveMode);
                    }
                }
                return moveModes;
            }
        }
		#endregion
	}
}
