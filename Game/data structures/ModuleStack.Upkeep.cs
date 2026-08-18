using System;

namespace SpaceAge
{
	public partial class ModuleStack
	{
		// Deduct medici for wounded/mad crew on this stack only. Unsupplied
		// crew roll catalog no-consume death. Food and terair are not deducted.
		public void ExecuteMedicalConsume(int week)
		{
			if (!ItemType.All.ContainsKey("medici"))
			{
				return;
			}

			ItemType medici = ItemType.All["medici"];
			this.consumeMedicalRace(week, "wndtrn", medici);
			this.consumeMedicalRace(week, "madtrn", medici);
		}

		private void consumeMedicalRace(int week, string raceName, ItemType medici)
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

			int unsupplied = wounded - (mediciConsumed / mediciPerPerson);
			if (unsupplied < 1)
			{
				return;
			}

			if (race.NoConsumeEffect != "death" || race.NoConsumeChance < 1)
			{
				return;
			}

			int died = 0;
			for (int i = 0; i < unsupplied; i++)
			{
				int roll = Sequence.GenerateRandomInt(0, 100, "medical consume death");
				if (roll < race.NoConsumeChance)
				{
					died++;
				}
			}
			if (died < 1)
			{
				return;
			}

			this.itemStacks.Minus(raceItem, died);
			this.EventReports.Add(
				week,
				string.Format("{0} {1} died from lack of medicines.",
					died,
					(died == 1) ? race.ReportName : race.ReportNameMultiple));
		}
	}
}
