
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
        #endregion
	}
}
