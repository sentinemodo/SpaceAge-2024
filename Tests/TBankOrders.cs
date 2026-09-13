using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TBankOrders : TTest
	{
		[SetUp]
		public void setup()
		{
			this.LoadDefaultGame();
		}

		[TearDown]
		public void teardown()
		{
			this.ClearGame();
		}

		[Test]
		public void DepositOrder_Parse_Quantity()
		{
			ModuleStack stack = this.game.ModuleStacks["100016"];
			DepositOrder order = new DepositOrder(stack);
			order.Parse("500");

			Assert.That(order.Quantity, Is.EqualTo(500));
			Assert.That(order.AllQuantity, Is.False);
		}

		[Test]
		public void DepositOrder_Parse_All()
		{
			ModuleStack stack = this.game.ModuleStacks["100016"];
			DepositOrder order = new DepositOrder(stack);
			order.Parse("all");

			Assert.That(order.AllQuantity, Is.True);
		}

		[Test]
		public void DepositOrder_Execute_MovesCashToBank()
		{
			ModuleStack stack = this.game.ModuleStacks["100016"];
			Faction owner = stack.Owner;
			ItemType cash = ItemType.All.Cash;
			double bankBefore = owner.Bank.Balance;

			DepositOrder order = new DepositOrder(stack);
			order.Parse("500");
			this.executeOrder(stack, order, 0);

			Assert.That(order.Executed, Is.True);
			Assert.That(stack.ItemStacks[cash].Quantity, Is.EqualTo(9500));
			Assert.That(owner.Bank.Balance, Is.EqualTo(bankBefore + 500));
		}

		[Test]
		public void DepositOrder_Execute_FailsWithoutEnoughCash()
		{
			ModuleStack stack = this.game.ModuleStacks["100016"];
			Faction owner = stack.Owner;
			ItemType cash = ItemType.All.Cash;
			double bankBefore = owner.Bank.Balance;
			int cashBefore = stack.ItemStacks[cash].Quantity;

			DepositOrder order = new DepositOrder(stack);
			order.Parse("20000");
			this.executeOrder(stack, order, 0);

			Assert.That(order.Executed, Is.False);
			Assert.That(stack.ItemStacks[cash].Quantity, Is.EqualTo(cashBefore));
			Assert.That(owner.Bank.Balance, Is.EqualTo(bankBefore));
			Assert.That(this.hasEvent(stack, 0, "DEPOSIT failed"), Is.True);
		}

		[Test]
		public void WithdrawOrder_Parse_Quantity()
		{
			ModuleStack stack = this.game.ModuleStacks["100016"];
			WithdrawOrder order = new WithdrawOrder(stack);
			order.Parse("250");

			Assert.That(order.Quantity, Is.EqualTo(250));
			Assert.That(order.AllQuantity, Is.False);
		}

		[Test]
		public void WithdrawOrder_Execute_MovesBankToCash()
		{
			ModuleStack stack = this.game.ModuleStacks["100016"];
			Faction owner = stack.Owner;
			ItemType cash = ItemType.All.Cash;
			int cashBefore = stack.ItemStacks[cash].Quantity;
			double bankBefore = owner.Bank.Balance;

			WithdrawOrder order = new WithdrawOrder(stack);
			order.Parse("250");
			this.executeOrder(stack, order, 0);

			Assert.That(order.Executed, Is.True);
			Assert.That(stack.ItemStacks[cash].Quantity, Is.EqualTo(cashBefore + 250));
			Assert.That(owner.Bank.Balance, Is.EqualTo(bankBefore - 250));
		}

		[Test]
		public void WithdrawOrder_Execute_FailsWithoutBalance()
		{
			ModuleStack stack = this.game.ModuleStacks["100016"];
			Faction owner = stack.Owner;
			ItemType cash = ItemType.All.Cash;
			int cashBefore = stack.ItemStacks[cash].Quantity;
			owner.Bank.Balance = 100;

			WithdrawOrder order = new WithdrawOrder(stack);
			order.Parse("500");
			this.executeOrder(stack, order, 0);

			Assert.That(order.Executed, Is.False);
			Assert.That(stack.ItemStacks[cash].Quantity, Is.EqualTo(cashBefore));
			Assert.That(owner.Bank.Balance, Is.EqualTo(100));
			Assert.That(this.hasEvent(stack, 0, "WITHDRAW failed"), Is.True);
		}

		private bool hasEvent(ModuleStack stack, int week, string fragment)
		{
			int eventWeek = this.game.Week + week;
			foreach (EventReport eventReport in stack.EventReports)
			{
				if (eventReport.Week == eventWeek && eventReport.Description.IndexOf(fragment) >= 0)
				{
					return true;
				}
			}
			return false;
		}
	}
}
