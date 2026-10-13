using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TMovePerson : TTest
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
		public void Report_PersonMove_DoesNotThrow()
		{
			Person person = this.CreateTerranOnRegion("R00001", "990101");
			this.AssignPersonMove(person, "R00002");

			MoveOrder order = (MoveOrder)person.Orders[0];
			ClassicAssert.DoesNotThrow(() => order.Report(this.game.Factions["2"]));
		}

		[Test]
		public void ExecutePersonMove_MatchesInfantryGroundDuration()
		{
			Region region1 = Region.All["R00001"];
			Region region2 = Region.All["R00002"];
			Faction faction = this.game.Factions["2"];

			ModuleStack infantry = new ModuleStack(region1, faction, ModuleType.All["inftry"], "888001");
			infantry.AddModules(1);
			infantry.ItemStacks.Add(new ItemStack(ItemType.All["terran"], 26));

			Person person = this.CreateTerranOnRegion("R00001", "888002");

			this.AssignMove(infantry, "R00002");
			this.AssignPersonMove(person, "R00002");

			MoveOrder infantryOrder = (MoveOrder)infantry.Orders[0];
			MoveOrder personOrder = (MoveOrder)person.Orders[0];

			infantry.ExecutedLongOrder = false;
			infantry.Orders.Execute(this.game.Week);
			person.ExecutedLongOrder = false;
			person.Orders.Execute(this.game.Week);

			ClassicAssert.AreEqual(infantryOrder.DurationLeft, personOrder.DurationLeft);
			ClassicAssert.AreEqual(region2, infantry.MovingTo);
			ClassicAssert.AreEqual(region2, person.MovingTo);
		}

		[Test]
		public void ExecutePersonMove_NavalOnlyExit_ReportsSeaFailure()
		{
			this.LoadNavalWorld();
			Person person = this.CreateTerranOnRegion("R00001", "990201");
			this.AssignPersonMove(person, "R00002");

			person.ExecutedLongOrder = false;
			person.Orders.Execute(this.game.Week);

			ClassicAssert.IsNull(person.MovingTo);
			ClassicAssert.IsTrue(this.HasEventContaining(person, "movable by sea"));
		}

		[Test]
		public void ExecutePersonMove_OrbitDestination_ReportsSpaceFailure()
		{
			Person person = this.CreateTerranOnRegion("R00001", "990301");
			this.AssignPersonMove(person, "O00005");

			person.ExecutedLongOrder = false;
			person.Orders.Execute(this.game.Week);

			ClassicAssert.IsNull(person.MovingTo);
			ClassicAssert.IsTrue(this.HasEventContaining(person, "move in space"));
		}

		private Person CreateTerranOnRegion(string regionId, string personId)
		{
			Region region = Region.All[regionId];
			Faction faction = this.game.Factions["2"];
			return new Person(region, faction, Race.All["terran"], personId);
		}

		private void AssignPersonMove(Person person, string destination)
		{
			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(new List<string>
			{
				"#faction " + person.Owner.Name,
				"#person " + person.Name,
				"move " + destination,
				"#end"
			});
		}

		private void AssignMove(ModuleStack stack, string destination)
		{
			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(new List<string>
			{
				"#faction " + stack.Owner.Name,
				"#modulestack " + stack.Name,
				"move " + destination,
				"#end"
			});
		}

		private bool HasEventContaining(Person person, string fragment)
		{
			foreach (EventReport report in person.EventReports)
			{
				if (report.Description.IndexOf(fragment, System.StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}
			}
			return false;
		}

		private void LoadNavalWorld()
		{
			this.ClearGame();
			string fixtureDir = Path.Combine(Directory.GetCurrentDirectory(), "fixtures", "naval");
			this.dataFile = new DataFile(fixtureDir);
			this.dataFile.LoadGameDocument(fixtureDir, "gamein.xml");
			this.dataFile.LoadConfiguration(Directory.GetCurrentDirectory());
			this.LoadExtraCatalog(Path.Combine(fixtureDir, "data-naval-module.xml"));
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;
		}

		private void LoadExtraCatalog(string path)
		{
			System.Xml.XmlDocument extra = new System.Xml.XmlDocument();
			extra.Load(path);
			CatalogLoader loader = new CatalogLoader(this.dataFile);
			loader.LoadItems(extra, this.dataFile.Game, true);
			loader.LoadItems(extra, this.dataFile.Game, false);
		}
	}
}
