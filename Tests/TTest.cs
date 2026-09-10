using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using SpaceAge;
using System.IO;

namespace UnitTests
{
	public class TTest : IDisposable
	{
        protected string testDir = Directory.GetCurrentDirectory();
        protected string confDir = Directory.GetCurrentDirectory();

        protected DataFile dataFile;

		protected Game game;

		protected void LoadDefaultGame()
		{
			this.dataFile = new DataFile(Directory.GetCurrentDirectory());
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadGame();
			this.game = this.dataFile.Game;
		}

		protected void ClearGame()
		{
			if (this.game != null)
				this.game.ClearDictionaries();
			Sequence.Reset();
			this.game = null;
			this.dataFile = null;
		}

        public void Dispose()
        {
            if (this.TextReader != null)
                this.TextReader.Dispose();
        }

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

        protected void consoleOutReport(string title, List<string> report, Faction faction)
        {
            Console.WriteLine(title);
            for (int i = 0; i < report.Count; i++)
            {
                Console.WriteLine(report[i]);
            }
            Console.WriteLine();
        }

        public TextReader TextReader { get; set; }

        private List<string> loadTextFile(string filename)
        {
            if (this.TextReader != null)
            {
                this.Dispose();
                this.TextReader = null;
            }

            this.TextReader = new StreamReader(Path.Combine(this.testDir, filename), System.Text.Encoding.GetEncoding(1251));
            List<string> lines = new List<string>();
            string line;
            while ((line = this.TextReader.ReadLine()) != null)
                lines.Add(line);
            return lines;
        }

        protected void consoleOutFile(string generated)
        {
            List<string> reportLines = this.loadTextFile(generated);

            Console.WriteLine("Generated file " + generated);
            for (int i = 0; i < reportLines.Count; i++)
            {
                Console.WriteLine(reportLines[i]);
            }
        }

        protected void compareFiles(string expected, string generated, bool allToConsole = true, int maxLines = int.MaxValue)
        {

            List<string> testLines = this.loadTextFile(expected);
            List<string> reportLines = this.loadTextFile(generated);

            Console.WriteLine("Generated file " + generated + " expected file " + expected);
            for (int i = 0; i < Math.Min(reportLines.Count, maxLines); i++)
            {
                if (allToConsole)
                {
                    Console.WriteLine(reportLines[i]);
                }
                Assert.That(reportLines[i], Is.EqualTo(testLines[i]), "error in file " + generated + " in line " + i + ": " + reportLines[i]);
            }
            if (maxLines == int.MaxValue)
            {
                Assert.That(reportLines.Count, Is.EqualTo(testLines.Count), "error in file " + generated + " files are differing in lenght");
            }
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
