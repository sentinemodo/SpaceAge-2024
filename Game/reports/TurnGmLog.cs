using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpaceAge
{
	[Flags]
	public enum ETurnGmLogChannel
	{
		None = 0,
		Research = 1,
		Battles = 2,
		Market = 4,
		All = Research | Battles | Market,
	}

	public class TurnGmLog
	{
		public static TurnGmLog Current { get; private set; }

		public static void Begin(ETurnGmLogChannel channels)
		{
			if (channels == ETurnGmLogChannel.None)
			{
				Current = null;
				return;
			}
			Current = new TurnGmLog(channels);
		}

		public static void Clear()
		{
			Current = null;
		}

		public static ETurnGmLogChannel ParseChannels(IEnumerable<string> tokens)
		{
			ETurnGmLogChannel channels = ETurnGmLogChannel.None;
			if (tokens == null)
			{
				return channels;
			}
			foreach (string rawToken in tokens)
			{
				if (string.IsNullOrEmpty(rawToken))
				{
					continue;
				}
				string[] parts = rawToken.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string part in parts)
				{
					string token = part.Trim().ToLowerInvariant();
					if (token == "all")
					{
						channels |= ETurnGmLogChannel.All;
					}
					else if (token == "research")
					{
						channels |= ETurnGmLogChannel.Research;
					}
					else if (token == "battles" || token == "battle")
					{
						channels |= ETurnGmLogChannel.Battles;
					}
					else if (token == "market")
					{
						channels |= ETurnGmLogChannel.Market;
					}
				}
			}
			return channels;
		}

		private readonly ETurnGmLogChannel channels;
		private readonly List<string> lines = new List<string>();

		private TurnGmLog(ETurnGmLogChannel channels)
		{
			this.channels = channels;
		}

		public bool IsEnabled(ETurnGmLogChannel channel)
		{
			if (this.channels == ETurnGmLogChannel.None)
			{
				return false;
			}
			if ((this.channels & ETurnGmLogChannel.All) == ETurnGmLogChannel.All)
			{
				return true;
			}
			return (this.channels & channel) == channel;
		}

		private List<string> BuildHeader(int turn)
		{
			List<string> header = new List<string>();
			header.Add(string.Format("GM turn log turn {0}", turn));
			header.Add(string.Format("channels: {0}", this.FormatChannels()));
			header.Add(string.Format("engine: {0}", Program.EngineVersion));
			header.Add(string.Empty);
			return header;
		}

		private string FormatChannels()
		{
			if ((this.channels & ETurnGmLogChannel.All) == ETurnGmLogChannel.All)
			{
				return "all";
			}
			List<string> names = new List<string>();
			if ((this.channels & ETurnGmLogChannel.Research) != 0)
			{
				names.Add("research");
			}
			if ((this.channels & ETurnGmLogChannel.Battles) != 0)
			{
				names.Add("battles");
			}
			if ((this.channels & ETurnGmLogChannel.Market) != 0)
			{
				names.Add("market");
			}
			return string.Join(",", names.ToArray());
		}

		public void Append(string line)
		{
			this.lines.Add(line);
		}

		public void AppendSection(string title)
		{
			this.lines.Add(string.Format("[{0}]", title));
		}

		public void LogResearchBreakthrough(
			ResearchOrder order,
			int labStackRp,
			int weeklyOutput,
			int cheapestBreakthroughCost,
			IList<int> breakthroughRolls,
			bool breakthrough,
			int preferenceRoll,
			bool usedPreferredPool,
			Technologies available,
			Technologies preferred,
			int selectionRoll,
			int selectionTotalArea,
			Technology selected)
		{
			if (!this.IsEnabled(ETurnGmLogChannel.Research))
			{
				return;
			}
			this.AppendSection("research breakthrough");
			this.Append(string.Format(
				"faction={0} lab={1} lab_stack_rp={2} weekly_output={3} research_target={4}",
				order.Researcher.Owner.Name,
				order.Researcher.Name,
				labStackRp,
				weeklyOutput,
				this.FormatResearchTarget(order)));
			this.Append(string.Format(
				"breakthrough_cost={0} breakthrough={1} rolls=[{2}]",
				cheapestBreakthroughCost,
				breakthrough ? "yes" : "no",
				this.FormatIntList(breakthroughRolls)));
			if (!breakthrough)
			{
				this.lines.Add(string.Empty);
				return;
			}
			this.Append(string.Format(
				"preference_roll={0} used_preferred_pool={1}",
				preferenceRoll,
				usedPreferredPool ? "yes" : "no"));
			this.Append(string.Format("available=[{0}]", this.FormatTechnologyList(available)));
			this.Append(string.Format("preferred=[{0}]", this.FormatTechnologyList(preferred)));
			this.Append(string.Format(
				"selection_roll={0} selection_total_area={1} selected={2}",
				selectionRoll,
				selectionTotalArea,
				selected != null ? selected.Name : "(none)"));
			this.lines.Add(string.Empty);
		}

		public void LogMarketOffer(Region region, string action, Offer offer)
		{
			if (!this.IsEnabled(ETurnGmLogChannel.Market) || region == null || offer == null)
			{
				return;
			}
			this.Append(string.Format(
				"region={0} action={1} type={2} offerent={3} item={4} module={5} tech={6} qty={7} price={8}",
				region.Name,
				action,
				offer.OfferType,
				this.FormatOfferent(offer),
				offer.ItemType != null ? offer.ItemType.Name : "-",
				offer.ModuleType != null ? offer.ModuleType.Name : "-",
				offer.Technology != null ? offer.Technology.Name : "-",
				offer.Quantity,
				offer.Price));
		}

		public void LogMarketPriceChange(Region region, NamedType type, double oldPrice, double newPrice, string reason)
		{
			if (!this.IsEnabled(ETurnGmLogChannel.Market) || region == null || type == null)
			{
				return;
			}
			if (Math.Abs(oldPrice - newPrice) < 0.001)
			{
				return;
			}
			this.Append(string.Format(
				"region={0} reason={1} type={2} price {3} -> {4}",
				region.Name,
				reason,
				type.Name,
				this.FormatPrice(oldPrice),
				this.FormatPrice(newPrice)));
		}

		public void WriteFile(string turnDir, int turn)
		{
			if (this.lines.Count == 0)
			{
				return;
			}
			List<string> output = this.BuildHeader(turn);
			output.AddRange(this.lines);
			string path = Path.Combine(turnDir, string.Format("gmturn-log.{0}.txt", turn));
			File.WriteAllLines(path, output.ToArray(), Encoding.GetEncoding(1251));
		}

		private string FormatResearchTarget(ResearchOrder order)
		{
			switch (order.ResearchType)
			{
				case EResearchType.Technology:
					return "technology " + (order.Technology != null ? order.Technology.Name : order.ResearchToken);
				case EResearchType.ItemType:
					return "item " + (order.ItemType != null ? order.ItemType.Name : order.ResearchToken);
				case EResearchType.ModuleType:
					return "module " + (order.ModuleType != null ? order.ModuleType.Name : order.ResearchToken);
				case EResearchType.Group:
					return "group " + ModuleTypeGroupXml.ToToken(order.ModuleTypesGroup);
				case EResearchType.Tag:
					return "tag " + order.ResearchToken;
				case EResearchType.Feature:
					return "feature " + order.ResearchToken;
				case EResearchType.SpaceObject:
					return "object " + order.ResearchToken;
				case EResearchType.Anomaly:
					return "anomaly " + order.ResearchToken;
				case EResearchType.ModuleStack:
					return "stack " + order.ResearchToken;
				case EResearchType.Any:
					return "any";
				default:
					return order.ResearchType.ToString();
			}
		}

		private string FormatTechnologyList(Technologies technologies)
		{
			if (technologies == null || technologies.Count == 0)
			{
				return string.Empty;
			}
			List<string> names = new List<string>();
			foreach (Technology technology in technologies)
			{
				names.Add(technology.Name);
			}
			names.Sort(StringComparer.Ordinal);
			return string.Join(", ", names.ToArray());
		}

		private string FormatIntList(IList<int> values)
		{
			if (values == null || values.Count == 0)
			{
				return string.Empty;
			}
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < values.Count; i++)
			{
				if (i > 0)
				{
					builder.Append(", ");
				}
				builder.Append(values[i]);
			}
			return builder.ToString();
		}

		private string FormatOfferent(Offer offer)
		{
			ModuleStack stack = offer.Offerent as ModuleStack;
			if (stack != null)
			{
				return stack.Name;
			}
			if (offer.Offerent != null)
			{
				return offer.Offerent.ToString();
			}
			return "-";
		}

		private string FormatPrice(double price)
		{
			return Convert.ToInt32(Math.Round(price, MidpointRounding.AwayFromZero)).ToString();
		}
	}
}
