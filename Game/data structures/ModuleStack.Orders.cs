
namespace SpaceAge
{
	public partial class ModuleStack
	{
		#region IOrderable Members

		private Orders orders = new Orders();
		public Orders Orders
		{
			get { return this.orders; }
		}

		#endregion
		public bool Execute(int week)
		{
			bool executedOrderByModuleStack;
			
			executedOrderByModuleStack = this.Orders.Execute(week);
			this.Orders.RemoveExecuted();
			this.Effects.Execute(week);
			this.Effects.RemoveExecuted();

			return executedOrderByModuleStack;
		}
	}
}
