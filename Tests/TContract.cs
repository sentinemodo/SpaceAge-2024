using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TContract
	{
		private DataFile dataFile;
		private Game game;

		[SetUp]
		public void Setup()
		{
			this.dataFile = new DataFile(Directory.GetCurrentDirectory());
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadGame();
			this.game = this.dataFile.Game;
		}

		[TearDown]
		public void Teardown()
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

		private Contract Publish(string name, string locationName, string receiverName)
		{
			GiveModuleTrigger trigger = new GiveModuleTrigger(
				1,
				ModuleType.All["inftry"],
				ModuleStack.All[receiverName]);
			return new Contract(
				name,
				Region.All[locationName],
				Faction.All["1"],
				trigger,
				Technology.All["rckter"]);
		}

		private ModuleStack CreateInfantry(string name, Faction owner, IHolder parent, int quantity)
		{
			ModuleStack infantry = new ModuleStack(parent, owner, ModuleType.All["inftry"], name);
			infantry.AddModules(quantity);
			return infantry;
		}

		[Test]
		public void Parse_CreateAndWithdraw()
		{
			List<string> commands = new List<string>
			{
				"#faction 1",
				"CONTRACT R00002 give 1 inftry to 000001 REWARD rckter technology",
				"#end"
			};
			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(commands);

			ContractOrder create = (ContractOrder)Faction.All["1"].Orders[0];
			Assert.That(create.IsWithdraw, Is.False);
			Assert.That(create.Location.Name, Is.EqualTo("R00002"));
			Assert.That(create.Quantity, Is.EqualTo(1));
			Assert.That(create.ModuleType.Name, Is.EqualTo("inftry"));
			Assert.That(create.Receiver.Name, Is.EqualTo("000001"));
			Assert.That(create.RewardTechnology.Name, Is.EqualTo("rckter"));
			Assert.That(create.AllowedBetweenTurns, Is.True);

			create.Execute(1);
			Assert.That(create.Executed, Is.True);
			Assert.That(Contract.All.Count, Is.EqualTo(1));
			string contractName = Contract.All[0].Name;
			Assert.That(contractName.StartsWith(Contract.NamePrefix), Is.True);
			Assert.That(contractName.Length, Is.EqualTo(NamedObject.MaxNameLength));
			Assert.That(Contract.All[0].CreatedThisSession, Is.True);

			List<string> withdrawCommands = new List<string>
			{
				"#faction 1",
				"contract " + contractName + " WITHDRAW",
				"#end"
			};
			reader.AssignOrders(withdrawCommands);
			ContractOrder withdraw = (ContractOrder)Faction.All["1"].Orders[Faction.All["1"].Orders.Count - 1];
			Assert.That(withdraw.IsWithdraw, Is.True);
			Assert.That(withdraw.ContractName, Is.EqualTo(contractName));

			withdraw.Execute(1);
			Assert.That(withdraw.Executed, Is.True);
			Assert.That(Contract.All.Count, Is.EqualTo(0));
		}

		[Test]
		public void Parse_RejectsUnknownLocation()
		{
			List<string> commands = new List<string>
			{
				"#faction 1",
				"contract R99999 give 1 inftry to 000001 reward rckter technology",
				"#end"
			};
			OrdersReader reader = new OrdersReader(this.game);
			Assert.Throws<Exception>(() => reader.AssignOrders(commands));
		}

		[Test]
		public void XmlRoundTrip_PersistsOpenContract()
		{
			this.Publish("CT0001", "R00002", "000001");
			GiveModuleTrigger trigger = (GiveModuleTrigger)Contract.All["CT0001"].Trigger;
			int baseline = trigger.Baseline;

			string testdir = Directory.GetCurrentDirectory();
			string testfile = "gameout.contracts.xml";
			this.dataFile.SaveGame(testdir, testfile);

			XmlDocument saved = new XmlDocument();
			saved.Load(Path.Combine(testdir, testfile));
			XmlElement elContract = (XmlElement)saved.SelectSingleNode("/game/contracts/contract[@name='CT0001']");
			Assert.That(elContract, Is.Not.Null);
			Assert.That(elContract.GetAttribute("location"), Is.EqualTo("R00002"));
			Assert.That(elContract.GetAttribute("issuer"), Is.EqualTo("1"));
			Assert.That(elContract.GetAttribute("trigger"), Is.EqualTo("give-module"));
			Assert.That(elContract.GetAttribute("quantity"), Is.EqualTo("1"));
			Assert.That(elContract.GetAttribute("module"), Is.EqualTo("inftry"));
			Assert.That(elContract.GetAttribute("receiver"), Is.EqualTo("000001"));
			Assert.That(elContract.GetAttribute("reward-type"), Is.EqualTo("technology"));
			Assert.That(elContract.GetAttribute("reward"), Is.EqualTo("rckter"));
			Assert.That(elContract.GetAttribute("baseline"), Is.EqualTo(baseline.ToString()));

			this.game.ClearDictionaries();
			this.game = null;
			this.dataFile = null;

			this.dataFile = new DataFile(testdir);
			this.dataFile.LoadGameDocument(testdir, testfile);
			this.dataFile.LoadConfiguration(testdir);
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.dataFile.LoadContracts();
			this.game = this.dataFile.Game;

			Assert.That(Contract.All.Count, Is.EqualTo(1));
			Contract loaded = Contract.All["CT0001"];
			Assert.That(loaded, Is.Not.Null);
			Assert.That(loaded.Location.Name, Is.EqualTo("R00002"));
			Assert.That(loaded.Issuer.Name, Is.EqualTo("1"));
			Assert.That(loaded.RewardTechnology.Name, Is.EqualTo("rckter"));
			GiveModuleTrigger loadedTrigger = (GiveModuleTrigger)loaded.Trigger;
			Assert.That(loadedTrigger.Quantity, Is.EqualTo(1));
			Assert.That(loadedTrigger.ModuleType.Name, Is.EqualTo("inftry"));
			Assert.That(loadedTrigger.Receiver.Name, Is.EqualTo("000001"));
			Assert.That(loadedTrigger.Baseline, Is.EqualTo(baseline));
			Assert.That(loaded.CreatedThisSession, Is.False);
		}

		[Test]
		public void Transfer_FirstGiverReceivesRckterAndRemovesContract()
		{
			this.Publish("CT0001", "R00002", "000001");
			Faction player = Faction.All["2"];
			ModuleStack infantry = this.CreateInfantry("i99901", player, Region.All["R00002"], 2);
			ModuleStack city = ModuleStack.All["000001"];

			TransferOrder first = new TransferOrder(infantry, city, ModuleType.All["inftry"], 1, 0);
			first.Execute(1);
			Assert.That(first.Executed, Is.True);

			Contract.All.Evaluate(1);
			Assert.That(Contract.All.Count, Is.EqualTo(0));
			Assert.That(infantry.Technologies.Contains("rckter"), Is.False, "inftry has no technology capacity");
			Assert.That(ModuleStack.All["000112"].Technologies.Contains("rckter"), Is.True);
			Assert.That(player.TechnologiesToShow.Contains("rckter"), Is.True);
			Assert.That(player.TechnologiesSeen.Contains("rckter"), Is.False);

			TransferOrder second = new TransferOrder(infantry, city, ModuleType.All["inftry"], 1, 0);
			second.Execute(1);
			Contract.All.Evaluate(1);
			Assert.That(Contract.All.Count, Is.EqualTo(0));
			Assert.That(player.TechnologiesToShow.Count, Is.EqualTo(1));
			Assert.That(player.TechnologiesSeen.Count, Is.EqualTo(0));
		}

		[Test]
		public void Evaluate_DoesNothingWhenCountUnchanged()
		{
			this.Publish("CT0001", "R00002", "000001");
			Faction player = Faction.All["2"];

			Contract.All.Evaluate(1);

			Assert.That(Contract.All.Count, Is.EqualTo(1));
			Assert.That(player.TechnologiesSeen.Contains("rckter"), Is.False);
			Assert.That(player.TechnologiesToShow.Contains("rckter"), Is.False);
		}

		[Test]
		public void Transfer_IgnoresIssuerSelfMove()
		{
			this.Publish("CT0001", "R00002", "000001");
			Faction npc = Faction.All["1"];
			ModuleStack infantry = this.CreateInfantry("i99902", npc, Region.All["R00002"], 1);

			TransferOrder order = new TransferOrder(infantry, ModuleStack.All["000001"], ModuleType.All["inftry"], 1, 0);
			order.Execute(1);
			Contract.All.Evaluate(1);

			Assert.That(Contract.All.Count, Is.EqualTo(1));
			Assert.That(npc.TechnologiesSeen.Contains("rckter"), Is.False);
		}

		[Test]
		public void UseFor_SameTypeParentTransfersAndCompletesContract()
		{
			Sequence.Ints.Push(201);
			Sequence.Ints.Push(200);

			ModuleStack factory = ModuleStack.All["000004"];
			factory.Technologies.Add(Technology.All["frminf"]);
			ModuleStack garrison = this.CreateInfantry("g99902", Faction.All["1"], Region.All["R00002"], 1);
			this.Publish("CT0002", "R00002", "g99902");

			UseOrder use = new UseOrder(factory);
			use.Parse("frminf for g99902");
			use.DurationInitial = 1;
			use.Execute(1);

			Assert.That(use.Executed, Is.True);
			Assert.That(garrison.Quantity, Is.EqualTo(2));
			Assert.That(ModuleStack.All.ContainsKey(((ModuleStack)use.Receiver).Name), Is.False);

			Contract.All.Evaluate(1);
			Assert.That(Contract.All.Count, Is.EqualTo(0));
			Assert.That(factory.Technologies.Contains("rckter"), Is.False, "factory is already over technology capacity");
			Assert.That(ModuleStack.All["000112"].Technologies.Contains("rckter"), Is.True, "overflow copy lands on Caste Prime Headquarters");
			Assert.That(garrison.Technologies.Contains("rckter"), Is.False);
			Assert.That(Faction.All["2"].TechnologiesToShow.Contains("rckter"), Is.True);
			Assert.That(Faction.All["2"].TechnologiesSeen.Contains("rckter"), Is.False);
		}

		[Test]
		public void ReceiveTechnologyCopy_WhenStartIsFull_HostsOnSameOwnerCityBeforeNestedHeadquarters()
		{
			Faction owner = Faction.All["2"];
			Region region = Region.All["R10009"];
			ModuleStack city = new ModuleStack(region, owner, ModuleType.All["city"], "c90001");
			city.AddModule();
			ModuleStack headquarters = new ModuleStack(city, owner, ModuleType.All["corphq"], "h90001");
			headquarters.AddModule();
			ModuleStack barracks = new ModuleStack(city, owner, ModuleType.All["barrck"], "b90001");
			barracks.AddModule();
			barracks.Technologies.Add(Technology.All["frminf"]);

			barracks.ReceiveTechnologyCopy(
				Technology.All["rckter"],
				1,
				"received copy of rocket launcher production [rckter] technology.");

			Assert.That(barracks.Technologies.Contains("rckter"), Is.False);
			Assert.That(headquarters.Technologies.Contains("rckter"), Is.False);
			Assert.That(city.Technologies.Contains("rckter"), Is.True);
			Assert.That(owner.TechnologiesToShow.Contains("rckter"), Is.True);
		}

		[Test]
		public void UseFor_DifferentLocationDoesNotDeliver()
		{
			Sequence.Ints.Push(203);
			Sequence.Ints.Push(202);

			ModuleStack factory = ModuleStack.All["000004"];
			factory.Technologies.Add(Technology.All["frminf"]);
			ModuleStack garrison = this.CreateInfantry("g10009", Faction.All["1"], Region.All["R10009"], 1);
			this.Publish("CT1009", "R10009", "g10009");

			UseOrder use = new UseOrder(factory);
			use.Parse("frminf for g10009");
			use.DurationInitial = 1;
			use.Execute(1);

			Assert.That(use.Executed, Is.True);
			Assert.That(garrison.Quantity, Is.EqualTo(1));
			Contract.All.Evaluate(1);
			Assert.That(Contract.All.Count, Is.EqualTo(1));
			Assert.That(factory.Technologies.Contains("rckter"), Is.False);
			Assert.That(ModuleStack.All["000112"].Technologies.Contains("rckter"), Is.False);
			Assert.That(Faction.All["2"].TechnologiesToShow.Contains("rckter"), Is.False);
		}

		[Test]
		public void NoTurn_AppliesContractOrderWithoutAdvancingTurn()
		{
			int turnBefore = this.game.Turn;
			List<string> commands = new List<string>
			{
				"#faction 1",
				"contract R00002 give 1 inftry to 000001 reward rckter technology",
				"#end"
			};
			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(commands);

			this.game.ExecuteBetweenTurnOrders();

			Assert.That(this.game.Turn, Is.EqualTo(turnBefore));
			Assert.That(Contract.All.Count, Is.EqualTo(1));
			Assert.That(Faction.All["1"].Orders.Count, Is.EqualTo(0));
		}

		[Test]
		public void NoTurn_AnnouncesToPresentFaction()
		{
			string turnDir = Directory.GetCurrentDirectory();
			string presentFile = Path.Combine(turnDir, "announce." + this.game.Turn + ".2.txt");
			if (File.Exists(presentFile))
			{
				File.Delete(presentFile);
			}

			Contract open = this.Publish("CT0001", "R00002", "000001");
			open.CreatedThisSession = true;
			Contract.All.WriteAnnouncements(turnDir, this.game);

			Assert.That(File.Exists(presentFile), Is.True);
			string announcement = File.ReadAllText(presentFile, Encoding.GetEncoding(1251));
			Assert.That(announcement.Contains("To: mail@mail.pl"), Is.True);
			Assert.That(announcement.Contains("Subject: [SpaceAge] Report for turn " + this.game.Turn), Is.True);
			Assert.That(announcement.Contains("CT0001"), Is.True);
			Assert.That(announcement.Contains("infantry battalion [inftry]"), Is.True);
			Assert.That(announcement.Contains("rocket launcher production [rckter]"), Is.True);
			File.Delete(presentFile);
		}

		[Test]
		public void NoTurn_SkipsAbsentFaction()
		{
			string turnDir = Directory.GetCurrentDirectory();
			string absentFile = Path.Combine(turnDir, "announce." + this.game.Turn + ".2.txt");
			if (File.Exists(absentFile))
			{
				File.Delete(absentFile);
			}

			ModuleStack remote = this.CreateInfantry("i10009", Faction.All["1"], Region.All["R10009"], 1);
			Contract remoteContract = this.Publish("CT1009", "R10009", remote.Name);
			remoteContract.CreatedThisSession = true;
			Contract.All.WriteAnnouncements(turnDir, this.game);

			Assert.That(File.Exists(absentFile), Is.False);
			string npcFile = Path.Combine(turnDir, "announce." + this.game.Turn + ".1.txt");
			if (File.Exists(npcFile))
			{
				File.Delete(npcFile);
			}
		}
	}
}
