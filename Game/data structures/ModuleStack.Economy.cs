
namespace SpaceAge
{
	public partial class ModuleStack
	{
		#region economy

		private bool allowBank = true;
		public bool AllowBank
		{
			get { return this.allowBank; }
			set { this.allowBank = value; }
		}

		public bool HasBankAccess
		{
			get { return this.allowBank; }
		}

        public Offers Offers
        {
            get { return Offer.All[this];  }
        }

        #endregion
        #region research
        private int researchPoints = 0;
        public int ResearchPoints
        {
            get { return this.researchPoints; }
            set { this.researchPoints = value; }
        }

        private int researchOutputRemainder = 0;
        public int ResearchOutputRemainder
        {
            get { return this.researchOutputRemainder; }
            set { this.researchOutputRemainder = value; }
        }

        public int ResearchItemThroughputBonus
        {
            get
            {
                int bonus = 0;
                foreach (ItemStack itemStack in this.ItemStacks.Values)
                {
                    if (itemStack.ItemType.ResearchThroughput > 0)
                    {
                        bonus += itemStack.ItemType.ResearchThroughput * itemStack.Quantity;
                    }
                }
                foreach (Person person in this.People.Values)
                {
                    foreach (ItemStack itemStack in person.ItemStacks.Values)
                    {
                        if (itemStack.ItemType.ResearchThroughput > 0)
                        {
                            bonus += itemStack.ItemType.ResearchThroughput * itemStack.Quantity;
                        }
                    }
                }
                foreach (ModuleStack nested in this.ModuleStacks.Values)
                {
                    bonus += nested.ResearchItemThroughputBonus;
                }
                return bonus;
            }
        }
        #endregion
	}
}
