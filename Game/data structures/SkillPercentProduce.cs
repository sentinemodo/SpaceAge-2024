using System.Collections.Generic;

namespace SpaceAge
{
	public class SkillPercentProduce
	{
		public string Effect { get; set; }

		public int Percent { get; set; }
	}

	public class SkillPercentProduces : List<SkillPercentProduce>
	{
		public int GetPercent(string effect, int defaultPercent = 100)
		{
			foreach (SkillPercentProduce produce in this)
			{
				if (produce.Effect == effect)
				{
					return produce.Percent;
				}
			}
			return defaultPercent;
		}

		public bool HasEffect(string effect)
		{
			foreach (SkillPercentProduce produce in this)
			{
				if (produce.Effect == effect)
				{
					return true;
				}
			}
			return false;
		}
	}
}
