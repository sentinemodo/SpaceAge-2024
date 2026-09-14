using System;

namespace SpaceAge
{
	public enum EAnomalyRewardKind
	{
		SurveyBlurb,
		ResearchRp,
		Technology,
		Resource
	}

	public class AnomalyReward
	{
		public int Band { get; set; }
		public EAnomalyRewardKind Kind { get; set; }
		public string TechnologyName { get; set; }
		public string ItemName { get; set; }
		public string RegionName { get; set; }
		public int Quantity { get; set; }
		public string Text { get; set; }
	}
}
