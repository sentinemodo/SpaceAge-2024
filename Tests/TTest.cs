using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Core;
using SpaceAge;

namespace UnitTests
{
	public class TTest
	{
		protected DataFile datafile;
		protected Game game;

		protected void consoleOutReport(string title, IReporting reporting, Faction faction)
		{
			List<string> report = reporting.Report(faction);
			Console.WriteLine(title);
			for (int i = 0; i < report.Count; i++)
			{
				Console.WriteLine(report[i]);
			}
			Console.WriteLine();
		}

		protected void executeOrder(ModuleStack moduleStack, Order order, int week)
		{
			moduleStack.ExecutedLongOrder = false;
			order.Execute(this.game.Week + week);
			moduleStack.Orders.RemoveExecuted();
			moduleStack.Effects.Execute(this.game.Week + week);
			moduleStack.Effects.RemoveExecuted();
		}

		protected void executeOrder(Person person, Order order, int week)
		{
			person.ExecutedLongOrder = false;
			order.Execute(this.game.Week + week);
			person.Orders.RemoveExecuted();
			person.Effects.Execute(this.game.Week + week);
			person.Effects.RemoveExecuted();
		}

	}
}
