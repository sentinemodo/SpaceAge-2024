namespace SpaceAge
{
	public partial class Person
	{
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

		public bool IsMoveActive
		{
			get { return this.IsFormed; }
		}
	}
}
