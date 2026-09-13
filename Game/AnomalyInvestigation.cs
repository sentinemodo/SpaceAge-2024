using System;
using System.Collections.Generic;

namespace SpaceAge
{
	public static class AnomalyInvestigation
	{
		public static int FactionTechBand(Faction faction)
		{
			if (faction == null)
			{
				return 0;
			}

			int maxLevel = faction.MaxTechnologyLevel;
			if (maxLevel <= 1)
			{
				return 0;
			}
			if (maxLevel <= 3)
			{
				return 2;
			}
			if (maxLevel <= 5)
			{
				return 4;
			}
			if (maxLevel <= 7)
			{
				return 6;
			}
			return 8;
		}

		public static int WeeklyProgress(ModuleStack researcher, Region region)
		{
			if (researcher == null || region == null || region.Anomaly == null)
			{
				return 0;
			}

			int hundredths = ModuleStackContribution(researcher, region.Anomaly.Type);
			hundredths += researcher.ResearchItemThroughputBonus * 100;
			return hundredths;
		}

		private static int ModuleStackContribution(ModuleStack stack, string anomalyType)
		{
			int hundredths = 0;
			if (stack.ModuleType != null && stack.ModuleType.Group == EModuleTypesGroup.research)
			{
				hundredths += ModuleContribution(
					stack.ModuleType,
					anomalyType,
					stack.Modules.Count);
			}

			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				hundredths += ModuleStackContribution(nested, anomalyType);
			}

			return hundredths;
		}

		private static int ModuleContribution(ModuleType moduleType, string anomalyType, int moduleCount)
		{
			if (moduleType == null || moduleCount < 1 || moduleType.InvestigationOutput < 1)
			{
				return 0;
			}

			int baseUnits = moduleType.InvestigationOutput * moduleCount * 100;
			if (TypeMatches(moduleType, anomalyType))
			{
				return baseUnits;
			}
			return baseUnits / 2;
		}

		public static bool TypeMatches(ModuleType moduleType, string anomalyType)
		{
			if (moduleType == null || string.IsNullOrEmpty(anomalyType))
			{
				return false;
			}

			if (moduleType.InvestigationTypes.Count == 0)
			{
				return DefaultTypeMatch(moduleType.Name, anomalyType);
			}

			foreach (string investigationType in moduleType.InvestigationTypes)
			{
				if (string.Equals(investigationType, anomalyType, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		private static bool DefaultTypeMatch(string moduleName, string anomalyType)
		{
			switch (moduleName)
			{
				case "moblab":
					return anomalyType == "spectral" || anomalyType == "radiometric";
				case "optlab":
				case "survsc":
				case "bolsen":
					return anomalyType == "spectral";
				case "seissc":
					return anomalyType == "seismic" || anomalyType == "gravimetric";
				case "maglab":
					return anomalyType == "magnetic";
				case "radlab":
					return anomalyType == "radiometric";
				case "dpsens":
					return anomalyType == "gravimetric" || anomalyType == "anomaly";
				default:
					return false;
			}
		}

		public static void Complete(ModuleStack researcher, Region region, int week)
		{
			if (researcher == null || region == null || region.Anomaly == null)
			{
				return;
			}

			Faction faction = researcher.Owner;
			if (faction == null || region.Anomaly.IsResolved(faction))
			{
				return;
			}

			int band = FactionTechBand(faction);
			List<string> messages = new List<string>();

			foreach (AnomalyReward reward in region.Anomaly.Rewards)
			{
				if (reward.Band > band)
				{
					continue;
				}

				ApplyReward(reward, researcher, region, week, messages);
			}

			region.Anomaly.MarkResolved(faction);

			if (messages.Count > 0)
			{
				researcher.EventReports.Add(
					week,
					string.Format("completed anomaly survey at {0}: {1}",
						region.ReportName,
						string.Join("; ", messages.ToArray())));
			}
			else
			{
				researcher.EventReports.Add(
					week,
					string.Format("completed anomaly survey at {0}.", region.ReportName));
			}
		}

		private static void ApplyReward(
			AnomalyReward reward,
			ModuleStack researcher,
			Region region,
			int week,
			List<string> messages)
		{
			switch (reward.Kind)
			{
				case EAnomalyRewardKind.SurveyBlurb:
					messages.Add("survey report recorded");
					break;
				case EAnomalyRewardKind.ResearchRp:
					if (!string.IsNullOrEmpty(reward.TechnologyName)
						&& Technology.All.Contains(reward.TechnologyName))
					{
						researcher.ResearchPoints += reward.Quantity;
						messages.Add(string.Format(
							"+{0} research points toward {1}",
							reward.Quantity,
							Technology.All[reward.TechnologyName].ReportName));
					}
					break;
				case EAnomalyRewardKind.Technology:
					if (!string.IsNullOrEmpty(reward.TechnologyName)
						&& Technology.All.Contains(reward.TechnologyName))
					{
						Technology technology = Technology.All[reward.TechnologyName];
						if (researcher.Technologies[technology.Name] == null)
						{
							researcher.ReceiveTechnologyCopy(technology, week, null);
							messages.Add(string.Format("received {0}", technology.ReportName));
						}
						else
						{
							researcher.ResearchPoints += 16;
							messages.Add(string.Format(
								"+16 research points (already know {0})",
								technology.ReportName));
						}
					}
					break;
				case EAnomalyRewardKind.Resource:
					if (!string.IsNullOrEmpty(reward.ItemName) && ItemType.All.ContainsKey(reward.ItemName))
					{
						Region target = region;
						if (!string.IsNullOrEmpty(reward.RegionName) && Region.All.ContainsKey(reward.RegionName))
						{
							target = Region.All[reward.RegionName];
						}
						target.Resources.AddOrIncrease(ItemType.All[reward.ItemName], reward.Quantity);
						messages.Add(string.Format(
							"confirmed {0} {1} at {2}",
							reward.Quantity,
							ItemType.All[reward.ItemName].ReportName,
							target.ReportName));
					}
					break;
			}
		}
	}
}
