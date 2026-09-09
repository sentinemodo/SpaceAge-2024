
namespace SpaceAge
{
	public partial class ModuleStack
	{
		#region economy

		public bool HasBankAccess
		{
			get { return true; }
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
