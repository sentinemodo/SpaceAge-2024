using System;
using System.Collections.Generic;

namespace SpaceAge
{
	// Research feature mechanics, kept in one file for easy management:
	//  - technology cost model (per-level default, overridable in the datafile),
	//  - weekly research-point output of a research modulestack,
	//  - breakthrough probability (constant weekly hazard).
	//
	// Costs double per level (L1=8, L2=16, L3=32, ...). Level 0 technologies are
	// the free starting set and are never researched. With a single base research
	// lab (research-output 1, one module -> output 1) the constant weekly hazard
	// p = 1 - (1 - 1/cost)^output gives a cumulative breakthrough chance of ~80%
	// after 1 quarter (L1), 2 quarters (L2) and 4 quarters (L3).
	public static class Research
	{
		public const int BaseCost = 8;

		public static int DefaultCostForLevel(int level)
		{
			if (level <= 0)
			{
				return 0;
			}
			return BaseCost * (1 << (level - 1));
		}

		public static int CostOf(Technology technology)
		{
			return technology.Cost;
		}

		// Research points produced per week = lab research-output x number of modules.
		public static int WeeklyOutput(ModuleStack researcher)
		{
			if (researcher == null || researcher.ModuleType == null)
			{
				return 0;
			}
			return researcher.ModuleType.ResearchOutput * researcher.Modules.Count;
		}

		// Weekly breakthrough roll against the cheapest available technology: each of
		// the `output` research points produced this week is an independent 1/cost
		// chance, i.e. p_week = 1 - (1 - 1/cost)^output. Rolls go through Sequence so
		// tests are deterministic (push 0 to force a breakthrough).
		public static bool RollBreakthrough(Technologies available, int output)
		{
			if (available == null || available.Count == 0 || output <= 0)
			{
				return false;
			}

			int cheapestCost = int.MaxValue;
			foreach (Technology technology in available)
			{
				int cost = CostOf(technology);
				if (cost > 0 && cost < cheapestCost)
				{
					cheapestCost = cost;
				}
			}
			if (cheapestCost == int.MaxValue)
			{
				return false;
			}

			for (int point = 0; point < output; point++)
			{
				if (Sequence.GenerateRandomInt(0, cheapestCost, "research breakthrough") == 0)
				{
					return true;
				}
			}
			return false;
		}
	}
}
