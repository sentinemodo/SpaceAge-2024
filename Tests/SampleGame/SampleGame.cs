using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;
using UnitTests;

namespace IntegrationTests
{
	[TestFixture]
    public class SampleGame : TTest, IDisposable
	{
		private string testDir;
		private string confDir;
		private DataFile dataFile;

        public TextReader TextReader { get; set; }
        public void Dispose()
        {
            this.TextReader.Dispose();
        }

        public SampleGame()
		{			
		}

		[SetUp]
		public void SetupGame()
		{
			this.testDir = string.Concat(Directory.GetCurrentDirectory(), "/SampleGame");
			this.confDir = Directory.GetCurrentDirectory();
			this.dataFile = new DataFile(this.testDir);
			this.game = new Game();
		}

		[TearDown]
		public void TeardownGame()
		{
			this.game.ClearDictionaries();
			this.game = null;
			this.dataFile = null;
		}

		[Test]
		public void SetupTeardown()
		{
            Assert.That(true);
		}

		public void LoadGalaxy(string gameinFileName)
		{
			this.dataFile.LoadGameDocument(this.testDir, gameinFileName);
			this.dataFile.LoadConfiguration(this.confDir);
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
            this.dataFile.LoadOrders();

            this.game = this.dataFile.Game;
            Assert.That(this.game.Galaxy, Is.Not.Null);
            Assert.That(this.game.Galaxy.SpaceSystems.Count, Is.EqualTo(1));
            Assert.That(this.game.Galaxy.SpaceSystems[0].FullName, Is.EqualTo("Sol"));

			SpaceSystem system = this.game.Galaxy.SpaceSystems[0];
            Assert.That(system.Objects, Is.Not.Null);
            Assert.That(system.Objects.Count, Is.EqualTo(2));
			ClassicAssert.IsInstanceOf(typeof(Star), system.Objects["S00001"]);
			ClassicAssert.IsInstanceOf(typeof(Planet), system.Objects["P00001"]);
		}


		[Test]
		public void _1_LoadGameIn1()
		{
			this.LoadGalaxy("gamein.1.xml");

			// story:
			// Single star system
			// Single planet
			// single moon
			// two region on the planet
			// two factions

			// sequence:
			// validate input faction reports
			ReportWriter reportsWriter = new ReportWriter(this.game, this.dataFile, this.testDir);
			reportsWriter.GenerateReports(this.testDir);

            this.compareFiles("testreport.1.1.txt", "report.1.1.txt");
			this.compareFiles("testreport.1.2.txt", "report.1.2.txt");
			this.compareFiles("testreport.1.3.txt", "report.1.3.txt");
		}

		[Test]
		public void _2_SaveGameIn1()
		{
			this.LoadGalaxy("gamein.1.xml");
			this.dataFile.SaveGame(this.testDir, "gamein.1_saved.xml");

            this.consoleOutFile("gamein.1_saved.xml");
			this.compareFiles("gamein.1.xml", "gamein.1_saved.xml");

			this.game.ClearDictionaries();
			this.dataFile = new DataFile(this.testDir);
			this.game = new Game();

			this.LoadGalaxy("gamein.1_saved.xml");
			ReportWriter reportsWriter = new ReportWriter(this.game, this.dataFile, this.testDir);
			reportsWriter.GenerateReports(this.testDir);

			this.compareFiles("testreport.1.1.txt", "report.1.1.txt");
			this.compareFiles("testreport.1.2.txt", "report.1.2.txt");
			this.compareFiles("testreport.1.3.txt", "report.1.3.txt");			
		}

		[Test]
		public void _2_SaveGameIn1_orders()
		{
            Sequence.Ints.Push(110);
            Sequence.Ints.Push(109);
            Sequence.Ints.Push(108);
            Sequence.Ints.Push(107);
            Sequence.Ints.Push(106);
            Sequence.Ints.Push(105);
			Sequence.Ints.Push(104);
			Sequence.Ints.Push(103);
			Sequence.Ints.Push(102);
			Sequence.Ints.Push(101);
			Sequence.Ints.Push(100);
			
			this.LoadGalaxy("gamein.1.xml");
			
			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.LoadOrders(Path.Combine(this.testDir, "orders.1.2.txt"), false);
			ordersReader.LoadOrders(Path.Combine(this.testDir, "orders.1.3.txt"), false);

			this.dataFile.SaveGame(this.testDir, "gamein.1_saved_orders.xml");
            this.consoleOutFile("gamein.1_saved_orders.xml");
			this.compareFiles("gamein.1_orders.xml", "gamein.1_saved_orders.xml");
		}

		[Test]
		public void _3_ExecuteTurn1()
		{
            Sequence.Ints.Push(110);
            Sequence.Ints.Push(109);
            Sequence.Ints.Push(108);
            Sequence.Ints.Push(107);
			Sequence.Ints.Push(106);
			Sequence.Ints.Push(105);
			Sequence.Ints.Push(104);
			Sequence.Ints.Push(103);
			Sequence.Ints.Push(102);
			Sequence.Ints.Push(101);
			Sequence.Ints.Push(100);

			this.LoadGalaxy("gamein.1.xml");

			// story:
			// one faction build ship
			// the other build small army			

			// sequence:        
			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.LoadOrders(Path.Combine(this.testDir, "orders.1.2.txt"), false);
			ordersReader.LoadOrders(Path.Combine(this.testDir, "orders.1.3.txt"), false);

			//this.game.Execute();
			#region this.game.Execute()			
			this.game.Turn++;
			Console.WriteLine("Turn: " + this.game.Turn);
			for (this.game.Week = 1; this.game.Week <= 13; this.game.Week++)
			{
				Console.WriteLine("  Week: " + this.game.Week);
				this.game.ClearExecutedLongOrder();
                //this.game.ClearFailedToExecuteImmediateOrders();
                this.game.ClearExecutedImmediateOrders();
				
                #region this.game.ExecuteOrders();
				//this.game.ExecuteOrders();
				foreach (Faction faction in this.game.Factions.Values)
				{
					Console.WriteLine("    Faction: " + faction.ToString());
					faction.Execute(this.game.Week);
				}
				bool executedOrderByModuleStack;
				executedOrderByModuleStack = true;
				ModuleStacks moduleStacks;

                Console.WriteLine("---------------------------starting orders by modules");
				while (executedOrderByModuleStack)
				{
					executedOrderByModuleStack = false;
                    moduleStacks = this.game.ModuleStacks.HavingOrders;					
                    Console.WriteLine("---------------starting modules pass " + moduleStacks.Count + " modulestacks have orders");
					foreach (ModuleStack moduleStack in moduleStacks.Values)
					{
						Console.WriteLine("    ModuleStack: " + moduleStack.ToString());

						#region moduleStack.Execute
						//moduleStack.Execute(this.game.Week);
						#region Orders.Execute
						//moduleStack.Orders.Execute(this.game.Week);
						bool executedOrder;
						#region executedOrder = moduleStack.Orders.ExecuteList(this.game.Week, moduleStack.Orders.Immediate);
						executedOrder = true;
						while (executedOrder)
						{
							//executedOrder = moduleStack.Orders.ExecuteList(this.game.Week, moduleStack.Orders.Immediate);
							executedOrder = false;
							foreach (Order order in moduleStack.Orders.Immediate)
							{
								Console.WriteLine("      Immediate Order: " + order.Report(moduleStack.Owner)[0]);
								if (!order.Executed & order.ConditionalOrders.Count == 0)
								{
									order.Execute(this.game.Week);
									if (order.Executed)
									{
										moduleStack.Orders.RemoveConditions(order);
										executedOrder = true;
										Console.WriteLine("        executed");
									}
								}
							}							
							if (executedOrder)
							{
								executedOrderByModuleStack = true;
							}
						}
						#endregion

						#region executedOrder = moduleStack.Orders.ExecuteList(this.game.Week, moduleStack.Orders.Long);
						//executedOrder = moduleStack.Orders.ExecuteList(this.game.Week, moduleStack.Orders.Long);
						executedOrder = false;
						foreach (Order order in moduleStack.Orders.Long)
						{
                            Console.WriteLine("      Long Order: " + order.Report(moduleStack.Owner)[0]);
							if (!order.Subject.ExecutedLongOrder & !order.Executed & order.ConditionalOrders.Count == 0)
							{
								Console.WriteLine("        started to execute");
									order.Execute(this.game.Week);
									if (order.Executed)
									{
										moduleStack.Orders.RemoveConditions(order);
									}
								
									if (order.Executed | order.Executing)
									{
										executedOrder = true;
										if (order.Executed) 
										{
											Console.WriteLine("        executed");
										} else if (order.Executing)
										{
											Console.WriteLine("        executing");
                                            if (order is UseOrder) 
                                            {

                                                Console.WriteLine(((UseOrder)order).DurationInitial);
                                                Console.WriteLine(((UseOrder)order).DurationLeft);
                                            }
										} else 
										{
											Console.WriteLine("        executed and executing ? STRANGE");
										}
										order.Subject.ExecutedLongOrder = true;
									}
							}
							else
							{
								Console.WriteLine("      !order.Executed: " + !order.Executed);
								Console.WriteLine("      order.ConditionalOrders.Count: " + order.ConditionalOrders.Count);
								Console.WriteLine("      order.Subject.ExecutedLongOrder: " + order.Subject.ExecutedLongOrder);
							}
						}
						#endregion

						#region executedOrder = moduleStack.Orders.ExecuteList(this.game.Week, moduleStack.Orders.Immediate);
						executedOrder = true;
						while (executedOrder)
						{
							//executedOrder = moduleStack.Orders.ExecuteList(this.game.Week, moduleStack.Orders.Immediate);
							executedOrder = false;
							foreach (Order order in moduleStack.Orders.Immediate)
							{
                                Console.WriteLine("      Immediate Order: " + order.Report(moduleStack.Owner)[0]);
								if (!order.Executed & order.ConditionalOrders.Count == 0)
								{
									order.Execute(this.game.Week);
									if (order.Executed)
									{
										moduleStack.Orders.RemoveConditions(order);
										executedOrder = true;
										Console.WriteLine("        executed");
									}
								}
							}
							if (executedOrder)
							{
								executedOrderByModuleStack = true;
							}
						}
						#endregion

						moduleStack.Orders.RemoveExecuted();
						#endregion
						moduleStack.Effects.Execute(this.game.Week);
						moduleStack.Effects.RemoveExecuted();
						#endregion
					}
				}
                Console.WriteLine("---------------------------starting orders by Person");
                bool executedOrderByPerson;
                executedOrderByPerson = true;

                People peopleHavingOrders;
                while (executedOrderByPerson)
                {
                    executedOrderByPerson = false;
                    peopleHavingOrders = this.game.People.HavingOrders;
                    Console.WriteLine("---------------starting people pass " + peopleHavingOrders.Count + " people have orders");

                    foreach (Person person in peopleHavingOrders.Values)
                    {
                      Console.WriteLine("    Person: " + person.ToString() + " having " + person.Orders.Count + " orders");
                      #region person.Execute
                      bool executedOrder;
											#region executedOrder = person.Orders.ExecuteList(this.game.Week, person.Orders.Immediate);
											executedOrder = true;
											while (executedOrder)
											{
												executedOrder = false;
												foreach (Order order in person.Orders.Immediate)
												{
													Console.WriteLine("      Immediate Order: " + order.ToString());
													if (!order.Executed & order.ConditionalOrders.Count == 0)
													{
														order.Execute(this.game.Week);
														if (order.Executed)
														{
															person.Orders.RemoveConditions(order);
															executedOrder = true;
															Console.WriteLine("        executed");
														}
													}
												}
												if (executedOrder)
												{
													executedOrderByPerson = true;
												}
											}
											#endregion
												
											#region executedOrder = person.Orders.ExecuteList(this.game.Week, person.Orders.Long);
												//executedOrder = person.Orders.ExecuteList(this.game.Week, person.Orders.Long);
						executedOrder = false;
						foreach (Order order in person.Orders.Long)
						{
							Console.WriteLine("      Long Order: " + order.ToString());
							if (!order.Subject.ExecutedLongOrder & !order.Executed & order.ConditionalOrders.Count == 0)
							{
								Console.WriteLine("        started to execute");
									order.Execute(this.game.Week);
									if (order.Executed)
									{
										person.Orders.RemoveConditions(order);
									}
								
									if (order.Executed | order.Executing)
									{
										executedOrder = true;
										if (order.Executed) 
										{
											Console.WriteLine("        executed");
										} else if (order.Executing)
										{
											Console.WriteLine("        executing");
										} else 
										{
											Console.WriteLine("        executed and executing ? STRANGE");
										}
										order.Subject.ExecutedLongOrder = true;
									}
							}
							else
							{
								Console.WriteLine("      !order.Executed: " + !order.Executed);
								Console.WriteLine("      order.ConditionalOrders.Count: " + order.ConditionalOrders.Count);
								Console.WriteLine("      order.Subject.ExecutedLongOrder: " + order.Subject.ExecutedLongOrder);
							}
						}
						#endregion

						#region executedOrder = person.Orders.ExecuteList(this.game.Week, person.Orders.Immediate);
						executedOrder = true;
						while (executedOrder)
						{
							executedOrder = false;
							foreach (Order order in person.Orders.Immediate)
							{
								Console.WriteLine("      Immediate Order: " + order.ToString());
								if (!order.Executed & order.ConditionalOrders.Count == 0)
								{
									order.Execute(this.game.Week);
									if (order.Executed)
									{
										person.Orders.RemoveConditions(order);
										executedOrder = true;
										Console.WriteLine("        executed");
									}
								}
							}
							if (executedOrder)
							{
								executedOrderByPerson = true;
							}
						}
						#endregion

                        #endregion


                        person.Orders.RemoveExecuted();
                        person.Effects.Execute(this.game.Week);
                        person.Effects.RemoveExecuted();

                        if (executedOrder)
                        {
                            executedOrderByPerson = true;
                        }
                    }
                }
            }
            #endregion
			this.game.ClearExecutedLongOrder();
			//this.game.ClearFailedToExecuteImmediateOrders();
			this.game.ClearExecutedImmediateOrders();
            this.game.ClearUnformed();
			this.game.Week = 1;
			Console.WriteLine("  Week: " + this.game.Week);
			#endregion

			// validate output reports
			ReportWriter reportsWriter = new ReportWriter(this.game, this.dataFile, this.testDir);
			reportsWriter.GenerateReports(this.testDir);

            this.compareFiles("testreport.2.3.txt", "report.2.3.txt", false);

			this.compareFiles("testreport.2.1.txt", "report.2.1.txt");
			this.compareFiles("testreport.2.2.txt", "report.2.2.txt");

			//   validate output game file
			this.dataFile.SaveGame(this.testDir, "gameout.1_saved.xml");
            this.consoleOutFile("gameout.1_saved.xml");

			this.compareFiles("gameout.1.xml", "gameout.1_saved.xml");
            this.copyFile("gameout.1_saved.xml", "gamein.2.xml");
		}

        private void copyFile(string source, string destination)
        {
            string sourceFile = Path.Combine(this.testDir, source);
            string destinationFile = Path.Combine(this.testDir, destination);

            // overwrite the destination file if it already exists.
            File.Copy(sourceFile, destinationFile, true);
        }

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

        private void consoleOutFile(string generated)
        {
            List<string> reportLines = this.loadTextFile(generated);

            Console.WriteLine("Generated file " + generated);
            for (int i = 0; i < reportLines.Count; i++)
            {
                Console.WriteLine(reportLines[i]);
            }
        }

		private void compareFiles(string expected, string generated, bool allToConsole = true)
		{

			List<string> testLines = this.loadTextFile(expected);
			List<string> reportLines = this.loadTextFile(generated);

			Console.WriteLine("Generated file " + generated + " expected file " + expected);			
			for (int i = 0; i < reportLines.Count; i++)
			{
                if (allToConsole)
                {
                    Console.WriteLine(reportLines[i]);
                }
				Assert.That(reportLines[i], Is.EqualTo(testLines[i]), "error in file " + generated + " in line " + i + ": " + reportLines[i]);
			}
			Assert.That(reportLines.Count, Is.EqualTo(testLines.Count), "error in file " + generated + " files are differing in lenght");
		}

		private void parseOrders(string filename)
		{
		}

        [Test]
        public void _4_LoadGameIn2()
        {
            // assert gamein2 is the same as gameout1
            this.compareFiles("gamein.2.xml", "gameout.1_saved.xml");
            this.LoadGalaxy("gamein.2.xml");

            // sequence:
            // validate input faction reports
            ReportWriter reportsWriter = new ReportWriter(this.game, this.dataFile, this.testDir);
            reportsWriter.GenerateReports(this.testDir);

            // reports delviered from gamein2 should be the same as made during turn 1 execution

            this.consoleOutFile("report.2.1.txt");

            this.compareFiles("testreport.2.1.txt", "report.2.1.txt");
            this.compareFiles("testreport.2.2.txt", "report.2.2.txt");
            this.compareFiles("testreport.2.3.txt", "report.2.3.txt");
        }

        [Test]
        public void _5_ParseGeneratedOrders()
        {
            // generated orders should be parseable
            Assert.Fail();
        }

		[Test]
		public void _6_ExecuteTurn2()
		{
            Sequence.Ints.Push(120);
            Sequence.Ints.Push(119);
            Sequence.Ints.Push(118);
            Sequence.Ints.Push(117);
            Sequence.Ints.Push(116);
            Sequence.Ints.Push(115);
            Sequence.Ints.Push(114);
            Sequence.Ints.Push(113);
            Sequence.Ints.Push(112);
            Sequence.Ints.Push(111);

            // assert gamein2 is the same as gameout1
            this.compareFiles("gamein.2.xml", "gameout.1_saved.xml");
            this.LoadGalaxy("gamein.2.xml");

			// story:
            // research lab is built
			// a ship launches toward the moon
			// and army attacks the other region
			// a city quest is completed and reward collected

            // sequence:        
            OrdersReader ordersReader = new OrdersReader(this.game);
            ordersReader.LoadOrders(Path.Combine(this.testDir, "orders.2.2.txt"), false);
            ordersReader.LoadOrders(Path.Combine(this.testDir, "orders.2.3.txt"), false);

            this.game.Execute();

            // validate output reports
            ReportWriter reportsWriter = new ReportWriter(this.game, this.dataFile, this.testDir);
            reportsWriter.GenerateReports(this.testDir);            

            this.compareFiles("testreport.3.1.txt", "report.3.1.txt");
            this.compareFiles("testreport.3.2.txt", "report.3.2.txt");
            this.compareFiles("testreport.3.3.txt", "report.3.3.txt");

            //   validate output game file
            this.dataFile.SaveGame(this.testDir, "gameout.2_saved.xml");
            this.compareFiles("gameout.2.xml", "gameout.2_saved.xml");
            this.copyFile("gameout.2_saved.xml", "gamein.3.xml");
		}

		[Test, Ignore("not ready")]
		public void ExecuteTurn3()
		{
			this.dataFile.LoadGameDocument(Directory.GetCurrentDirectory(), "SampleGame/gamein.3.xml");
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();

			// story:
            // research is commenced in the lab built
			// a ship reaches moon and explore it, crashed alien ship is found
            // quest is created to take over the crashed ship with an ability to communicate and control alien units as a reward
			// city build a ship
		}

		[Test, Ignore("not ready")]
		public void ExecuteTurn4()
		{
			this.dataFile.LoadGameDocument(Directory.GetCurrentDirectory(), "SampleGame/gamein.4.xml");
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();

			// story:
			// a ship launches toward the moon
			// ground regions make cash on market
            // crashed ship is taken over
            // crashed ships contain working fighter drones (tech 2) a hibernated seed of a colony, 
            //   technology to build domed city (tech 2) and resources to build it
            
		}

		[Test, Ignore("not ready")]
		public void ExecuteTurn5()
		{
			this.dataFile.LoadGameDocument(Directory.GetCurrentDirectory(), "SampleGame/gamein.5.xml");
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();

			// story:			
            // a moon colony is started
			// a space battle commence resulting in drones victory
            // endstate earth vs moon - human vs human officers and alien race
		}



	}
}
