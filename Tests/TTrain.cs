using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TTrain : TTest
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
		public void AssignTrain_officer()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000112"];
            Assert.That(testModuleStack.FullName, Is.EqualTo("Caste Prime Headquarters"));

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000112",
                "train terran officer as new3",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is TrainOrder);
			TrainOrder trainOrder = (TrainOrder)testModuleStack.Orders[0];
		}


		[Test]
		public void ExecuteTrain_officer()
		{
			Sequence.Ints.Push(100);

			this.AssignTrain_officer();

			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000112"];
			TrainOrder order = (TrainOrder)testModuleStack.Orders[0];
			ModuleType corphq = ModuleType.All["corphq"];
			ItemType terran = ItemType.All["terran"];

            // check if the world state changes correctly	
            Assert.That(testModuleStack.Effects.IsProducing, Is.False);
            Assert.That(testModuleStack.ItemStacks[terran].Quantity, Is.EqualTo(20));
            Assert.That(testModuleStack.People.Count, Is.EqualTo(1));
            Assert.That(order.Race.OfficerTrainingDuration, Is.EqualTo(6));
            Assert.That(order.Repeat, Is.EqualTo(1));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False);

			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Orders[0].Execute(this.game.Week + 0);
			testModuleStack.Orders.RemoveExecuted();
			testModuleStack.Effects.Execute(this.game.Week + 0);
			testModuleStack.Effects.RemoveExecuted();

            Assert.That(testModuleStack.Effects.IsTraining, "Should be training  - it's a 6 weeks duration order for terran officers");
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.DurationLeft, Is.EqualTo(5));
            Assert.That(order.Repeat, Is.EqualTo(1));
            Assert.That(order.Executing);
            Assert.That(order.Executed, Is.False);

			for (int i = 0; i < 5; i++)
			{
				testModuleStack.ExecutedLongOrder = false;
				testModuleStack.Orders[0].Execute(this.game.Week + i);
				testModuleStack.Orders.RemoveExecuted();
				testModuleStack.Effects.Execute(this.game.Week + i);
				testModuleStack.Effects.RemoveExecuted();
			}

            Assert.That(testModuleStack.Effects.IsTraining, Is.False, "Shouldn't be training - it's a 6 weeks duration order for terran officers");
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(0));
            //ClassicAssert.AreEqual(0, order.DurationLeft);
            Assert.That(order.Repeat, Is.EqualTo(0));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed);

            Assert.That(testModuleStack.ItemStacks[terran].Quantity, Is.EqualTo(20 - 1));
            Assert.That(testModuleStack.People.Count, Is.EqualTo(2));

			foreach (Person person in testModuleStack.People.Values)
			{
				Console.WriteLine(person.ReportName);
			}

			Person trainedOfficer = Person.All["2_new3"];
            Assert.That(trainedOfficer, Is.Not.Null);
            Assert.That(trainedOfficer.Name, Is.EqualTo("100"));
            Assert.That(trainedOfficer.Parent, Is.EqualTo(testModuleStack));
		}


		[Test]
		public void AssignTrain_skill()
		{
			Faction testFaction = this.game.Factions["2"];
			Person testPerson = this.game.People["000101"];
            Assert.That(testPerson.FullName, Is.EqualTo("Caste Prime CEO"));

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#person 000101",
                "train skill arpldr",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testPerson.Orders.Count, Is.EqualTo(1));

            Assert.That(testPerson.Orders[0] is TrainOrder);
			TrainOrder trainOrder = (TrainOrder)testPerson.Orders[0];
            Assert.That(trainOrder.TrainingOfficer, Is.False);
		}


		[Test]
		public void ExecuteTrain_skill()
		{
			this.AssignTrain_skill();
			Faction testFaction = this.game.Factions["2"];
			Person testPerson = this.game.People["000101"];
			TrainOrder order = (TrainOrder)testPerson.Orders[0];
			SkillType skill = SkillType.All["arpldr"];

            // check if the world state changes correctly	
            Assert.That(testPerson.Skills.ContainsKey(skill), Is.False);
            Assert.That(testPerson.Effects.IsProducing, Is.False);
            Assert.That(order.SkillType.TrainingDuration, Is.EqualTo(4));
            Assert.That(order.Repeat, Is.EqualTo(1));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False);

			testPerson.ExecutedLongOrder = false;
			testPerson.Orders[0].Execute(this.game.Week + 0);
			testPerson.Orders.RemoveExecuted();
			testPerson.Effects.Execute(this.game.Week + 0);
			testPerson.Effects.RemoveExecuted();

            Assert.That(testPerson.Effects.IsTraining, "Should be training - it's a 4 weeks duration order for terran officers");
            Assert.That(testPerson.Orders.Count, Is.EqualTo(1));
            Assert.That(order.DurationLeft, Is.EqualTo(3));
            Assert.That(order.Repeat, Is.EqualTo(1));
            Assert.That(order.Executing);
            Assert.That(order.Executed, Is.False);

            Assert.That(testPerson.Skills.ContainsKey(skill), Is.False, "shouldn't have skill yet");

			for (int i = 0; i < 3; i++)
			{
				testPerson.ExecutedLongOrder = false;
				testPerson.Orders[0].Execute(this.game.Week + i);
				testPerson.Orders.RemoveExecuted();
				testPerson.Effects.Execute(this.game.Week + i);
				testPerson.Effects.RemoveExecuted();
			}

            Assert.That(testPerson.Effects.IsTraining, Is.False, "Shouldn't be training - it's a 4 weeks duration order for terran officers");
            Assert.That(testPerson.Orders.Count, Is.EqualTo(0));
            //ClassicAssert.AreEqual(0, order.DurationLeft);
            Assert.That(order.Repeat, Is.EqualTo(0));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed);

            Assert.That(testPerson.Skills.ContainsKey(skill), "should have skill now");

            this.consoleOutReport("trainign progress", testPerson, testFaction);
		}


		[Test]
		public void TrainOfficer_stacking()
		{
			//+ Gelvaren complex [000026], city [city], disabled, immobile.
			//	events:
			//	+ terran officer [108], terran [terran].
			//		events:
			//			week 1: started training of terran [terran] officer.
			//			week 6: trained by Gelvaren Headquarters [000018].
			//			week 6: trained by Gelvaren Headquarters [000018].
			//			week 6: stacked under Gelvaren complex [000026].
			//			week 6: stacked under Gelvaren complex [000026].
			//			week 6: stacked under Gelvaren complex [000026].
			//			week 6: stacked under Gelvaren complex [000026].
			//			week 6: stacked under Gelvaren complex [000026].
			//			week 6: stacked under Gelvaren complex [000026].
			//	+ Gelvaren Headquarters [000018], corporate headquarters [corphq],
			//		disabled.
			//		events:
			//			week 6: trained terran [terran] into terran officer [108].
			//			week 6: trained terran [terran] into terran officer [108].

			//Assert.Fail("when training officer by module stacked under module, officer should stack under training module not his parent");

			this.ExecuteTrain_officer();

			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000112"];
			Person trainedOfficer = Person.All["2_new3"];

            Assert.That(trainedOfficer.Parent, Is.EqualTo(testModuleStack));

			//manual verification if there are no double event report
			this.consoleOutReport("trainer:", testModuleStack, testFaction);
			this.consoleOutReport("trainee:", trainedOfficer, testFaction);
		}


		[Test]
		public void TrainOfficer_byDisabled()
		{
			//	+ Gelvaren Headquarters [000018], corporate headquarters [corphq],
			//		disabled.
			//		events:
			//			week 6: trained terran [terran] into terran officer [108].
			
			//Assert.Fail("training officer should fail, if there is not enough energy");

			Sequence.Ints.Push(100);

			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000112"];
            Assert.That(testModuleStack.FullName, Is.EqualTo("Caste Prime Headquarters"));

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000112",
                "train terran officer as new3",
                "#end"
            };

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is TrainOrder);
			TrainOrder trainOrder = (TrainOrder)testModuleStack.Orders[0];
			ItemType terran = ItemType.All["terran"];

			testModuleStack.ItemStacks[terran].Quantity = 10;
            Assert.That(testModuleStack.IsActive, Is.False);

			TrainOrder order = (TrainOrder)testModuleStack.Orders[0];

            // check if the world state changes correctly	
            Assert.That(testModuleStack.Effects.IsProducing, Is.False);
            Assert.That(testModuleStack.People.Count, Is.EqualTo(1));
            Assert.That(order.Repeat, Is.EqualTo(1));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False);

			testModuleStack.ExecutedLongOrder = false;
			testModuleStack.Orders[0].Execute(this.game.Week + 0);
			testModuleStack.Orders.RemoveExecuted();
			testModuleStack.Effects.Execute(this.game.Week + 0);
			testModuleStack.Effects.RemoveExecuted();

            Assert.That(testModuleStack.Effects.IsTraining, Is.False, "Shouldn't be training - it's a disabled module");
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1), "order failed due to lack of energy shouldn't be treated as executed");
            Assert.That(order.Repeat, Is.EqualTo(1), "order failed due to lack of energy shouldn't be treated as executed");
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False);

            Assert.That(testModuleStack.People.Count, Is.EqualTo(1));

			this.consoleOutReport("trainer: ", testModuleStack, testFaction);
			//this.consoleOutReport("trainee: ", this.game.People["100"], testFaction);
		}

		[Test]
		public void AssignOrders_LeftoverTrain_ReconnectsDurationAfterSaveLoad()
		{
			this.AssignTrain_skill();
			Person trainee = this.game.People["000101"];
			TrainOrder order = (TrainOrder)trainee.Orders[0];
			this.executeOrder(trainee, order, 0);
			Assert.That(order.DurationLeft, Is.EqualTo(3));
			Assert.That(trainee.Effects.IsTraining);

			string testdir = Directory.GetCurrentDirectory();
			string testfile = "gameout.saved_trainLeftover.xml";
			this.dataFile.SaveGame(testdir, testfile);
			this.ClearGame();

			this.dataFile = new DataFile(testdir);
			this.dataFile.LoadGameDocument(testdir, testfile);
			this.dataFile.LoadConfiguration(testdir);
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.dataFile.LoadOrders();
			this.game = this.dataFile.Game;

			trainee = Person.All["000101"];
			Assert.That(trainee.Effects.IsTraining);
			Assert.That(trainee.Orders.Count, Is.EqualTo(1));

			new OrdersReader(this.game).AssignOrders(new List<string>
			{
				"#faction 2",
				"#person 000101",
				"train skill arpldr",
				"#end"
			});

			Assert.That(trainee.Orders.Count, Is.EqualTo(1), "reissued TRAIN must keep leftover, not duplicate");
			order = (TrainOrder)trainee.Orders[0];
			this.executeOrder(trainee, order, 1);

			Assert.That(order.DurationLeft, Is.EqualTo(2));
			Assert.That(trainee.Effects.IsTraining);
			Assert.That(trainee.Skills.ContainsKey(SkillType.All["arpldr"]), Is.False);
		}

	}
}
