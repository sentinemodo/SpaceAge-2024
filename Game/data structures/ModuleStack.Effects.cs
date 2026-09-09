
namespace SpaceAge
{
	public partial class ModuleStack
	{
        #region IEffectable Members

        private Effects effects = new Effects();
		public Effects Effects
		{
			get { return this.effects; }
		}

		#endregion

		#region IEventReporting Members

		private EventReports eventReports = new EventReports();
		public EventReports EventReports
		{
			get { return this.eventReports; }
		}

		#endregion

		private bool executedLongOrder = false;
		public bool ExecutedLongOrder
		{
			get { return this.executedLongOrder; }
			set { this.executedLongOrder = value; }
		}
	}
}
