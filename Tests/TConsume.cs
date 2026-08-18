using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TConsume : TTest
	{
		[SetUp]
		public void Setup()
		{
			this.LoadDefaultGame();
		}

		[TearDown]
		public void Teardown()
		{
			Sequence.Ints.Clear();
			this.ClearGame();
		}

		[Test]
		public void LoadConfiguration_WoundedNeedMedicinesOrDie()
		{
			Race wounded = Race.All["wndtrn"];
			Assert.That(wounded.Consume.ContainsKey(ItemType.All["medici"]));
			Assert.That(wounded.NoConsumeEffect, Is.EqualTo("death"));
			Assert.That(wounded.NoConsumeChance, Is.EqualTo(25));
		}

		[Test]
		public void ExecuteMedicalConsume_WithoutMedicines_DoesNotKillDuringWeek()
		{
			ModuleStack stack = ModuleStack.All["000005"];
			stack.ItemStacks.Add(new ItemStack(ItemType.All["wndtrn"], 4));
			Sequence.Ints.Push(0);
			Sequence.Ints.Push(0);
			Sequence.Ints.Push(0);
			Sequence.Ints.Push(0);

			this.game.Week = 1;
			this.game.ExecuteMedicalConsume();

			Assert.That(stack.ItemStacks[ItemType.All["wndtrn"]].Quantity, Is.EqualTo(4));
		}

		[Test]
		public void ExecuteMedicalConsume_WithMedicines_ConsumesMedicinesWithoutWeeklyDeath()
		{
			ModuleStack stack = ModuleStack.All["000005"];
			stack.ItemStacks.Add(new ItemStack(ItemType.All["wndtrn"], 4));
			stack.ItemStacks.Add(new ItemStack(ItemType.All["medici"], 2));
			Sequence.Ints.Push(0);
			Sequence.Ints.Push(0);

			this.game.Week = 1;
			this.game.ExecuteMedicalConsume();

			Assert.That(stack.ItemStacks.Has(ItemType.All["medici"]), Is.False);
			Assert.That(stack.ItemStacks[ItemType.All["wndtrn"]].Quantity, Is.EqualTo(4));
		}

		[Test]
		public void ExecuteQuarterlyWoundedOutcome_RollsDeathStayAndRecover()
		{
			ModuleStack stack = ModuleStack.All["000005"];
			int terranBefore = stack.ItemStacks.Quantity(ItemType.All["terran"]);
			stack.ItemStacks.Add(new ItemStack(ItemType.All["wndtrn"], 4));
			// LIFO: first pop dies (<25), next two stay (25-74), last recovers (>=75)
			Sequence.Ints.Push(80);
			Sequence.Ints.Push(50);
			Sequence.Ints.Push(50);
			Sequence.Ints.Push(0);

			this.game.ExecuteQuarterlyWoundedOutcome();

			Assert.That(stack.ItemStacks[ItemType.All["wndtrn"]].Quantity, Is.EqualTo(2));
			Assert.That(stack.ItemStacks.Quantity(ItemType.All["terran"]), Is.EqualTo(terranBefore + 1));
			bool diedEvent = false;
			bool recoveredEvent = false;
			foreach (EventReport eventReport in stack.EventReports)
			{
				if (eventReport.Week == 13 && eventReport.Description.IndexOf("died of their wounds") >= 0)
				{
					diedEvent = true;
				}
				if (eventReport.Week == 13 && eventReport.Description.IndexOf("recovered from their wounds") >= 0)
				{
					recoveredEvent = true;
				}
			}
			Assert.That(diedEvent, Is.True);
			Assert.That(recoveredEvent, Is.True);
		}

		[Test]
		public void ExecuteMedicalConsume_DoesNotDeductFoodOrAir()
		{
			ModuleStack stack = ModuleStack.All["000005"];
			int foodBefore = stack.ItemStacks.ContainsKey(ItemType.All["food"])
				? stack.ItemStacks[ItemType.All["food"]].Quantity
				: 0;
			stack.ItemStacks.Add(new ItemStack(ItemType.All["wndtrn"], 1));
			Sequence.Ints.Push(99);

			this.game.Week = 1;
			this.game.ExecuteMedicalConsume();

			int foodAfter = stack.ItemStacks.ContainsKey(ItemType.All["food"])
				? stack.ItemStacks[ItemType.All["food"]].Quantity
				: 0;
			Assert.That(foodAfter, Is.EqualTo(foodBefore));
			Assert.That(stack.ItemStacks[ItemType.All["wndtrn"]].Quantity, Is.EqualTo(1));
		}
	}
}
