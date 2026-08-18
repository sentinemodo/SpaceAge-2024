using System;

namespace SpaceAge
{
	public partial class ModuleStack
	{
		// Deduct medici for wounded/mad crew on this stack only. Food and
		// terair are not deducted. Unsupplied crew no longer die weekly;
		// wounded terrans roll die/stay/recover at quarter end.
		public void ExecuteMedicalConsume(int week)
		{
			if (!ItemType.All.ContainsKey("medici"))
			{
				return;
			}

			ItemType medici = ItemType.All["medici"];
			this.consumeMedicalRace("wndtrn", medici);
			this.consumeMedicalRace("madtrn", medici);
		}

		public void ExecuteQuarterlyWoundedOutcome(int week)
		{
			if (!ItemType.All.ContainsKey("wndtrn") || !ItemType.All.ContainsKey("terran"))
			{
				return;
			}

			ItemType woundedType = ItemType.All["wndtrn"];
			if (!this.itemStacks.ContainsKey(woundedType) || this.itemStacks[woundedType].Quantity < 1)
			{
				return;
			}

			Race race = Race.All.ContainsKey("wndtrn") ? Race.All["wndtrn"] : null;
			int wounded = this.itemStacks[woundedType].Quantity;
			int died = 0;
			int recovered = 0;
			for (int i = 0; i < wounded; i++)
			{
				int roll = Sequence.GenerateRandomInt(0, 100, "quarterly wounded outcome");
				if (roll < 25)
				{
					died++;
				}
				else if (roll >= 75)
				{
					recovered++;
				}
			}

			if (died > 0)
			{
				this.itemStacks.Minus(woundedType, died);
				string name = (race == null)
					? woundedType.ReportName
					: ((died == 1) ? race.ReportName : race.ReportNameMultiple);
				this.EventReports.Add(
					week,
					string.Format("{0} {1} died of their wounds.", died, name));
			}

			if (recovered > 0)
			{
				this.itemStacks.Minus(woundedType, recovered);
				this.itemStacks.Add(new ItemStack(ItemType.All["terran"], recovered));
				string name = (race == null)
					? woundedType.ReportName
					: ((recovered == 1) ? race.ReportName : race.ReportNameMultiple);
				this.EventReports.Add(
					week,
					string.Format("{0} {1} recovered from their wounds.", recovered, name));
			}
		}

		private void consumeMedicalRace(string raceName, ItemType medici)
		{
			if (!ItemType.All.ContainsKey(raceName) || !Race.All.ContainsKey(raceName))
			{
				return;
			}

			ItemType raceItem = ItemType.All[raceName];
			if (!this.itemStacks.ContainsKey(raceItem) || this.itemStacks[raceItem].Quantity < 1)
			{
				return;
			}

			Race race = Race.All[raceName];
			int mediciPerPerson = 0;
			if (race.Consume != null && race.Consume.ContainsKey(medici))
			{
				mediciPerPerson = race.Consume[medici].Quantity;
			}
			if (mediciPerPerson < 1)
			{
				return;
			}

			int wounded = this.itemStacks[raceItem].Quantity;
			int mediciNeeded = wounded * mediciPerPerson;
			int mediciAvailable = this.itemStacks.ContainsKey(medici)
				? this.itemStacks[medici].Quantity
				: 0;
			int mediciConsumed = Math.Min(mediciNeeded, mediciAvailable);
			if (mediciConsumed > 0)
			{
				this.itemStacks.Minus(medici, mediciConsumed);
			}
		}
	}
}
