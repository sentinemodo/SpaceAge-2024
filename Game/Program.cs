using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace SpaceAge
{
	public class Program
	{
		public const string EngineVersion = "0.1.158";

		public static void Main(string[] args)
		{
#if RELEASE
			try 
			{
#endif
			string game_dir = Directory.GetCurrentDirectory();
			string turn_dir = Directory.GetCurrentDirectory();
			string order_to_check = null;
			bool noTurn = false;
			bool reportsOnly = false;
			string battleSimFile = null;
			string battleSimOutput = null;
			int? battleSimSeed = null;

			for (int i = 0; i < args.Length; i++)
			{
				if (args[i] == "/no-turn")
					noTurn = true;
				else if (args[i] == "/reports")
					reportsOnly = true;
				else if (args[i] == "/battle-sim" && i < args.Length - 1)
				{
					battleSimFile = args[++i];
					if (i < args.Length - 1 && !args[i + 1].StartsWith("/"))
					{
						battleSimOutput = args[++i];
					}
				}
				else if (i < args.Length - 1)
				{
					if (args[i] == "/data")
					{
						game_dir = args[++i];
					}
					else if (args[i] == "/turn-dir")
					{
						turn_dir = args[++i];
					}
					else if (args[i] == "/check")
					{
						order_to_check = args[++i];
					}
					else if (args[i] == "/seed")
					{
						battleSimSeed = Convert.ToInt32(args[++i]);
					}
				}
			}

			if (battleSimFile != null)
			{
				BattleSimulatorRunner runner = new BattleSimulatorRunner(game_dir);
				string output = runner.Run(battleSimFile, battleSimSeed);
				if (string.IsNullOrEmpty(battleSimOutput))
				{
					battleSimOutput = Path.ChangeExtension(battleSimFile, ".out.txt");
				}
				File.WriteAllText(battleSimOutput, output, Encoding.GetEncoding(1251));
				Console.Write(output);
				return;
			}

			Console.WriteLine("SpaceAge " + EngineVersion);
			Console.WriteLine("");

			DataFile dataFile = new DataFile(game_dir);
			Console.WriteLine("Loading configuration");
			dataFile.LoadConfiguration();
			Console.WriteLine("Loading game data");
			dataFile.LoadGame();
			Game game = dataFile.Game;

			if (order_to_check == null)
			{
				if (reportsOnly)
				{
					Console.WriteLine("Generating reports");
					ReportWriter reportsWriter = new ReportWriter(game, dataFile, turn_dir);
					reportsWriter.GenerateReports(turn_dir);
				}
				else if (noTurn)
				{
					Console.WriteLine("Loading orders");
					OrdersReader ordersReader = new OrdersReader(game);
					ordersReader.Load(turn_dir);
					Console.WriteLine("Applying between-turn orders");
					game.ExecuteBetweenTurnOrders();
					Contract.All.WriteAnnouncements(turn_dir, game);
					Console.WriteLine("Saving game");
					dataFile.SaveGame();
				}
				else
				{
					Console.WriteLine("Processing requests");
					Request.Load(turn_dir);
					Console.WriteLine("Loading game events");
					EventsReaders.Load(game, turn_dir);
					game.Events.Execute();
					Console.WriteLine("Loading orders");
					OrdersReader ordersReader = new OrdersReader(game);
					ordersReader.Load(turn_dir);
					Console.WriteLine("Processing game turn");
					game.Execute();
					Console.WriteLine("Generating reports");

					ReportWriter reportsWriter = new ReportWriter(game, dataFile, turn_dir);
					reportsWriter.GenerateReports(turn_dir);

					Console.WriteLine("Saving game");
					dataFile.SaveGame();
				}
			}
			else
			{
				Console.Write("Checking order");
				OrdersReader.Check(turn_dir);
			}
#if RELEASE
			}
			catch (Exception ex) 
			{
				TextWriter tw = new StreamWriter("error.log", true, System.Text.Encoding.GetEncoding(1251));
				tw.WriteLine(ex.Message);
				tw.WriteLine(ex.StackTrace);
				tw.WriteLine();
				tw.Close();
				Environment.ExitCode = 1;
			}
#endif
		}
	}
}
