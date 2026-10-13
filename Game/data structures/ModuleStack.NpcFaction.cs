namespace SpaceAge
{
	public partial class ModuleStack
	{
		public bool UsesPlayerSupplyRules
		{
			get { return ContractScope.IsPlayerFaction(this.Owner); }
		}

		internal int quantityOperationalForOwnerRules()
		{
			if (!this.IsFormed || !this.online)
			{
				return 0;
			}

			return this.QuantityActive;
		}
	}
}
