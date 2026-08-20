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
			Battle.All.Clear();
			this.dataFile = new DataFile(Directory.GetCurrentDirectory());
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadGame();
			this.game = this.dataFile.Game;
		}

		[TearDown]
		public void teardownReport()
		{
			this.game.Week = 1;
			this.game.ClearDictionaries();
			this.game = null;
			this.dataFile = null;			
		}

		[Test]
		public void SetupTeardown()
		{
            Assert.That(true);
		}

		[Test]
		public void RegionReport()
		{
			Faction faction = this.game.Factions["2"];
			Region region = this.game.Regions["R00001"];

			List<string> testlines = new List<string>
            {
                "Western Europe [R00001] (0,4), grassland region, settlement capacity 8/2.",
                "Exits:",
                "  Eastern Europe [R00002] (1,4), grassland region, ground travel duration 3 weeks.",
                "Resources: 20 units of iron [iron], 500 units of food [food].",
                "Market report:",
                "  Offers of selling items:",
                "    sell 20 terrans [terran] at 50 each from Berlin [000005].",
                "    sell 40 units of food [food] at 2 each from Berlin farms [000008].",
                "  Offers of buying items:",
                "    buy 200 units of food [food] at any price by Berlin [000005].",
                "  Offers of selling modules:",
                "    none.",
                "  Offers of buying modules:",
                "    buy core drill [cdrill] at 100 by Berlin [000005].",
                "  Offers of selling technologies:",
                "    none.",
                "  Offers of buying technologies:",
                "    buy city planning [ctypln] at 100 by Berlin [000005].",
                "",
                "- Berlin [000005], 2 cities [city], immobile, owned by NPC [1].",
                "  size: 50000, capacity: 30000/7230, energy: 140/80 (20).",
                "  - Berlin farms [000008], 3 farming complexes [farms], immobile, owned by NPC [1].",
                "    size: 3000.",
                "  - Berlin wind powerplants [000011], 15 wind powerplants [wnplnt], immobile, owned by NPC [1].",
                "    size: 150.",
                "  + coal-burning plant [000007], 2 coal-burning plants [cplant], immobile.",
                "    size: 2000, mass: 2116 (2000), capacity: 1000/91, energy: 80/20 (consume: 10 units of carbon [carbon] for 13 weeks), crew: 4/4, upkeep: 104 (100) cash [cash], consume: 4 (0) units of food [food], 4 (0) units of terran breathing gas mixture [terair].",
                "    fuel requirements: 10 units of carbon [carbon] per 13 weeks.",
                "    items: 10 units of carbon [carbon] (size: 50, mass: 50), 5 units of iron [iron] (size: 25, mass: 50), 4 terrans [terran] (size: 16, mass: 16, upkeep: 4 cash [cash], consume: 4 units of food [food], 4 units of terran breathing gas mixture [terair]).",
                "  + core drill [000006], 2 core drills [cdrill], immobile.",
                "    size: 2000, mass: 2148 (2000), capacity: 1500/98, energy: 10, crew: 12/12, upkeep: 92 (80) cash [cash], consume: 12 (0) units of food [food], 12 (0) units of terran breathing gas mixture [terair].",
                "    technologies: hydrocarbons drilling [hcdril].",
                "    items: 10 units of iron [iron] (size: 50, mass: 100), 12 terrans [terran] (size: 48, mass: 48, upkeep: 12 cash [cash], consume: 12 units of food [food], 12 units of terran breathing gas mixture [terair]).",
                "+ trucks [100001], 2 trucks [trucks].",
                "  size: 500, mass: 220 (200), capacity: 300/16, crew: 2/2, upkeep: 31 (20) cash [cash], consume: 10 (8) units of food [food], 10 (8) units of terran breathing gas mixture [terair].",
                "  movement speed: 0.5 on ground.",
                "  fuel requirements: 2 units of oil [oil] per 13 weeks.",
                "  items: 2 units of oil [oil] (size: 8, mass: 10), terran [terran] (size: 4, mass: 4, upkeep: cash [cash], consume: unit of food [food], unit of terran breathing gas mixture [terair]).",
                "  + terran officer [200001], terran [terran].",
                "    size: 4, mass: 6 (4), capacity: 3/2, upkeep: 10 cash [cash], consume: unit of food [food], unit of terran breathing gas mixture [terair].",
                "    items: 2 units of food [food] (size: 2, mass: 2), 100 cash [cash]."
            };
			List<string> lines = region.Report(faction);
			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

			for (int i = 0; i < lines.Count; i++)
			{
                Assert.That(lines[i], Is.EqualTo(testlines[i]), "line number " + i);
			}
            Assert.That(lines.Count, Is.EqualTo(testlines.Count));			
		}

		[Test]
		public void RegionReport_ShowsContractAboveMarket()
		{
			Faction faction = this.game.Factions["2"];
			Region region = this.game.Regions["R00001"];
			GiveModuleTrigger trigger = new GiveModuleTrigger(1, ModuleType.All["inftry"], ModuleStack.All["000005"]);
			new Contract("CT0001", region, this.game.Factions["1"], trigger, Technology.All["rckter"]);

			List<string> lines = region.Report(faction);
			int contractsIndex = lines.IndexOf("Contracts:");
			int marketIndex = lines.IndexOf("Market report:");
			Assert.That(contractsIndex, Is.GreaterThanOrEqualTo(0));
			Assert.That(marketIndex, Is.GreaterThan(contractsIndex));
			Assert.That(lines[contractsIndex + 1], Is.EqualTo("  CT0001: deliver 1 infantry battalion [inftry] to Berlin [000005]."));
			Assert.That(lines[contractsIndex + 2], Is.EqualTo("    Reward: rocket launcher production [rckter] technology."));
			Assert.That(lines[contractsIndex + 3], Is.EqualTo("Market report:"));
		}

		[Test]
		public void MarketReport()
		{
			Faction faction = this.game.Factions["2"];
			Region region = this.game.Regions["R00001"];
			Market market = region.Market;
			List<string> testlines = new List<string>
            {
                "Market report:",
                "  Offers of selling items:",
                "    sell 20 terrans [terran] at 50 each from Berlin [000005] in Western Europe [R00001] (0,4).",
                "    sell 40 units of food [food] at 2 each from Berlin farms [000008] in Western Europe [R00001] (0,4).",
                "  Offers of buying items:",
                "    buy 200 units of food [food] at any price by Berlin [000005] in Western Europe [R00001] (0,4).",
                "  Offers of selling modules:",
                "    none.",
                "  Offers of buying modules:",
                "    buy core drill [cdrill] at 100 by Berlin [000005] in Western Europe [R00001] (0,4).",
                "  Offers of selling technologies:",
                "    none.",
                "  Offers of buying technologies:",
                "    buy city planning [ctypln] at 100 by Berlin [000005] in Western Europe [R00001] (0,4).",
                ""
            };
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
                Assert.That(lines[i], Is.EqualTo(testlines[i]), "error in line " + i);
			}
            Assert.That(lines.Count, Is.EqualTo(testlines.Count));
		}

		[Test]
		public void UnitReport_Movement()
		{
			Faction faction = this.game.Factions["2"];
			ModuleStack moduleStack = ModuleStack.All["100001"];
			
			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 100001",
                "move R00002",
                "#end"
            };

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

			List<string> testlines = new List<string>
            {
                "+ trucks [100001], 2 trucks [trucks].",
                "  size: 500, mass: 210 (200), capacity: 300/8, crew: 2/2, upkeep: 31 (20) cash [cash], consume: 10 (8) units of food [food], 10 (8) units of terran breathing gas mixture [terair].",
                "  movement speed: 0.5 on ground.",
                "  fuel requirements: 2 units of oil [oil] per 13 weeks.",
                "  items: terran [terran] (size: 4, mass: 4, upkeep: cash [cash], consume: unit of food [food], unit of terran breathing gas mixture [terair]).",
                "  effects:",
                "    can operate for another 7 weeks without refueling.",
                "  events:",
                "    week 1: departed from Western Europe [R00001] (0,4) to Eastern Europe [R00002] (1,4), ETA 5.",
                "    week 1: consumed 2 units of oil [oil] as fuel.",
                "    week 2: moving from Western Europe [R00001] (0,4) to Eastern Europe [R00002] (1,4), ETA 4.",
                "    week 3: moving from Western Europe [R00001] (0,4) to Eastern Europe [R00002] (1,4), ETA 3.",
                "    week 4: moving from Western Europe [R00001] (0,4) to Eastern Europe [R00002] (1,4), ETA 2.",
                "    week 5: moving from Western Europe [R00001] (0,4) to Eastern Europe [R00002] (1,4), ETA 1.",
                //TODO: testlines.Add("    week 3: arrived at border of Western Europe [R00001] (0,4) and Eastern Europe [R00002] (1,4).");

                "    week 6: arrived at Eastern Europe [R00002] (1,4) from Western Europe [R00001] (0,4).",
                "  + terran officer [200001], terran [terran].",
                "    size: 4, mass: 6 (4), capacity: 3/2, upkeep: 10 cash [cash], consume: unit of food [food], unit of terran breathing gas mixture [terair].",
                "    items: 2 units of food [food] (size: 2, mass: 2), 100 cash [cash]."
            };

			List<string> lines = moduleStack.Report(faction);
			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

			for (int i = 0; i < lines.Count; i++)
			{
                Assert.That(lines[i], Is.EqualTo(testlines[i]));
			}
            Assert.That(lines.Count, Is.EqualTo(testlines.Count));
		}

		[Test]
		public void UnitReport_Produce()
		{
			Faction faction = this.game.Factions["2"];
			ModuleStack moduleStack = ModuleStack.All["000004"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "use agrplx",
                "#end"
            };

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

			List<string> testlines1 = new List<string>
            {
                "+ factory [000004], factory [factry], immobile.",
                "  size: 1000, mass: 890 (750), capacity: 500/90, energy: 15, crew: 10/10, upkeep: 70 (60) cash [cash], consume: 10 (0) units of food [food], 10 (0) units of terran breathing gas mixture [terair].",
                "  technologies: agricultural complex [agrplx], armored combat [armcbt].",
                "  items: 10 terrans [terran] (size: 40, mass: 40, upkeep: 10 cash [cash], consume: 10 units of food [food], 10 units of terran breathing gas mixture [terair]), 10 units of iron [iron] (size: 50, mass: 100), 300 cash [cash].",
                "  events:",
                "    week 1: consumed 10 units of iron [iron] to produce farming complex [farms] module.",
                string.Format("    week 4: produced farming complex [farms] into {0}.", farmsStack.ReportName)
            };

			List<string> lines;
			lines = moduleStack.Report(faction);
			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

			for (int i = 0; i < lines.Count; i++)
			{
                Assert.That(lines[i], Is.EqualTo(testlines1[i]));
			}
            Assert.That(lines.Count, Is.EqualTo(testlines1.Count));

			List<string> testlines2 = new List<string>
            {
                string.Format("+ farming complex [{0}], farming complex [farms], disabled, immobile.", farmsStack.Name),
                "  size: 1000, mass: 100 (100), capacity: 500/0, energy: 5, crew: 5/0, upkeep: 50 cash [cash].",
                "  events:",
                "    week 1: formed by factory [000004] with farming complex [farms].",
                "    week 4: received farming complex [farms] produced by factory [000004]."
            };			

           lines = farmsStack.Report(faction);
			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

			for (int i = 0; i < lines.Count; i++)
			{
                Assert.That(lines[i], Is.EqualTo(testlines2[i]));
			}
            Assert.That(lines.Count, Is.EqualTo(testlines2.Count));

		}

		[Test]
		public void UnitReport_ModuleDamage()
		{
			Faction faction = this.game.Factions["2"];
			ModuleStack moduleStack = ModuleStack.All["000006"];
			moduleStack.Modules[0].Damage = 26;
			moduleStack.Modules[0].CaptureDamage = 9;

			List<string> testlines = new List<string>
			{
				"+ core drill [000006], 2 core drills [cdrill], immobile.",
				"  size: 2000, mass: 2148 (2000), capacity: 1500/98, energy: 10, crew: 12/12, upkeep: 92 (80) cash [cash], consume: 12 (0) units of food [food], 12 (0) units of terran breathing gas mixture [terair].",
				"  hit points: 200/174, capture: 9.",
				"    #1 hit points: 100/74, capture: 9, lightly damaged.",
				"    #2 hit points: 100/100.",
				"  technologies: hydrocarbons drilling [hcdril].",
				"  items: 10 units of iron [iron] (size: 50, mass: 100), 12 terrans [terran] (size: 48, mass: 48, upkeep: 12 cash [cash], consume: 12 units of food [food], 12 units of terran breathing gas mixture [terair])."
			};

			List<string> lines = moduleStack.Report(faction);
			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

			for (int i = 0; i < testlines.Count; i++)
			{
				Assert.That(lines[i], Is.EqualTo(testlines[i]), "line number " + i);
			}
			Assert.That(lines.Count, Is.EqualTo(testlines.Count));
		}

		[Test]
		public void BattleReport_ShipVsStation_ShipPerspective()
		{

            Faction faction = this.game.Factions["2"];
			Battles battles = this.game.Battles;
			ModuleStack frigate = ModuleStack.All["100011"];
			ModuleStack station = ModuleStack.All["100021"];

            Sequence.Rolls.Clear();
            Sequence.Ints.Push(120);
            Sequence.Ints.Push(1);
            Sequence.Ints.Push(120);
            Sequence.Ints.Push(1);
            Sequence.Ints.Push(120);
            Sequence.Ints.Push(1);
			Sequence.Ints.Push(20);

			Battle battle = new Battle(frigate, station);
			battle.Execute(this.game.Week);

			List<string> testlines = new List<string>
            {
                "Battles report:",
                "",
                "  Week 1.",
                "  Battle has commenced at orbit [O00003] of Earth [P00002] at AU 1, ocean planet [ocean] in system Sol [SS0001] (0, 0, 0).",
                "  ------------------------------------------------------------",
                "  Round 1:",
                "  ------------------------------------------------------------",
                "  Attackers:",
                "  + Frigate [100011], spaceship hull [sshull].",
                "    size: 5000, mass: 10000/4680 (100), energy: 120/78 (1), crew: 6/30.",
                "    hit points: 370/370 (50/50), attack: 24 (0), defense: 17 (10), initiative: 50 (40).",
                "    tactics: disable.",
                "      #1 hit points: 50/50.",
                // attack is a sum of module values and technology and skills
                // base value 24
                // technology 24*0,1 = 2,4 => 3
                // inititive was calculated by the following formula ((energy supply / consumption) + (drive capacity / mass)) *10 + bonuses from technologies and skills
                // power 120/78 = 1,53 * 10
                // thrust 10000/5400 = 1,85 * 10  
                "    + command bridge [100012], command bridge [cbridg], immobile.",
                "      size: 800, mass: 340 (300), energy: 5, crew: 1/10.",
                "      hit points: 55/55, defense: 5, initiative: 10 (5).",
                "      technologies: military tactics [miltac] (initiative: 5).",
                "        #1 hit points: 55/55.",
                "      + terran officer [200002], terran [terran].",
                "        mass: 4, defense: 5, initiative: 5.",
                "        skills: frigate pilot [frgplt] (defense: 5, initiative: 5).",
                "    + fission reactor [100013], 2 fission reactors [fisrec], immobile.",
                "      size: 800, mass: 384 (280), energy: 120/20, crew: 2/6.",
                "      hit points: 90/90, attack: 4.",
                "        #1 hit points: 45/45.",
                "        #2 hit points: 45/45.",
                "    + reaction drive [100014], reaction drive [rctdrv].",
                "      size: 600, mass: 10000/804 (700), energy: 30, crew: 1/1.",
                "      hit points: 65/65.",
                "        #1 hit points: 65/65.",
                "    + x-ray laser [100015], 2 x-ray lasers [xraylz], immobile.",
                "      size: 200, mass: 208 (200), energy: 20, crew: 2/2.",
                "      hit points: 20/20, attack: 20, defense: 2.",
                "        #1 hit points: 10/10.",
                "        #2 hit points: 10/10.",
                "    + crew quarters [100016], 2 crew quarters [crwqrt], immobile.",
                "      size: 1000, mass: 2844 (800), energy: 2.",
                "      hit points: 90/90.",
                "        #1 hit points: 45/45.",
                "        #2 hit points: 45/45.",
                "  Defenders:",
                "  - Station [100021], orbital complex [orcmpx], immobile, owned by NPC [1].",
                "    size: 5000.",
                "    hit points: 305/305 (50/50), attack: 2 (0), defense: 5 (0), initiative: 40.",
                // initiative 
                // power => 40/11 = 3 * 10 => 30 
                // thrust => immobile => 0
                "      #1 hit points: 50/50.",
                "    - command bridge [100022], command bridge [cbridg], immobile, owned by NPC [1].",
                "      size: 800.",
                "      hit points: 55/55, defense: 5.",
                "        #1 hit points: 55/55.",
                "      - terran officer [200003], terran [terran], working for NPC [1].",
                "        defense: 5.",
                "    - fission reactor [100023], fission reactor [fisrec], immobile, owned by NPC [1].",
                "      size: 400.",
                "      hit points: 45/45, attack: 2.",
                "        #1 hit points: 45/45.",
                "    - small cargo bay [100024], 2 small cargo bays [cargob], immobile, owned by NPC [1].",
                "      size: 4000.",
                "      hit points: 110/110.",
                "        #1 hit points: 55/55.",
                "        #2 hit points: 55/55.",
                "    - crew quarters [100025], crew quarters [crwqrt], immobile, owned by NPC [1].",
                "      size: 500.",
                "      hit points: 45/45.",
                "        #1 hit points: 45/45.",
                "  ------------------------------------------------------------",
                // disable tactics reduces hit chances by half but treat command, energy and drive modulestacks as double size for hit resolution
                // calculated with weapon attack + bonuses (technologies, skills, fleets): (24)/2 = 12, and attacker attak + defender defence: 11 + 5
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and misses.",
                // 1-50 complex, 51-66 bridge, 67-74 reactor, 75-95 cargo #1, 96-116 cargo #2
                // damage is calculated with attack / 10;
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and hits #1 fission reactor [100023] doing 10 damage.",
                "  ------------------------------------------------------------",
                "  Round 2:",
                "  ------------------------------------------------------------",
                "  Attackers:",
                "  + Frigate [100011], spaceship hull [sshull].",
                "    size: 5000, mass: 10000/4680 (100), energy: 120/78 (1), crew: 6/30.",
                "    hit points: 370/370 (50/50), attack: 24 (0), defense: 17 (10), initiative: 50 (40).",
                "    tactics: disable.",
                "      #1 hit points: 50/50.",
                "    + command bridge [100012], command bridge [cbridg], immobile.",
                "      size: 800, mass: 340 (300), energy: 5, crew: 1/10.",
                "      hit points: 55/55, defense: 5, initiative: 10 (5).",
                "      technologies: military tactics [miltac] (initiative: 5).",
                "        #1 hit points: 55/55.",
                "      + terran officer [200002], terran [terran].",
                "        mass: 4, defense: 5, initiative: 5.",
                "        skills: frigate pilot [frgplt] (defense: 5, initiative: 5).",
                "    + fission reactor [100013], 2 fission reactors [fisrec], immobile.",
                "      size: 800, mass: 384 (280), energy: 120/20, crew: 2/6.",
                "      hit points: 90/90, attack: 4.",
                "        #1 hit points: 45/45.",
                "        #2 hit points: 45/45.",
                "    + reaction drive [100014], reaction drive [rctdrv].",
                "      size: 600, mass: 10000/804 (700), energy: 30, crew: 1/1.",
                "      hit points: 65/65.",
                "        #1 hit points: 65/65.",
                "    + x-ray laser [100015], 2 x-ray lasers [xraylz], immobile.",
                "      size: 200, mass: 208 (200), energy: 20, crew: 2/2.",
                "      hit points: 20/20, attack: 20, defense: 2.",
                "        #1 hit points: 10/10.",
                "        #2 hit points: 10/10.",
                "    + crew quarters [100016], 2 crew quarters [crwqrt], immobile.",
                "      size: 1000, mass: 2844 (800), energy: 2.",
                "      hit points: 90/90.",
                "        #1 hit points: 45/45.",
                "        #2 hit points: 45/45.",
                "  Defenders:",
                "  - Station [100021], orbital complex [orcmpx], immobile, owned by NPC [1].",
                "    size: 5000.",
                "    hit points: 305/295 (50/50), attack: 2 (0), defense: 5 (0), initiative: 40.",
                "      #1 hit points: 50/50.",
                "    - command bridge [100022], command bridge [cbridg], immobile, owned by NPC [1].",
                "      size: 800.",
                "      hit points: 55/55, defense: 5.",
                "        #1 hit points: 55/55.",
                "      - terran officer [200003], terran [terran], working for NPC [1].",
                "        defense: 5.",
                "    - fission reactor [100023], fission reactor [fisrec], immobile, owned by NPC [1].",
                "      size: 400.",
                "      hit points: 45/35, attack: 2.",
                "        #1 hit points: 45/35.",
                "    - small cargo bay [100024], 2 small cargo bays [cargob], immobile, owned by NPC [1].",
                "      size: 4000.",
                "      hit points: 110/110.",
                "        #1 hit points: 55/55.",
                "        #2 hit points: 55/55.",
                "    - crew quarters [100025], crew quarters [crwqrt], immobile, owned by NPC [1].",
                "      size: 500.",
                "      hit points: 45/45.",
                "        #1 hit points: 45/45.",
                "  ------------------------------------------------------------",
                // Station won initiative but cannot attack
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and hits #1 fission reactor [100023] doing 10 damage.",
                "    #1 fission reactor [fisrec] is lightly damaged.",
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and hits #1 fission reactor [100023] doing 10 damage.",
                "    #1 fission reactor [fisrec] is heavily damaged.",
                "    #1 fission reactor [fisrec] is disabled.",
                "    Station [100021] lost it's power supply and disables.",
                "  ------------------------------------------------------------",
                "  Battle won by attackers.",
                ""
            };

            Console.WriteLine("Random generator log: ");
            foreach (Sequence.RollDescription roll in Sequence.Rolls)
            {
                Console.WriteLine(string.Format("roll: {0} description: {1}", roll.Roll.ToString(), roll.Description));
            }

            List<string> lines = battles.Report(faction);
			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

			for (int i = 0; i < lines.Count; i++)
			{
                Assert.That(lines[i], Is.EqualTo(testlines[i]));
			}
            Assert.That(lines.Count, Is.EqualTo(testlines.Count));
		}

		private static List<string> captureCombatNarrative(List<string> lines)
		{
			List<string> narrative = new List<string>();
			foreach (string line in lines)
			{
				string trimmed = line.Trim();
				if (trimmed.StartsWith("Week ")
					|| trimmed.StartsWith("Battle has commenced")
					|| trimmed.StartsWith("Round ")
					|| trimmed.StartsWith("----")
					|| trimmed.Contains("tactics: capture")
					|| trimmed.Contains("fires ")
					|| trimmed.Contains("disabled (capture)")
					|| trimmed.Contains("captured")
					|| trimmed.Contains("killed")
					|| trimmed.Contains("wounded")
					|| trimmed.Contains("items:")
					|| trimmed.Contains("[c100024]")
					|| trimmed.Contains("is wrecked")
					|| trimmed.Contains("lost it's")
					|| trimmed.Contains("Battle won")
					|| trimmed.Contains("Battle ended")
					|| trimmed.Contains("evades and leaves"))
				{
					narrative.Add(line);
				}
			}
			return narrative;
		}

		private void resetModuleBattleDamage(ModuleStack stack)
		{
			foreach (Module module in stack.Modules)
			{
				module.Damage = 0;
				module.CaptureDamage = 0;
			}
			foreach (ModuleStack nested in stack.ModuleStacks.Values)
			{
				this.resetModuleBattleDamage(nested);
			}
		}

		[Test]
		public void CaptureBattleReport_ShipVsStation_ShipPerspective()
		{
			Faction faction = this.game.Factions["2"];
			Battles battles = this.game.Battles;
			ModuleStack frigate = ModuleStack.All["100011"];
			ModuleStack station = ModuleStack.All["100021"];

			frigate.ApplyTactic("capture");
			this.resetModuleBattleDamage(station);

			Sequence.Rolls.Clear();
			Sequence.Ints.Clear();
			// LIFO: last push is consumed first. Location 250 is past hull/command/reactor
			// on the capture-weighted station and lands on cargo [100024].
			for (int i = 0; i < 20; i++)
			{
				Sequence.Ints.Push(250);
				Sequence.Ints.Push(1);   // hit
			}
			Sequence.Ints.Push(20); // first shot misses (20 > chance 18)

			Battle battle = new Battle(frigate, station);
			battle.Execute(this.game.Week);

			List<string> lines = battles.Report(faction);
			List<string> narrative = captureCombatNarrative(lines);

			Console.WriteLine("Capture battle narrative:");
			foreach (string line in narrative)
			{
				Console.WriteLine(line);
			}

			Assert.That(battle.Round, Is.LessThanOrEqualTo(Battle.MaxRounds));
			Assert.That(string.Join("\n", lines.ToArray()), Does.Not.Contain("Round 11"));

			ModuleStack capturedCargo = ModuleStack.All["c100024"];
			Assert.That(capturedCargo.Owner.Name, Is.EqualTo("2"));
			Assert.That(capturedCargo.ModuleType.Name, Is.EqualTo("cargob"));
			Assert.That(capturedCargo.Quantity, Is.EqualTo(1));
			Assert.That(capturedCargo.ItemStacks.Quantity("iron"), Is.EqualTo(50));
			Assert.That(capturedCargo.ItemStacks.Quantity("titani"), Is.EqualTo(50));
			Assert.That(capturedCargo.ItemStacks.Quantity("copper"), Is.EqualTo(50));
			Assert.That(capturedCargo.ItemStacks.Quantity("silici"), Is.EqualTo(50));
			Assert.That(capturedCargo.ItemStacks.Quantity("h2o2"), Is.EqualTo(500));
			Assert.That(capturedCargo.ItemStacks.Quantity("terran"), Is.EqualTo(2));

			List<string> expected = new List<string>
            {
                "  Week 1.",
                "  Battle has commenced at orbit [O00003] of Earth [P00002] at AU 1, ocean planet [ocean] in system Sol [SS0001] (0, 0, 0).",
                "  ------------------------------------------------------------",
                "  Round 1:",
                "  ------------------------------------------------------------",
                "    tactics: capture.",
                "  ------------------------------------------------------------",
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and misses.",
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and hits #1 small cargo bay [100024] doing 2 damage and 8 capture damage.",
                "  ------------------------------------------------------------",
                "  Round 2:",
                "  ------------------------------------------------------------",
                "    tactics: capture.",
                "  ------------------------------------------------------------",
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and hits #1 small cargo bay [100024] doing 2 damage and 8 capture damage.",
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and hits #1 small cargo bay [100024] doing 2 damage and 8 capture damage.",
                "  ------------------------------------------------------------",
                "  Round 3:",
                "  ------------------------------------------------------------",
                "    tactics: capture.",
                "  ------------------------------------------------------------",
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and hits #1 small cargo bay [100024] doing 2 damage and 8 capture damage.",
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and hits #1 small cargo bay [100024] doing 2 damage and 8 capture damage.",
                "  ------------------------------------------------------------",
                "  Round 4:",
                "  ------------------------------------------------------------",
                "    tactics: capture.",
                "  ------------------------------------------------------------",
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and hits #1 small cargo bay [100024] doing 2 damage and 3 capture damage.",
                "    #1 small cargo bay [cargob] module captured by Caste Prime [2].",
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and hits #1 small cargo bay [100024] doing 2 damage and 8 capture damage.",
                "  ------------------------------------------------------------",
                "  Round 5:",
                "  ------------------------------------------------------------",
                "    tactics: capture.",
                "    + small cargo bay [c100024], small cargo bay [cargob], disabled, immobile.",
                "      items: 50 units of iron [iron], 50 units of titanium [titani], 50 units of copper [copper], 50 units of silicium [silici], 500 units of oxyhydro [h2o2], 2 terrans [terran].",
            };
			for (int i = 0; i < expected.Count; i++)
			{
				Assert.That(narrative[i], Is.EqualTo(expected[i]), "narrative line " + i);
			}
			Assert.That(narrative.Count, Is.GreaterThanOrEqualTo(expected.Count));
		}

		[Test]
		public void CaptureBattleReport_LastCommandCapturesParent_ShipPerspective()
		{
			Faction faction = this.game.Factions["2"];
			Battles battles = this.game.Battles;
			ModuleStack frigate = ModuleStack.All["100011"];
			ModuleStack station = ModuleStack.All["100021"];
			ModuleStack command = ModuleStack.All["100022"];
			ModuleStack cargo = ModuleStack.All["100024"];

			frigate.ApplyTactic("capture");
			this.resetModuleBattleDamage(station);

			new Person(command, command.Owner, Race.All["terran"], "200010");
			new Person(command, command.Owner, Race.All["terran"], "200011");
			new Person(command, command.Owner, Race.All["terran"], "200012");

			Sequence.Rolls.Clear();
			Sequence.Ints.Clear();
			for (int i = 0; i < 10; i++)
			{
				Sequence.Ints.Push(120);
				Sequence.Ints.Push(1);
			}
			Sequence.Ints.Push(20);

			Battle battle = new Battle(frigate, station);
			battle.Execute(this.game.Week);

			List<string> lines = battles.Report(faction);
			List<string> narrative = captureCombatNarrative(lines);

			Console.WriteLine("Last-command parent capture narrative:");
			foreach (string line in narrative)
			{
				Console.WriteLine(line);
			}

			Assert.That(station.Owner.Name, Is.EqualTo("2"));
			Assert.That(station.Name, Is.EqualTo("100021"));
			Assert.That(cargo.Name, Is.EqualTo("100024"));
			Assert.That(cargo.Owner.Name, Is.EqualTo("2"));
			ModuleStack capturedCommand = ModuleStack.All["c100022"];
			Assert.That(capturedCommand.Owner.Name, Is.EqualTo("2"));
			Assert.That(capturedCommand.Quantity, Is.EqualTo(1));
			Assert.That(Person.All.Contains("200003"), Is.False);
			Assert.That(Person.All["200010"].Race.Name, Is.EqualTo("wndtrn"));
			Assert.That(Person.All["200011"].Race.Name, Is.EqualTo("wndtrn"));
			Assert.That(Person.All["200012"].Race.Name, Is.EqualTo("terran"));
			Assert.That(Person.All["200012"].Parent, Is.EqualTo(capturedCommand));
			Assert.That(capturedCommand.ItemStacks.Quantity("terran"), Is.EqualTo(3));
			Assert.That(capturedCommand.ItemStacks.Quantity("wndtrn"), Is.EqualTo(4));

			List<string> expected = new List<string>
            {
                "  Week 1.",
                "  Battle has commenced at orbit [O00003] of Earth [P00002] at AU 1, ocean planet [ocean] in system Sol [SS0001] (0, 0, 0).",
                "  ------------------------------------------------------------",
                "  Round 1:",
                "  ------------------------------------------------------------",
                "    tactics: capture.",
                "  ------------------------------------------------------------",
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and misses.",
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and hits #1 command bridge [100022] doing 2 damage and 8 capture damage.",
                "  ------------------------------------------------------------",
                "  Round 2:",
                "  ------------------------------------------------------------",
                "    tactics: capture.",
                "  ------------------------------------------------------------",
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and hits #1 command bridge [100022] doing 2 damage and 8 capture damage.",
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and hits #1 command bridge [100022] doing 2 damage and 8 capture damage.",
                "  ------------------------------------------------------------",
                "  Round 3:",
                "  ------------------------------------------------------------",
                "    tactics: capture.",
                "  ------------------------------------------------------------",
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and hits #1 command bridge [100022] doing 2 damage and 8 capture damage.",
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and hits #1 command bridge [100022] doing 2 damage and 8 capture damage.",
                "  ------------------------------------------------------------",
                "  Round 4:",
                "  ------------------------------------------------------------",
                "    tactics: capture.",
                "  ------------------------------------------------------------",
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and hits #1 command bridge [100022] doing 2 damage and 3 capture damage.",
                "    #1 command bridge [cbridg] module captured by Caste Prime [2].",
                "    2 terrans [terran] killed.",
                "    4 terrans [terran] wounded.",
                "    3 terrans [terran] captured.",
                "    terran officer [200003] is killed.",
                "    terran officer [200010] is wounded.",
                "    terran officer [200011] is wounded.",
                "    terran officer [200012] is captured.",
                "    Station [100021] captured by Caste Prime [2].",
                "  ------------------------------------------------------------",
                "  Battle won by attackers.",
            };
			for (int i = 0; i < expected.Count; i++)
			{
				Assert.That(narrative[i], Is.EqualTo(expected[i]), "narrative line " + i);
			}
			Assert.That(narrative.Count, Is.EqualTo(expected.Count));
		}

		[Test]
		public void BattleReport_ShipVsStation_StationPerspective()
		{
			Faction faction = this.game.Factions["1"];
			Battles battles = this.game.Battles;
			ModuleStack frigate = ModuleStack.All["100011"];
			ModuleStack station = ModuleStack.All["100021"];

            Sequence.Rolls.Clear();
            Sequence.Ints.Push(120);
            Sequence.Ints.Push(1);
            Sequence.Ints.Push(120);
            Sequence.Ints.Push(1);
            Sequence.Ints.Push(120);
            Sequence.Ints.Push(1);
            Sequence.Ints.Push(20);

            Battle battle = new Battle(frigate, station);
			battle.Execute(this.game.Week);

			List<string> testlines = new List<string>
            {
                "Battles report:",
                "",
                "  Week 1.",
                "  Battle has commenced at orbit [O00003] of Earth [P00002] at AU 1, ocean planet [ocean] in system Sol [SS0001] (0, 0, 0).",
                "  ------------------------------------------------------------",
                "  Round 1:",
                "  ------------------------------------------------------------",
                "  Attackers:",
                "  - Frigate [100011], spaceship hull [sshull], owned by Caste Prime [2].",
                "    size: 5000.",
                "    hit points: 370/370 (50/50), attack: 24 (0), defense: 17 (10), initiative: 50 (40).",
                "      #1 hit points: 50/50.",
                "    - command bridge [100012], command bridge [cbridg], immobile, owned by Caste Prime [2].",
                "      size: 800.",
                "      hit points: 55/55, defense: 5, initiative: 10 (5).",
                "        #1 hit points: 55/55.",
                "      - terran officer [200002], terran [terran], working for Caste Prime [2].",
                "        defense: 5, initiative: 5.",
                "    - fission reactor [100013], 2 fission reactors [fisrec], immobile, owned by Caste Prime [2].",
                "      size: 800.",
                "      hit points: 90/90, attack: 4.",
                "        #1 hit points: 45/45.",
                "        #2 hit points: 45/45.",
                "    - reaction drive [100014], reaction drive [rctdrv], owned by Caste Prime [2].",
                "      size: 600.",
                "      hit points: 65/65.",
                "        #1 hit points: 65/65.",
                "    - x-ray laser [100015], 2 x-ray lasers [xraylz], immobile, owned by Caste Prime [2].",
                "      size: 200.",
                "      hit points: 20/20, attack: 20, defense: 2.",
                "        #1 hit points: 10/10.",
                "        #2 hit points: 10/10.",
                "    - crew quarters [100016], 2 crew quarters [crwqrt], immobile, owned by Caste Prime [2].",
                "      size: 1000.",
                "      hit points: 90/90.",
                "        #1 hit points: 45/45.",
                "        #2 hit points: 45/45.",
                "  Defenders:",
                "  + Station [100021], orbital complex [orcmpx], immobile.",
                "    size: 5000, mass: 6564 (100), energy: 60/17 (1), crew: 2/21.",
                "    hit points: 305/305 (50/50), attack: 2 (0), defense: 5 (0), initiative: 40.",
                "      #1 hit points: 50/50.",
                "    + command bridge [100022], command bridge [cbridg], immobile.",
                "      size: 800, mass: 340 (300), energy: 5, crew: 1/10.",
                "      hit points: 55/55, defense: 5.",
                "        #1 hit points: 55/55.",
                "      + terran officer [200003], terran [terran].",
                "        mass: 4, defense: 5.",
                "        skills: space station command [sscmnd] (defense: 5).",
                "    + fission reactor [100023], fission reactor [fisrec], immobile.",
                "      size: 400, mass: 192 (140), energy: 60/10, crew: 1/3.",
                "      hit points: 45/45, attack: 2.",
                "        #1 hit points: 45/45.",
                "    + small cargo bay [100024], 2 small cargo bays [cargob], immobile.",
                "      size: 4000, mass: 4516 (400).",
                "      hit points: 110/110.",
                "        #1 hit points: 55/55.",
                "        #2 hit points: 55/55.",
                "    + crew quarters [100025], crew quarters [crwqrt], immobile.",
                "      size: 500, mass: 1416 (400), energy: 1.",
                "      hit points: 45/45.",
                "        #1 hit points: 45/45.",
                "  ------------------------------------------------------------",
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and misses.",
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and hits #1 fission reactor [100023] doing 10 damage.",
                "  ------------------------------------------------------------",
                "  Round 2:",
                "  ------------------------------------------------------------",
                "  Attackers:",
                "  - Frigate [100011], spaceship hull [sshull], owned by Caste Prime [2].",
                "    size: 5000.",
                "    hit points: 370/370 (50/50), attack: 24 (0), defense: 17 (10), initiative: 50 (40).",
                "      #1 hit points: 50/50.",
                "    - command bridge [100012], command bridge [cbridg], immobile, owned by Caste Prime [2].",
                "      size: 800.",
                "      hit points: 55/55, defense: 5, initiative: 10 (5).",
                "        #1 hit points: 55/55.",
                "      - terran officer [200002], terran [terran], working for Caste Prime [2].",
                "        defense: 5, initiative: 5.",
                "    - fission reactor [100013], 2 fission reactors [fisrec], immobile, owned by Caste Prime [2].",
                "      size: 800.",
                "      hit points: 90/90, attack: 4.",
                "        #1 hit points: 45/45.",
                "        #2 hit points: 45/45.",
                "    - reaction drive [100014], reaction drive [rctdrv], owned by Caste Prime [2].",
                "      size: 600.",
                "      hit points: 65/65.",
                "        #1 hit points: 65/65.",
                "    - x-ray laser [100015], 2 x-ray lasers [xraylz], immobile, owned by Caste Prime [2].",
                "      size: 200.",
                "      hit points: 20/20, attack: 20, defense: 2.",
                "        #1 hit points: 10/10.",
                "        #2 hit points: 10/10.",
                "    - crew quarters [100016], 2 crew quarters [crwqrt], immobile, owned by Caste Prime [2].",
                "      size: 1000.",
                "      hit points: 90/90.",
                "        #1 hit points: 45/45.",
                "        #2 hit points: 45/45.",
                "  Defenders:",
                "  + Station [100021], orbital complex [orcmpx], immobile.",
                "    size: 5000, mass: 6564 (100), energy: 60/17 (1), crew: 2/21.",
                "    hit points: 305/295 (50/50), attack: 2 (0), defense: 5 (0), initiative: 40.",
                "      #1 hit points: 50/50.",
                "    + command bridge [100022], command bridge [cbridg], immobile.",
                "      size: 800, mass: 340 (300), energy: 5, crew: 1/10.",
                "      hit points: 55/55, defense: 5.",
                "        #1 hit points: 55/55.",
                "      + terran officer [200003], terran [terran].",
                "        mass: 4, defense: 5.",
                "        skills: space station command [sscmnd] (defense: 5).",
                "    + fission reactor [100023], fission reactor [fisrec], immobile.",
                "      size: 400, mass: 192 (140), energy: 60/10, crew: 1/3.",
                "      hit points: 45/35, attack: 2.",
                "        #1 hit points: 45/35.",
                "    + small cargo bay [100024], 2 small cargo bays [cargob], immobile.",
                "      size: 4000, mass: 4516 (400).",
                "      hit points: 110/110.",
                "        #1 hit points: 55/55.",
                "        #2 hit points: 55/55.",
                "    + crew quarters [100025], crew quarters [crwqrt], immobile.",
                "      size: 500, mass: 1416 (400), energy: 1.",
                "      hit points: 45/45.",
                "        #1 hit points: 45/45.",
                "  ------------------------------------------------------------",
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and hits #1 fission reactor [100023] doing 10 damage.",
                "    #1 fission reactor [fisrec] is lightly damaged.",
                "  Frigate [100011] fires x-ray laser [xraylz] on Station [100021] (chance: 18/29) and hits #1 fission reactor [100023] doing 10 damage.",
                "    #1 fission reactor [fisrec] is heavily damaged.",
                "    #1 fission reactor [fisrec] is disabled.",
                "    Station [100021] lost it's power supply and disables.",
                "  ------------------------------------------------------------",
                "  Battle won by attackers.",
                ""
            };
			List<string> lines = battles.Report(faction);
			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

			for (int i = 0; i < lines.Count; i++)
			{
                Assert.That(lines[i], Is.EqualTo(testlines[i]));
			}
            Assert.That(lines.Count, Is.EqualTo(testlines.Count));
		}

		[Test]
		public void BankReport()
		{
			Faction faction = this.game.Factions["2"];
			Bank bank = faction.Bank;
			List<string> testlines = new List<string>
            {
                "Bank report:",
                "  Bank account balance: 10000.",
                "  Credit line maximum: 10000.",
                "  Credit rate: 20%, Deposit rate: 5%."
            };
			List<string> lines = bank.Report(faction);
			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

			for (int i = 0; i < lines.Count; i++)
			{
                Assert.That(lines[i], Is.EqualTo(testlines[i]));
			}
            Assert.That(lines.Count, Is.EqualTo(testlines.Count));
		}

		[Test]
		public void BankBalance_RoundsToWholeCredits()
		{
			Faction faction = this.game.Factions["2"];
			Bank bank = faction.Bank;
			bank.Balance = 6221.9422265625;
			Assert.That(bank.Balance, Is.EqualTo(6222));
			Assert.That(bank.Report(faction)[1], Is.EqualTo("  Bank account balance: 6222."));

			bank.Balance = 10000;
			bank.AddQuarterlyInterest(13);
			Assert.That(bank.Balance, Is.EqualTo(10125));
		}

		[Test]
		public void MoonReport_IncludesOrbitStacks()
		{
			Orbit lunaOrbit = Orbit.All["O00004"];
			Moon luna = (Moon)lunaOrbit.OrbitHolder;
			Faction faction = this.game.Factions["2"];
			ModuleStack shuttle = ModuleStack.All.GetOrCreateNewModuleStack(faction, "100100");
			shuttle.Parent = lunaOrbit;
			shuttle.ModuleType = ModuleType.All["shuttl"];
			shuttle.AddModule();

			List<string> lines = luna.Report(faction);
			bool foundOrbit = false;
			bool foundShuttle = false;
			foreach (string line in lines)
			{
				if (line.IndexOf("orbit [O00004]") >= 0)
				{
					foundOrbit = true;
				}
				if (line.IndexOf("[100100]") >= 0)
				{
					foundShuttle = true;
				}
			}

			Assert.That(foundOrbit, Is.True, "moon report should include its orbit");
			Assert.That(foundShuttle, Is.True, "owned stacks in moon orbit should appear in the moon report");
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

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "use agrplx",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is UseOrder);
			UseOrder useOrder = (UseOrder)testModuleStack.Orders[0];
            Assert.That(useOrder.Technology, Is.EqualTo(Technology.All["agrplx"]));
            Assert.That(useOrder.Repeat, Is.EqualTo(1));

            Assert.That(Technology.All["agrplx"].UseTime, Is.EqualTo(4));

			testModuleStack.Execute(this.game.Week);
			this.consoleOutReport("factory:", testModuleStack, testFaction);

			List<string> testlines = new List<string>
            {
                //testlines.Add("#modulestack 000004");
                "+ factory [000004], factory [factry], immobile.",
                "technologies: agricultural complex [agrplx], armored combat [armcbt].",
                "items: 10 terrans [terran], 10 units of iron [iron], 300 cash [cash].",
                "effects:",
                "  producing farming complex [farms] into farming complex [100], 3 weeks to complete.",
                ""
            };

			List<string> lines = testModuleStack.ReportOrdersTemplateHeader(testFaction);

			for (int i = 0; i < lines.Count; i++)
			{
				Console.WriteLine(lines[i]);
			}

			for (int i = 0; i < lines.Count; i++)
			{
                Assert.That(lines[i], Is.EqualTo(testlines[i]), "error in line " + i);
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

			List<string> testlines = new List<string>
            {
                "Market report:",
                "  Offers of selling items:",
                "    none.",
                "  Offers of buying items:",
                "    buy all terrans [terran] at any price by Berlin [000005].",
                "  Offers of selling modules:",
                "    none.",
                "  Offers of buying modules:",
                "    none.",
                "  Offers of selling technologies:",
                "    none.",
                "  Offers of buying technologies:",
                "    none.",
                ""
            };

			List<string> lines = market.Report(faction);

			for (int i = 0; i < lines.Count; i++)
			{
                Assert.That(lines[i], Is.EqualTo(testlines[i]), "error in line " + i);
			}
		}

        [Test]
        public void ProduceOrderReport()
        {
            // unlimited orders should report in next turn templates with parameter
            // @produce energy or @produce cash instead of @produce
            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["000003"];

            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000003",
                "@produce energy",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            this.consoleOutReport("orders:", testModuleStack.Orders, testFaction);

            List<string> lines = testModuleStack.Orders.Report(testFaction);

            Assert.That(lines[0], Is.EqualTo("@produce energy"));
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

            List<string> testlines = new List<string>
            {
                "Market report:",
                "  Offers of selling items:",
                "    none.",
                "  Offers of buying items:",
                "    buy all terrans [terran] at any price by Berlin [000005].",
                "    buy 100 units of food [food] at 1 each by trucks [100001].",
                "  Offers of selling modules:",
                "    none.",
                "  Offers of buying modules:",
                "    none.",
                "  Offers of selling technologies:",
                "    none.",
                "  Offers of buying technologies:",
                "    none.",
                ""
            };

            List<string> lines = market.Report(faction);

            for (int i = 0; i < lines.Count; i++)
            {
                Assert.That(lines[i], Is.EqualTo(testlines[i]), "error in line " + i);
            }

            this.dataFile.SaveGame(Directory.GetCurrentDirectory(), "gameout.marketsequence.xml");

            this.game.Week = 1;
            this.game.ClearDictionaries();
            this.game = null;
            this.dataFile = null;			

			this.dataFile = new DataFile(Directory.GetCurrentDirectory());
            this.dataFile.LoadGameDocument(Directory.GetCurrentDirectory(), "gameout.marketsequence.xml");
            this.dataFile.LoadConfiguration(Directory.GetCurrentDirectory());
            this.dataFile.LoadFactions();
            this.dataFile.LoadGalaxy();
            this.game = this.dataFile.Game;

            // restart variables
            faction = this.game.Factions["2"];
            region = this.game.Regions["R00001"];

            market = region.Market;
            this.consoleOutReport("market report:", market, faction);
            
            // output should be the same
            lines = market.Report(faction);

            for (int i = 0; i < lines.Count; i++)
            {
                Assert.That(lines[i], Is.EqualTo(testlines[i]), "error in line " + i);
            }
        }

		[Test]
		public void Execute_ClearsPreviousTurnEventReports()
		{
			ModuleStack factory = ModuleStack.All["000004"];
			Person ceo = Person.All["000101"];
			Faction faction = Faction.All["2"];
			Region region = Region.All["R00002"];
			factory.EventReports.Add(1, "leftover from previous turn.");
			ceo.EventReports.Add(1, "leftover from previous turn.");
			faction.EventReports.Add(1, "leftover from previous turn.");
			region.EventReports.Add(1, "leftover from previous turn.");

			this.game.Execute();

			Assert.That(this.containsEvent(factory.EventReports, "leftover from previous turn."), Is.False);
			Assert.That(this.containsEvent(ceo.EventReports, "leftover from previous turn."), Is.False);
			Assert.That(this.containsEvent(faction.EventReports, "leftover from previous turn."), Is.False);
			Assert.That(this.containsEvent(region.EventReports, "leftover from previous turn."), Is.False);
		}

		private bool containsEvent(EventReports events, string description)
		{
			foreach (EventReport eventReport in events)
			{
				if (eventReport.Description == description)
				{
					return true;
				}
			}
			return false;
		}

	}
}
