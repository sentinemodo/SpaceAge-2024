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
	public class TReport : TTest
	{
		public TReport()
		{			
		}
			
		[SetUp]
		public void setupReport()
		{
			this.datafile = new DataFile(Directory.GetCurrentDirectory());
			this.datafile.LoadConfiguration();
			this.datafile.LoadGame();
			this.game = this.datafile.Game;
		}

		[TearDown]
		public void teardownReport()
		{
			this.game.Week = 1;
			this.game.ClearDictionaries();
			this.game = null;
			this.datafile = null;			
		}

		[Test]
		public void SetupTeardown()
		{
			ClassicAssert.IsTrue(true);
		}

		[Test]
		public void RegionReport()
		{
			Faction faction = this.game.Factions["2"];
			Region region = this.game.Regions["R00001"];

			List<string> testlines = new List<string>();
			testlines.Add("Western Europe [R00001] (0,4), grassland region, settlement capacity 8/2.");
			testlines.Add("Exits:");
			testlines.Add("  Eastern Europe [R00002] (1,4), grassland region, ground travel duration 3 weeks.");
			testlines.Add("Resources: 20 units of iron [iron], 500 units of food [food].");
			testlines.Add("Market report:");
			testlines.Add("  Offers of selling items:");
			testlines.Add("    sell 20 terrans [terran] at 50 each from Berlin [000005].");
            testlines.Add("    sell 40 units of food [food] at 2 each from Berlin farms [000008].");
            testlines.Add("  Offers of buying items:");
			testlines.Add("    buy 200 units of food [food] at any price by Berlin [000005].");
			testlines.Add("  Offers of selling modules:");
			testlines.Add("    none.");
			testlines.Add("  Offers of buying modules:");
			testlines.Add("    buy core drill [cdrill] at 100 by Berlin [000005].");
			testlines.Add("  Offers of selling technologies:");
			testlines.Add("    none.");
			testlines.Add("  Offers of buying technologies:");
			testlines.Add("    buy city planning [ctypln] at 100 by Berlin [000005].");
			testlines.Add("");
			testlines.Add("- Berlin [000005], 2 cities [city], immobile, owned by NPC [1].");
			testlines.Add("  size: 50000, capacity: 30000/7230, energy: 140/80 (20).");
            testlines.Add("  - Berlin farms [000008], 3 farming complexes [farms], owned by NPC [1].");
			testlines.Add("    size: 3000.");
            testlines.Add("  - Berlin wind powerplants [000011], 15 wind powerplants [wnplnt], owned by NPC [1].");
			testlines.Add("    size: 150.");
			testlines.Add("  + coal-burning plant [000007], 2 coal-burning plants [cplant].");
            testlines.Add("    size: 2000, mass: 2116 (2000), capacity: 1000/91, energy: 80/20 (consume: 10 units of carbon [carbon] for 13 weeks), crew: 4/4, upkeep: 104 (100) cash [cash], consume: 4 (0) units of food [food], 4 (0) units of terran breathing gas mixture [terair].");
            testlines.Add("    fuel requirements: 10 units of carbon [carbon] per 13 weeks.");
            testlines.Add("    items: 10 units of carbon [carbon] (size: 50, mass: 50), 5 units of iron [iron] (size: 25, mass: 50), 4 terrans [terran] (size: 16, mass: 16, upkeep: 4 cash [cash], consume: 4 units of food [food], 4 units of terran breathing gas mixture [terair]).");
			testlines.Add("  + core drill [000006], 2 core drills [cdrill].");
            testlines.Add("    size: 2000, mass: 2148 (2000), capacity: 1500/98, energy: 10, crew: 12/12, upkeep: 92 (80) cash [cash], consume: 12 (0) units of food [food], 12 (0) units of terran breathing gas mixture [terair].");
			testlines.Add("    technologies: hydrocarbons drilling [hcdril].");
			testlines.Add("    items: 10 units of iron [iron] (size: 50, mass: 100), 12 terrans [terran] (size: 48, mass: 48, upkeep: 12 cash [cash], consume: 12 units of food [food], 12 units of terran breathing gas mixture [terair]).");
			testlines.Add("+ trucks [100001], 2 trucks [trucks].");
			testlines.Add("  size: 500, mass: 220 (200), capacity: 300/16, crew: 2/2, upkeep: 31 (20) cash [cash], consume: 10 (8) units of food [food], 10 (8) units of terran breathing gas mixture [terair].");
			testlines.Add("  movement speed: 0.5 on ground.");
            testlines.Add("  fuel requirements: 2 units of oil [oil] per 13 weeks.");
            testlines.Add("  items: 2 units of oil [oil] (size: 8, mass: 10), terran [terran] (size: 4, mass: 4, upkeep: cash [cash], consume: unit of food [food], unit of terran breathing gas mixture [terair]).");
			testlines.Add("  + terran officer [200001], terran [terran].");
			testlines.Add("    size: 4, mass: 6 (4), capacity: 3/2, upkeep: 10 cash [cash], consume: unit of food [food], unit of terran breathing gas mixture [terair].");
			testlines.Add("    items: 2 units of food [food] (size: 2, mass: 2), 100 cash [cash].");
			List<string> lines = region.Report(faction);
			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

			for (int i = 0; i < lines.Count; i++)
			{
				ClassicAssert.AreEqual(testlines[i], lines[i], "line number " + i);
			}
			ClassicAssert.AreEqual(testlines.Count, lines.Count);			
		}

		[Test]
		public void MarketReport()
		{
			Faction faction = this.game.Factions["2"];
			Region region = this.game.Regions["R00001"];
			Market market = region.Market;
			List<string> testlines = new List<string>();
			testlines.Add("Market report:");
			testlines.Add("  Offers of selling items:");
            testlines.Add("    sell 20 terrans [terran] at 50 each from Berlin [000005] in Western Europe [R00001] (0,4).");
            testlines.Add("    sell 40 units of food [food] at 2 each from Berlin farms [000008] in Western Europe [R00001] (0,4).");
			testlines.Add("  Offers of buying items:");
			testlines.Add("    buy 200 units of food [food] at any price by Berlin [000005] in Western Europe [R00001] (0,4).");
			testlines.Add("  Offers of selling modules:");
			testlines.Add("    none.");
			testlines.Add("  Offers of buying modules:");
			testlines.Add("    buy core drill [cdrill] at 100 by Berlin [000005] in Western Europe [R00001] (0,4).");
			testlines.Add("  Offers of selling technologies:");
			testlines.Add("    none.");
			testlines.Add("  Offers of buying technologies:");
			testlines.Add("    buy city planning [ctypln] at 100 by Berlin [000005] in Western Europe [R00001] (0,4).");
			testlines.Add("");
			List<string> lines = market.Report(faction, true);

			Console.WriteLine("generated lines");
			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

			Console.WriteLine("test lines");
			for (int i = 0; i < testlines.Count; i++)
			{
				Console.WriteLine(testlines[i]);
			}

			for (int i = 0; i < lines.Count; i++)
			{
				ClassicAssert.AreEqual(testlines[i], lines[i], "error in line " + i);
			}
			ClassicAssert.AreEqual(testlines.Count, lines.Count);
		}

		[Test]
		public void UnitReport_Movement()
		{
			Faction faction = this.game.Factions["2"];
			ModuleStack moduleStack = ModuleStack.All["100001"];
			
			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 100001");
			testcommands.Add("move R00002");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);

			MoveOrder order = (MoveOrder)moduleStack.Orders[0];
			Region region1 = Region.All["R00001"];
			Region region2 = Region.All["R00002"];

			for (int i = 0; i < 6; i++)
			{
				moduleStack.ExecutedLongOrder = false;
				moduleStack.Orders.Execute(this.game.Week);
				moduleStack.Effects.Execute(this.game.Week);

				moduleStack.Orders.RemoveExecuted();
				moduleStack.Effects.RemoveExecuted();
				this.game.Week++;
			}	

			List<string> testlines = new List<string>();
			testlines.Add("+ trucks [100001], 2 trucks [trucks].");
			testlines.Add("  size: 500, mass: 210 (200), capacity: 300/8, crew: 2/2, upkeep: 31 (20) cash [cash], consume: 10 (8) units of food [food], 10 (8) units of terran breathing gas mixture [terair].");
			testlines.Add("  movement speed: 0.5 on ground.");
            testlines.Add("  fuel requirements: 2 units of oil [oil] per 13 weeks.");
            testlines.Add("  items: terran [terran] (size: 4, mass: 4, upkeep: cash [cash], consume: unit of food [food], unit of terran breathing gas mixture [terair]).");
			testlines.Add("  effects:");
			testlines.Add("    can operate for another 7 weeks without refueling.");
			testlines.Add("  events:");
			testlines.Add("    week 1: departed from Western Europe [R00001] (0,4) to Eastern Europe [R00002] (1,4), ETA 5.");
			testlines.Add("    week 1: consumed 2 units of oil [oil] as fuel.");
            testlines.Add("    week 2: moving from Western Europe [R00001] (0,4) to Eastern Europe [R00002] (1,4), ETA 4.");
            testlines.Add("    week 3: moving from Western Europe [R00001] (0,4) to Eastern Europe [R00002] (1,4), ETA 3.");
            testlines.Add("    week 4: moving from Western Europe [R00001] (0,4) to Eastern Europe [R00002] (1,4), ETA 2.");
            testlines.Add("    week 5: moving from Western Europe [R00001] (0,4) to Eastern Europe [R00002] (1,4), ETA 1.");
			//TODO: testlines.Add("    week 3: arrived at border of Western Europe [R00001] (0,4) and Eastern Europe [R00002] (1,4).");

			testlines.Add("    week 6: arrived at Eastern Europe [R00002] (1,4) from Western Europe [R00001] (0,4).");
			testlines.Add("  + terran officer [200001], terran [terran].");
			testlines.Add("    size: 4, mass: 6 (4), capacity: 3/2, upkeep: 10 cash [cash], consume: unit of food [food], unit of terran breathing gas mixture [terair].");
			testlines.Add("    items: 2 units of food [food] (size: 2, mass: 2), 100 cash [cash].");

			List<string> lines = moduleStack.Report(faction);
			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

			for (int i = 0; i < lines.Count; i++)
			{
				ClassicAssert.AreEqual(testlines[i], lines[i]);
			}
			ClassicAssert.AreEqual(testlines.Count, lines.Count);
		}

		[Test]
		public void UnitReport_Produce()
		{
			Faction faction = this.game.Factions["2"];
			ModuleStack moduleStack = ModuleStack.All["000004"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000004");
			testcommands.Add("use agrplx");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);

			UseOrder useOrder = (UseOrder)moduleStack.Orders[0];
			ItemType iron = ItemType.All["iron"];
			ModuleType farms = ModuleType.All["farms"];


			moduleStack.ExecutedLongOrder = false;
			moduleStack.Execute(this.game.Week);
			this.game.Week++;	

			ProducingModule producing = (ProducingModule)useOrder.Producing;

			for (int i = 0; i < 3; i++)
			{
				moduleStack.ExecutedLongOrder = false; 
				moduleStack.Execute(this.game.Week);
				this.game.Week++;	
			}
			ModuleStack farmsStack = producing.Produced;

			List<string> testlines1 = new List<string>();
			testlines1.Add("+ factory [000004], factory [factry].");
			testlines1.Add("  size: 1000, mass: 890 (750), capacity: 500/90, energy: 15, crew: 10/10, upkeep: 70 (60) cash [cash], consume: 10 (0) units of food [food], 10 (0) units of terran breathing gas mixture [terair].");
			testlines1.Add("  technologies: agricultural complex [agrplx], armored combat [armcbt].");
			testlines1.Add("  items: 10 terrans [terran] (size: 40, mass: 40, upkeep: 10 cash [cash], consume: 10 units of food [food], 10 units of terran breathing gas mixture [terair]), 10 units of iron [iron] (size: 50, mass: 100), 300 cash [cash].");
			testlines1.Add("  events:");
			testlines1.Add("    week 1: consumed 10 units of iron [iron] to produce farming complex [farms] module.");
			testlines1.Add(string.Format("    week 4: produced farming complex [farms] into {0}.", farmsStack.ReportName));

			List<string> lines;
			lines = moduleStack.Report(faction);
			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

			for (int i = 0; i < lines.Count; i++)
			{
				ClassicAssert.AreEqual(testlines1[i], lines[i]);
			}
			ClassicAssert.AreEqual(testlines1.Count, lines.Count);

			List<string> testlines2 = new List<string>();			
			testlines2.Add(string.Format("+ farming complex [{0}], farming complex [farms], disabled.", farmsStack.Name));
			testlines2.Add("  size: 1000, mass: 100 (100), capacity: 500/0, energy: 5, crew: 5/0, upkeep: 50 cash [cash].");
			testlines2.Add("  events:");
			testlines2.Add("    week 4: formed by factory [000004] with farming complex [farms].");
            testlines2.Add("    week 4: received farming complex [farms] produced by factory [000004].");

           lines = farmsStack.Report(faction);
			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

			for (int i = 0; i < lines.Count; i++)
			{
				ClassicAssert.AreEqual(testlines2[i], lines[i]);
			}
			ClassicAssert.AreEqual(testlines2.Count, lines.Count);

		}

		[Test]
		public void BattleReport_ShipVsStation_ShipPerspective()
		{
			Faction faction = this.game.Factions["2"];
			Battles battles = this.game.Battles;
			ModuleStack frigate = ModuleStack.All["100011"];
			ModuleStack station = ModuleStack.All["100021"];

			Sequence.Ints.Push(1);
			Sequence.Ints.Push(1);
			Sequence.Ints.Push(1);
			Sequence.Ints.Push(13);

			Battle battle = new Battle(frigate, station);
			battle.Execute(this.game.Week);

			List<string> testlines = new List<string>();
			testlines.Add("Battles report:");
			testlines.Add("");
			testlines.Add("  Week 1.");
			testlines.Add("  Battle has commenced at orbit [O00003] of Earth [P00002] at AU 1, ocean planet [ocean] in system Sol [SS0001] (0, 0, 0).");
			testlines.Add("  ------------------------------------------------------------");
			testlines.Add("  Round 1:");
			testlines.Add("  ------------------------------------------------------------");
			testlines.Add("  Attackers:");
			testlines.Add("  + Frigate [100011], spaceship hull [sshull], military.");
			testlines.Add("    size: 5000, mass: 10000/4680 (100), energy: 120/78 (1), crew: 6/30.");
			testlines.Add("    hit points: 370/370 (50/50), attack: 24 (0), defense: 17 (10), initiative: 50 (40).");
			testlines.Add("    tactics: disable.");
			testlines.Add("      #1 hit points: 50/50.");
			// attack is a sum of module values and technology and skills
			// base value 24
			// technology 24*0,1 = 2,4 => 3
			// inititive was calculated by the following formula ((energy supply / consumption) + (drive capacity / mass)) *10 + bonuses from technologies and skills
			// power 120/78 = 1,53 * 10
			// thrust 10000/5400 = 1,85 * 10  
			testlines.Add("    + command bridge [100012], command bridge [cbridg].");
			testlines.Add("      size: 800, mass: 340 (300), energy: 5, crew: 1/10.");
			testlines.Add("      hit points: 55/55, defense: 5, initiative: 10 (5).");
			testlines.Add("      technologies: military tactics [miltac] (initiative: 5).");
			testlines.Add("        #1 hit points: 55/55.");
			testlines.Add("      + terran officer [200002], terran [terran].");
			testlines.Add("        mass: 4, defense: 5, initiative: 5.");
			testlines.Add("        skills: frigate pilot [frgplt] (defense: 5, initiative: 5).");
			testlines.Add("    + fission reactor [100013], 2 fission reactors [fisrec].");
			testlines.Add("      size: 800, mass: 384 (280), energy: 120/20, crew: 2/6.");
			testlines.Add("      hit points: 90/90, attack: 4.");
			testlines.Add("        #1 hit points: 45/45.");
			testlines.Add("        #2 hit points: 45/45.");
			testlines.Add("    + reaction drive [100014], reaction drive [rctdrv].");
			testlines.Add("      size: 600, mass: 10000/804 (700), energy: 30, crew: 1/1.");
			testlines.Add("      hit points: 65/65.");
			testlines.Add("        #1 hit points: 65/65.");
			testlines.Add("    + x-ray laser [100015], 2 x-ray lasers [xraylz].");
			testlines.Add("      size: 200, mass: 208 (200), energy: 20, crew: 2/2.");
			testlines.Add("      hit points: 20/20, attack: 20, defense: 2.");
			testlines.Add("        #1 hit points: 10/10.");
			testlines.Add("        #2 hit points: 10/10.");
			testlines.Add("    + crew quarters [100016], 2 crew quarters [crwqrt].");
			testlines.Add("      size: 1000, mass: 2844 (800), energy: 2.");
			testlines.Add("      hit points: 90/90.");
			testlines.Add("        #1 hit points: 45/45.");
			testlines.Add("        #2 hit points: 45/45.");
			testlines.Add("  Defenders:");
            testlines.Add("  - Station [100021], orbital complex [orcmpx], immobile, owned by NPC [1].");
			testlines.Add("    size: 5000.");
			testlines.Add("    hit points: 305/305 (50/50), attack: 2 (0), defense: 5 (0), initiative: 40.");
			// initiative 
			// power => 40/11 = 3 * 10 => 30 
			// thrust => immobile => 0
			testlines.Add("      #1 hit points: 50/50.");
            testlines.Add("    - command bridge [100022], command bridge [cbridg], owned by NPC [1].");
			testlines.Add("      size: 800.");
			testlines.Add("      hit points: 55/55, defense: 5.");
			testlines.Add("        #1 hit points: 55/55.");
            testlines.Add("      - terran officer [200003], terran [terran], working for NPC [1].");
			testlines.Add("        defense: 5.");
            testlines.Add("    - fission reactor [100023], fission reactor [fisrec], owned by NPC [1].");
			testlines.Add("      size: 400.");
			testlines.Add("      hit points: 45/45, attack: 2.");
			testlines.Add("        #1 hit points: 45/45.");
            testlines.Add("    - small cargo bay [100024], 2 small cargo bays [cargob], owned by NPC [1].");
			testlines.Add("      size: 4000.");
			testlines.Add("      hit points: 110/110.");
			testlines.Add("        #1 hit points: 55/55.");
			testlines.Add("        #2 hit points: 55/55.");
            testlines.Add("    - crew quarters [100025], crew quarters [crwqrt], owned by NPC [1].");
			testlines.Add("      size: 500.");
			testlines.Add("      hit points: 45/45.");
			testlines.Add("        #1 hit points: 45/45.");
			testlines.Add("  ------------------------------------------------------------");
            // Station won initiative but cannot attack
            testlines.Add("  Station [100021] is unarmed and cannot attack.");
            testlines.Add("  Station [100021] is immobile and cannot escape.");
			// targets Station as it's the only target
			// disable tactics reduces hit chances by half but treat command, energy and drive modulestacks as double size for hit resolution
			// calculated with weapon attack + bonuses (technologies, skills, fleets): (24)/2 = 12, and attacker attak + defender defence: 11 + 5
			testlines.Add("  Frigate [100011] fires x-ray laser [100015] on Station [100021] (chance: 12/29) and misses.");
			// 1-50 complex, 51-66 bridge, 67-74 reactor, 75-95 cargo #1, 96-116 cargo #2
			// damage is calculated with attack / 10;
			testlines.Add("  Frigate [100011] fires x-ray laser [100015] on Station [100021] (chance: 12/29) and hits #1 fission reactor [100023] doing 10 damage.");
			testlines.Add("  ------------------------------------------------------------");
			testlines.Add("  Round 2:");
			testlines.Add("  ------------------------------------------------------------");
			testlines.Add("  Attackers:");
			testlines.Add("  + Frigate [100011], spaceship hull [sshull], military.");
			testlines.Add("    size: 5000, mass: 10000/4680 (100), energy: 120/78 (1), crew: 6/30.");
			testlines.Add("    hit points: 370/370 (50/50), attack: 24 (0), defense: 17 (10), initiative: 50 (40).");
			testlines.Add("    tactics: disable.");
			testlines.Add("      #1 hit points: 50/50.");
			testlines.Add("    + command bridge [100012], command bridge [cbridg].");
			testlines.Add("      size: 800, mass: 340 (300), energy: 5, crew: 1/10.");
			testlines.Add("      hit points: 55/55, defense: 5, initiative: 10 (5).");
			testlines.Add("      technologies: military tactics [miltac] (initiative: 5).");
			testlines.Add("        #1 hit points: 55/55.");
			testlines.Add("      + terran officer [200002], terran [terran].");
			testlines.Add("        mass: 4, defense: 5, initiative: 5.");
			testlines.Add("        skills: frigate pilot [frgplt] (defense: 5, initiative: 5).");
			testlines.Add("    + fission reactor [100013], 2 fission reactors [fisrec].");
			testlines.Add("      size: 800, mass: 384 (280), energy: 120/20, crew: 2/6.");
			testlines.Add("      hit points: 90/90, attack: 4.");
			testlines.Add("        #1 hit points: 45/45.");
			testlines.Add("        #2 hit points: 45/45.");
			testlines.Add("    + reaction drive [100014], reaction drive [rctdrv].");
			testlines.Add("      size: 600, mass: 10000/804 (700), energy: 30, crew: 1/1.");
			testlines.Add("      hit points: 65/65.");
			testlines.Add("        #1 hit points: 65/65.");
			testlines.Add("    + x-ray laser [100015], 2 x-ray lasers [xraylz].");
			testlines.Add("      size: 200, mass: 208 (200), energy: 20, crew: 2/2.");
			testlines.Add("      hit points: 20/20, attack: 20, defense: 2.");
			testlines.Add("        #1 hit points: 10/10.");
			testlines.Add("        #2 hit points: 10/10.");
			testlines.Add("    + crew quarters [100016], 2 crew quarters [crwqrt].");
			testlines.Add("      size: 1000, mass: 2844 (800), energy: 2.");
			testlines.Add("      hit points: 90/90.");
			testlines.Add("        #1 hit points: 45/45.");
			testlines.Add("        #2 hit points: 45/45.");
			testlines.Add("  Defenders:");
            testlines.Add("  - Station [100021], orbital complex [orcmpx], immobile, owned by NPC [1].");
			testlines.Add("    size: 5000.");
			testlines.Add("    hit points: 305/295 (50/50), attack: 2 (0), defense: 5 (0), initiative: 40.");
			testlines.Add("      #1 hit points: 50/50.");
            testlines.Add("    - command bridge [100022], command bridge [cbridg], owned by NPC [1].");
			testlines.Add("      size: 800.");
			testlines.Add("      hit points: 55/55, defense: 5.");
			testlines.Add("        #1 hit points: 55/55.");
            testlines.Add("      - terran officer [200003], terran [terran], working for NPC [1].");
			testlines.Add("        defense: 5.");
            testlines.Add("    - fission reactor [100023], fission reactor [fisrec], owned by NPC [1].");
			testlines.Add("      size: 400.");
			testlines.Add("      hit points: 45/35, attack: 2.");
			testlines.Add("        #1 hit points: 45/35.");
            testlines.Add("    - small cargo bay [100024], 2 small cargo bays [cargob], owned by NPC [1].");
			testlines.Add("      size: 4000.");
			testlines.Add("      hit points: 110/110.");
			testlines.Add("        #1 hit points: 55/55.");
			testlines.Add("        #2 hit points: 55/55.");
            testlines.Add("    - crew quarters [100025], crew quarters [crwqrt], owned by NPC [1].");
			testlines.Add("      size: 500.");
			testlines.Add("      hit points: 45/45.");
			testlines.Add("        #1 hit points: 45/45.");
			testlines.Add("  ------------------------------------------------------------");
            // Station won initiative but cannot attack
            testlines.Add("  Station [100021] is unarmed and cannot attack.");
            testlines.Add("  Station [100021] is immobile and cannot escape.");
            testlines.Add("  Frigate [100011] fires x-ray laser [100015] on Station [100021] (chance: 12/29) and hits #1 fission reactor [100023] doing 10 damage.");
			testlines.Add("    #1 fission reactor [fisrec] is lightly damaged.");			
			testlines.Add("  Frigate [100011] fires x-ray laser [100015] on Station [100021] (chance: 12/29) and hits #1 fission reactor [100023] doing 10 damage.");
			testlines.Add("    #1 fission reactor [fisrec] is heavily damaged.");
			testlines.Add("    #1 fission reactor [fisrec] is disabled.");
			testlines.Add("    Station [100021] lost it's power supply and disables.");
			testlines.Add("  ------------------------------------------------------------");
			testlines.Add("  Battle won by attackers.");
			testlines.Add("");
			List<string> lines = battles.Report(faction);
			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

			for (int i = 0; i < lines.Count; i++)
			{
				ClassicAssert.AreEqual(testlines[i], lines[i]);
			}
			ClassicAssert.AreEqual(testlines.Count, lines.Count);
		}

		[Test]
		public void BattleReport_ShipVsStation_StationPerspective()
		{
			Faction faction = this.game.Factions["1"];
			Battles battles = this.game.Battles;
			ModuleStack frigate = ModuleStack.All["100011"];
			ModuleStack station = ModuleStack.All["100021"];

			Sequence.Ints.Push(1);
			Sequence.Ints.Push(1);
			Sequence.Ints.Push(1);
			Sequence.Ints.Push(13);

			Battle battle = new Battle(frigate, station);
			battle.Execute(this.game.Week);

			List<string> testlines = new List<string>();
			testlines.Add("Battles report:");
			testlines.Add("");
			testlines.Add("  Week 1.");
			testlines.Add("  Battle has commenced at orbit [O00003] of Earth [P00002] at AU 1, ocean planet [ocean] in system Sol [SS0001] (0, 0, 0).");
			testlines.Add("  ------------------------------------------------------------");
			testlines.Add("  Round 1:");
			testlines.Add("  ------------------------------------------------------------");
			testlines.Add("  Attackers:");
            testlines.Add("  - Frigate [100011], spaceship hull [sshull], military, owned by Caste Prime [2].");
			testlines.Add("    size: 5000.");
			testlines.Add("    hit points: 370/370 (50/50), attack: 24 (0), defense: 17 (10), initiative: 50 (40).");
			testlines.Add("      #1 hit points: 50/50.");
            testlines.Add("    - command bridge [100012], command bridge [cbridg], owned by Caste Prime [2].");
			testlines.Add("      size: 800.");
			testlines.Add("      hit points: 55/55, defense: 5, initiative: 10 (5).");
			testlines.Add("        #1 hit points: 55/55.");
            testlines.Add("      - terran officer [200002], terran [terran], working for Caste Prime [2].");
			testlines.Add("        defense: 5, initiative: 5.");
            testlines.Add("    - fission reactor [100013], 2 fission reactors [fisrec], owned by Caste Prime [2].");
			testlines.Add("      size: 800.");
			testlines.Add("      hit points: 90/90, attack: 4.");
			testlines.Add("        #1 hit points: 45/45.");
			testlines.Add("        #2 hit points: 45/45.");
            testlines.Add("    - reaction drive [100014], reaction drive [rctdrv], owned by Caste Prime [2].");
			testlines.Add("      size: 600.");
			testlines.Add("      hit points: 65/65.");
			testlines.Add("        #1 hit points: 65/65.");
            testlines.Add("    - x-ray laser [100015], 2 x-ray lasers [xraylz], owned by Caste Prime [2].");
			testlines.Add("      size: 200.");
			testlines.Add("      hit points: 20/20, attack: 20, defense: 2.");
			testlines.Add("        #1 hit points: 10/10.");
			testlines.Add("        #2 hit points: 10/10.");
            testlines.Add("    - crew quarters [100016], 2 crew quarters [crwqrt], owned by Caste Prime [2].");
			testlines.Add("      size: 1000.");
			testlines.Add("      hit points: 90/90.");
			testlines.Add("        #1 hit points: 45/45.");
			testlines.Add("        #2 hit points: 45/45.");
			testlines.Add("  Defenders:");
			testlines.Add("  + Station [100021], orbital complex [orcmpx], immobile.");
			testlines.Add("    size: 5000, mass: 6564 (100), energy: 60/17 (1), crew: 2/21.");
			testlines.Add("    hit points: 305/305 (50/50), attack: 2 (0), defense: 5 (0), initiative: 40.");
			testlines.Add("      #1 hit points: 50/50.");
			testlines.Add("    + command bridge [100022], command bridge [cbridg].");
			testlines.Add("      size: 800, mass: 340 (300), energy: 5, crew: 1/10.");
			testlines.Add("      hit points: 55/55, defense: 5.");
			testlines.Add("        #1 hit points: 55/55.");
			testlines.Add("      + terran officer [200003], terran [terran].");
			testlines.Add("        mass: 4, defense: 5.");
			testlines.Add("        skills: space station command [sscmnd] (defense: 5).");
			testlines.Add("    + fission reactor [100023], fission reactor [fisrec].");
			testlines.Add("      size: 400, mass: 192 (140), energy: 60/10, crew: 1/3.");
			testlines.Add("      hit points: 45/45, attack: 2.");
			testlines.Add("        #1 hit points: 45/45.");
			testlines.Add("    + small cargo bay [100024], 2 small cargo bays [cargob].");
			testlines.Add("      size: 4000, mass: 4516 (400).");
			testlines.Add("      hit points: 110/110.");
			testlines.Add("        #1 hit points: 55/55.");
			testlines.Add("        #2 hit points: 55/55.");
			testlines.Add("    + crew quarters [100025], crew quarters [crwqrt].");
			testlines.Add("      size: 500, mass: 1416 (400), energy: 1.");
			testlines.Add("      hit points: 45/45.");
			testlines.Add("        #1 hit points: 45/45.");
			testlines.Add("  ------------------------------------------------------------");
            testlines.Add("  Station [100021] is unarmed and cannot attack.");
            testlines.Add("  Station [100021] is immobile and cannot escape."); 
            testlines.Add("  Frigate [100011] fires x-ray laser [100015] on Station [100021] (chance: 12/29) and misses.");
			testlines.Add("  Frigate [100011] fires x-ray laser [100015] on Station [100021] (chance: 12/29) and hits #1 fission reactor [100023] doing 10 damage.");
			testlines.Add("  ------------------------------------------------------------");
			testlines.Add("  Round 2:");
			testlines.Add("  ------------------------------------------------------------");
			testlines.Add("  Attackers:");
            testlines.Add("  - Frigate [100011], spaceship hull [sshull], military, owned by Caste Prime [2].");
			testlines.Add("    size: 5000.");
			testlines.Add("    hit points: 370/370 (50/50), attack: 24 (0), defense: 17 (10), initiative: 50 (40).");
			testlines.Add("      #1 hit points: 50/50.");
            testlines.Add("    - command bridge [100012], command bridge [cbridg], owned by Caste Prime [2].");
			testlines.Add("      size: 800.");
			testlines.Add("      hit points: 55/55, defense: 5, initiative: 10 (5).");
			testlines.Add("        #1 hit points: 55/55.");
            testlines.Add("      - terran officer [200002], terran [terran], working for Caste Prime [2].");
			testlines.Add("        defense: 5, initiative: 5.");
            testlines.Add("    - fission reactor [100013], 2 fission reactors [fisrec], owned by Caste Prime [2].");
			testlines.Add("      size: 800.");
			testlines.Add("      hit points: 90/90, attack: 4.");
			testlines.Add("        #1 hit points: 45/45.");
			testlines.Add("        #2 hit points: 45/45.");
            testlines.Add("    - reaction drive [100014], reaction drive [rctdrv], owned by Caste Prime [2].");
			testlines.Add("      size: 600.");
			testlines.Add("      hit points: 65/65.");
			testlines.Add("        #1 hit points: 65/65.");
            testlines.Add("    - x-ray laser [100015], 2 x-ray lasers [xraylz], owned by Caste Prime [2].");
			testlines.Add("      size: 200.");
			testlines.Add("      hit points: 20/20, attack: 20, defense: 2.");
			testlines.Add("        #1 hit points: 10/10.");
			testlines.Add("        #2 hit points: 10/10.");
            testlines.Add("    - crew quarters [100016], 2 crew quarters [crwqrt], owned by Caste Prime [2].");
			testlines.Add("      size: 1000.");
			testlines.Add("      hit points: 90/90.");
			testlines.Add("        #1 hit points: 45/45.");
			testlines.Add("        #2 hit points: 45/45.");
			testlines.Add("  Defenders:");
			testlines.Add("  + Station [100021], orbital complex [orcmpx], immobile.");
			testlines.Add("    size: 5000, mass: 6564 (100), energy: 60/17 (1), crew: 2/21.");
			testlines.Add("    hit points: 305/295 (50/50), attack: 2 (0), defense: 5 (0), initiative: 40.");
			testlines.Add("      #1 hit points: 50/50.");
			testlines.Add("    + command bridge [100022], command bridge [cbridg].");
			testlines.Add("      size: 800, mass: 340 (300), energy: 5, crew: 1/10.");
			testlines.Add("      hit points: 55/55, defense: 5.");
			testlines.Add("        #1 hit points: 55/55.");
			testlines.Add("      + terran officer [200003], terran [terran].");
			testlines.Add("        mass: 4, defense: 5.");
			testlines.Add("        skills: space station command [sscmnd] (defense: 5).");
			testlines.Add("    + fission reactor [100023], fission reactor [fisrec].");
            testlines.Add("      size: 400, mass: 192 (140), energy: 60/10, crew: 1/3.");
			testlines.Add("      hit points: 45/35, attack: 2.");
			testlines.Add("        #1 hit points: 45/35.");
			testlines.Add("    + small cargo bay [100024], 2 small cargo bays [cargob].");
			testlines.Add("      size: 4000, mass: 4516 (400).");
			testlines.Add("      hit points: 110/110.");
			testlines.Add("        #1 hit points: 55/55.");
			testlines.Add("        #2 hit points: 55/55.");
			testlines.Add("    + crew quarters [100025], crew quarters [crwqrt].");
			testlines.Add("      size: 500, mass: 1416 (400), energy: 1.");
			testlines.Add("      hit points: 45/45.");
			testlines.Add("        #1 hit points: 45/45.");
			testlines.Add("  ------------------------------------------------------------");
            testlines.Add("  Station [100021] is unarmed and cannot attack.");
            testlines.Add("  Station [100021] is immobile and cannot escape."); 
            testlines.Add("  Frigate [100011] fires x-ray laser [100015] on Station [100021] (chance: 12/29) and hits #1 fission reactor [100023] doing 10 damage.");
			testlines.Add("    #1 fission reactor [fisrec] is lightly damaged.");
			testlines.Add("  Frigate [100011] fires x-ray laser [100015] on Station [100021] (chance: 12/29) and hits #1 fission reactor [100023] doing 10 damage.");
			testlines.Add("    #1 fission reactor [fisrec] is heavily damaged.");
			testlines.Add("    #1 fission reactor [fisrec] is disabled.");
			testlines.Add("    Station [100021] lost it's power supply and disables.");
			testlines.Add("  ------------------------------------------------------------");
			testlines.Add("  Battle won by attackers.");
			testlines.Add("");
			List<string> lines = battles.Report(faction);
			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

			for (int i = 0; i < lines.Count; i++)
			{
				ClassicAssert.AreEqual(testlines[i], lines[i]);
			}
			ClassicAssert.AreEqual(testlines.Count, lines.Count);
		}

		[Test]
		public void BankReport()
		{
			Faction faction = this.game.Factions["2"];
			Bank bank = faction.Bank;
			List<string> testlines = new List<string>();
			testlines.Add("Bank report:");
			testlines.Add("  Bank account balance: 10000.");
			testlines.Add("  Credit line maximum: 10000.");
			testlines.Add("  Credit rate: 20%, Deposit rate: 5%.");
			testlines.Add("");
			List<string> lines = bank.Report(faction);
			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

			for (int i = 0; i < lines.Count; i++)
			{
				ClassicAssert.AreEqual(testlines[i], lines[i]);
			}
			ClassicAssert.AreEqual(testlines.Count, lines.Count);
		}

		[Test]
		public void OrdersTemplate_producingEffect()
		{
            Sequence.Ints.Push(100);
		//orbit [O00002], has atmosphere suitable for terran [terran].
		// + space shuttle [100], 10 space shuttle [shuttl].
		//	 effects:
		//		 can operate for another 10 weeks without refueling.
		//		 producing spaceship hull [sshull] for space shuttle [100] into spaceship hull [101], 3 weeks to complete (10 efficiency multiplier).

		//Orders Template:
		//#faction 2 "xyzzy"
		//#modulestack 100
		//; + space shuttle [100], 10 space shuttle [shuttl].
		//; items: 40 terrans [terran], 24 units of iron [iron], 26 units of titanium [titani], 20 units of
		//; silicium [silici], 20 units of copper [copper], 10 units of uranium [uraniu], 150 units of food
		//; [food], 90 units of oxyhydro [h2o2], 150 units of terran breathing gas mixture [terair].

			//Assert.Fail("producing modules that carryover should have producing effect in the template");

			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>();
			testcommands.Add("#faction 2");
			testcommands.Add("#modulestack 000004");
			testcommands.Add("use agrplx");
			testcommands.Add("#end");

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
			ClassicAssert.AreEqual(1, testModuleStack.Orders.Count);

			ClassicAssert.IsTrue(testModuleStack.Orders[0] is UseOrder);
			UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];
			ClassicAssert.AreEqual(Technology.All["agrplx"], useOrder.Technology);
			ClassicAssert.AreEqual(1, useOrder.Repeat);

			ClassicAssert.AreEqual(4, Technology.All["agrplx"].UseTime);

			testModuleStack.Execute(this.game.Week);
			this.consoleOutReport("factory:", testModuleStack, testFaction);

			List<string> testlines = new List<string>();
			//testlines.Add("#modulestack 000004");
			testlines.Add("+ factory [000004], factory [factry].");
			testlines.Add("technologies: agricultural complex [agrplx], armored combat [armcbt].");
			testlines.Add("items: 10 terrans [terran], 10 units of iron [iron], 300 cash [cash].");
			testlines.Add("effects:");
			testlines.Add("  producing farming complex [farms] into empty stack [100], 3 weeks to complete.");
			testlines.Add("");

			List<string> lines = testModuleStack.ReportOrdersTemplateHeader(testFaction);

			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

			for (int i = 0; i < lines.Count; i++)
			{
				ClassicAssert.AreEqual(testlines[i], lines[i], "error in line " + i);
			}

		}

		[Test]
		public void MarketReport_unlimitedOffers()
		{
			//Market report:
			//Offers of buying items:
			//	terran [terran] for -1 by Gelvaren Headquarters [000018].

			//Assert.Fail("@buy all offers need to be properly described in report");

			Faction faction = this.game.Factions["2"];
			Region region = this.game.Regions["R00001"];
			ModuleStack offerent = ModuleStack.All["000005"];
			ItemType terran = ItemType.All["terran"];

			Market market = region.Market;
			Offer.All.Clear();
			Offer offer = new Offer(market, offerent, EOfferType.BuyItems);
			offer.AllQuantity = true;
			offer.Price = -1;
			offer.ItemType = terran;

			this.consoleOutReport("market report:", market, faction);

			List<string> testlines = new List<string>();
			testlines.Add("Market report:");
			testlines.Add("  Offers of selling items:");
			testlines.Add("    none.");
			testlines.Add("  Offers of buying items:");
			testlines.Add("    buy all terrans [terran] at any price by Berlin [000005].");
			testlines.Add("  Offers of selling modules:");
			testlines.Add("    none.");
			testlines.Add("  Offers of buying modules:");
			testlines.Add("    none.");
			testlines.Add("  Offers of selling technologies:");
			testlines.Add("    none.");
			testlines.Add("  Offers of buying technologies:");
			testlines.Add("    none.");
			testlines.Add("");

			List<string> lines = market.Report(faction);

			for (int i = 0; i < lines.Count; i++)
			{
				ClassicAssert.AreEqual(testlines[i], lines[i], "error in line " + i);
			}
		}

        [Test]
        public void ProduceOrderReport()
        {
            // unlimited orders should report in next turn templates with parameter
            // @produce energy or @produce cash instead of @produce
            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["000003"];

            List<string> testcommands = new List<string>();
            testcommands.Add("#faction 2");
            testcommands.Add("#modulestack 000003");
            testcommands.Add("@produce energy");
            testcommands.Add("#end");

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            ClassicAssert.AreEqual(1, testModuleStack.Orders.Count);

            this.consoleOutReport("orders:", testModuleStack.Orders, testFaction);

            List<string> lines = testModuleStack.Orders.Report(testFaction);

            ClassicAssert.AreEqual("@produce energy", lines[0]);
        }

        [Test]
        public void MarketReport_sequence()
        {
            // in report the sequence is:
                //Market report:
                //  Offers of selling items:
                //    none.
                //  Offers of buying items:
                //    buy all terrans [terran] at any price by Berlin [000005].
                //    buy 100 units of food [food] at 1 each by trucks [100001].
                //  Offers of selling modules:
                //    none.
                //  Offers of buying modules:
                //    none.
                //  Offers of selling technologies:
                //    none.
                //  Offers of buying technologies:
                //    none.
            // after reload, the sequence shouldn't be:
                //Market report:
                //  Offers of selling items:
                //    none.
                //  Offers of buying items:
                //    buy terran [terran] at any price by Berlin [000005].
                //    buy 100 units of food [food] at 1 each by trucks [100001].
                //  Offers of selling modules:
                //    none.
                //  Offers of buying modules:
                //    none.
                //  Offers of selling technologies:
                //    none.
                //  Offers of buying technologies:
                //    none.

            // I think there are two issues here
            // 1. xml load and save are in different order
            // 2. buy all terrans saves as buy terran

            Faction faction = this.game.Factions["2"];
            Region region = this.game.Regions["R00001"];
            ModuleStack offerent1 = ModuleStack.All["000005"];
            ModuleStack offerent2 = ModuleStack.All["100001"];

            ItemType terran = ItemType.All["terran"];
            ItemType food = ItemType.All["food"];

            Market market = region.Market;
            Offer.All.Clear();
            Offer offer2 = new Offer(market, offerent2, EOfferType.BuyItems);
            offer2.Quantity = 100;
            offer2.Price = 1;
            offer2.ItemType = food; 
            
            Offer offer1 = new Offer(market, offerent1, EOfferType.BuyItems);
            offer1.AllQuantity = true;
            offer1.Price = -1;
            offer1.ItemType = terran;

            this.consoleOutReport("market report:", market, faction);

            List<string> testlines = new List<string>();
            testlines.Add("Market report:");
            testlines.Add("  Offers of selling items:");
            testlines.Add("    none.");
            testlines.Add("  Offers of buying items:");
            testlines.Add("    buy all terrans [terran] at any price by Berlin [000005].");
            testlines.Add("    buy 100 units of food [food] at 1 each by trucks [100001].");
            testlines.Add("  Offers of selling modules:");
            testlines.Add("    none.");
            testlines.Add("  Offers of buying modules:");
            testlines.Add("    none.");
            testlines.Add("  Offers of selling technologies:");
            testlines.Add("    none.");
            testlines.Add("  Offers of buying technologies:");
            testlines.Add("    none.");
            testlines.Add("");

            List<string> lines = market.Report(faction);

            for (int i = 0; i < lines.Count; i++)
            {
                ClassicAssert.AreEqual(testlines[i], lines[i], "error in line " + i);
            }

            this.datafile.SaveGame(Directory.GetCurrentDirectory(), "gameout.marketsequence.xml");

            this.game.Week = 1;
            this.game.ClearDictionaries();
            this.game = null;
            this.datafile = null;			

			this.datafile = new DataFile(Directory.GetCurrentDirectory());
            this.datafile.LoadGameDocument(Directory.GetCurrentDirectory(), "gameout.marketsequence.xml");
            this.datafile.LoadConfiguration(Directory.GetCurrentDirectory());
            this.datafile.LoadFactions();
            this.datafile.LoadGalaxy();
            this.game = this.datafile.Game;

            // restart variables
            faction = this.game.Factions["2"];
            region = this.game.Regions["R00001"];

            market = region.Market;
            this.consoleOutReport("market report:", market, faction);
            
            // output should be the same
            lines = market.Report(faction);

            for (int i = 0; i < lines.Count; i++)
            {
                ClassicAssert.AreEqual(testlines[i], lines[i], "error in line " + i);
            }
        }

	}
}
