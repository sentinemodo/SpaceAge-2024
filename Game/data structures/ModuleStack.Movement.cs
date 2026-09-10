
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
