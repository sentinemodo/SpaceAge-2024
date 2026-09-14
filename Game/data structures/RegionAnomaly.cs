using System;
using System.Collections.Generic;
using System.Xml;

namespace SpaceAge
{
	public class RegionAnomaly
	{
		public string Type { get; set; }
		public string Description { get; set; }
		public int Points { get; set; }
		public List<AnomalyReward> Rewards { get; private set; }
		public Dictionary<string, int> Progress { get; private set; }
		public Dictionary<string, int> ProgressRemainder { get; private set; }
		public HashSet<string> ResolvedFactions { get; private set; }

		public RegionAnomaly()
		{
			this.Points = 8;
			this.Rewards = new List<AnomalyReward>();
			this.Progress = new Dictionary<string, int>();
			this.ProgressRemainder = new Dictionary<string, int>();
			this.ResolvedFactions = new HashSet<string>();
		}

		public bool IsResolved(Faction faction)
		{
			return faction != null && this.ResolvedFactions.Contains(faction.Name);
		}

		public int GetProgress(Faction faction)
		{
			if (faction == null)
			{
				return 0;
			}
			int progress;
			this.Progress.TryGetValue(faction.Name, out progress);
			return progress;
		}

		public void AddProgress(Faction faction, int increment, out int applied)
		{
			applied = 0;
			if (faction == null || increment < 1 || this.IsResolved(faction))
			{
				return;
			}

			int remainder;
			this.ProgressRemainder.TryGetValue(faction.Name, out remainder);
			remainder += increment;
			applied = remainder / 100;
			remainder %= 100;
			this.ProgressRemainder[faction.Name] = remainder;

			if (applied < 1)
			{
				return;
			}

			int progress = this.GetProgress(faction) + applied;
			this.Progress[faction.Name] = progress;
		}

		public void MarkResolved(Faction faction)
		{
			if (faction != null)
			{
				this.ResolvedFactions.Add(faction.Name);
			}
		}

		public static EAnomalyRewardKind ParseRewardKind(string kind)
		{
			switch (kind)
			{
				case "survey-blurb":
					return EAnomalyRewardKind.SurveyBlurb;
				case "research-rp":
					return EAnomalyRewardKind.ResearchRp;
				case "technology":
					return EAnomalyRewardKind.Technology;
				case "resource":
					return EAnomalyRewardKind.Resource;
				default:
					throw new Exception("Unknown anomaly reward kind: " + kind);
			}
		}

		public void LoadXml(XmlElement elAnomaly, DataFile dataFile)
		{
			this.Type = elAnomaly.GetAttribute("type");
			if (elAnomaly.HasAttribute("description"))
			{
				this.Description = elAnomaly.GetAttribute("description");
			}
			this.Points = dataFile.XMLAssignInteger(elAnomaly.GetAttribute("points"), 8);

			foreach (XmlElement elReward in elAnomaly.SelectNodes("reward"))
			{
				AnomalyReward reward = new AnomalyReward();
				reward.Band = dataFile.XMLAssignInteger(elReward.GetAttribute("band"), 0);
				reward.Kind = ParseRewardKind(elReward.GetAttribute("kind"));
				if (elReward.HasAttribute("technology"))
				{
					reward.TechnologyName = elReward.GetAttribute("technology");
				}
				if (elReward.HasAttribute("item"))
				{
					reward.ItemName = elReward.GetAttribute("item");
				}
				if (elReward.HasAttribute("region"))
				{
					reward.RegionName = elReward.GetAttribute("region");
				}
				reward.Quantity = dataFile.XMLAssignInteger(elReward.GetAttribute("quantity"), 1);
				if (elReward.HasAttribute("text"))
				{
					reward.Text = elReward.GetAttribute("text");
				}
				this.Rewards.Add(reward);
			}

			foreach (XmlElement elProgress in elAnomaly.SelectNodes("progress"))
			{
				string factionName = elProgress.GetAttribute("faction");
				this.Progress[factionName] = dataFile.XMLAssignInteger(elProgress.GetAttribute("quantity"), 0);
			}

			foreach (XmlElement elResolved in elAnomaly.SelectNodes("resolved"))
			{
				this.ResolvedFactions.Add(elResolved.GetAttribute("faction"));
			}
		}

		public void SaveXml(XmlDocument doc, XmlElement elAnomaly)
		{
			elAnomaly.SetAttribute("type", this.Type);
			if (!string.IsNullOrEmpty(this.Description))
			{
				elAnomaly.SetAttribute("description", this.Description);
			}
			if (this.Points != 8)
			{
				elAnomaly.SetAttribute("points", this.Points.ToString());
			}

			foreach (AnomalyReward reward in this.Rewards)
			{
				XmlElement elReward = doc.CreateElement("reward");
				elAnomaly.AppendChild(elReward);
				elReward.SetAttribute("band", reward.Band.ToString());
				switch (reward.Kind)
				{
					case EAnomalyRewardKind.SurveyBlurb:
						elReward.SetAttribute("kind", "survey-blurb");
						break;
					case EAnomalyRewardKind.ResearchRp:
						elReward.SetAttribute("kind", "research-rp");
						break;
					case EAnomalyRewardKind.Technology:
						elReward.SetAttribute("kind", "technology");
						break;
					case EAnomalyRewardKind.Resource:
						elReward.SetAttribute("kind", "resource");
						break;
				}
				if (!string.IsNullOrEmpty(reward.TechnologyName))
				{
					elReward.SetAttribute("technology", reward.TechnologyName);
				}
				if (!string.IsNullOrEmpty(reward.ItemName))
				{
					elReward.SetAttribute("item", reward.ItemName);
				}
				if (!string.IsNullOrEmpty(reward.RegionName))
				{
					elReward.SetAttribute("region", reward.RegionName);
				}
				if (reward.Quantity != 1)
				{
					elReward.SetAttribute("quantity", reward.Quantity.ToString());
				}
				if (!string.IsNullOrEmpty(reward.Text))
				{
					elReward.SetAttribute("text", reward.Text);
				}
			}

			foreach (KeyValuePair<string, int> entry in this.Progress)
			{
				if (entry.Value > 0)
				{
					XmlElement elProgress = doc.CreateElement("progress");
					elAnomaly.AppendChild(elProgress);
					elProgress.SetAttribute("faction", entry.Key);
					elProgress.SetAttribute("quantity", entry.Value.ToString());
				}
			}

			foreach (string factionName in this.ResolvedFactions)
			{
				XmlElement elResolved = doc.CreateElement("resolved");
				elAnomaly.AppendChild(elResolved);
				elResolved.SetAttribute("faction", factionName);
			}
		}
	}
}
