using System.Collections.Generic;
using System.Xml;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TSynchro : TTest
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
		public void AssignSynchroOrder_ReadsTag()
		{
			ModuleStack mover = this.game.ModuleStacks["100001"];
			this.Assign(new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"synchro move_signal",
				"-move R00002",
				"#end"
			});

			Assert.That(mover.Orders.Count, Is.EqualTo(2));
			SynchroOrder synchro = (SynchroOrder)mover.Orders[0];
			MoveOrder move = (MoveOrder)mover.Orders[1];
			Assert.That(synchro.Tag, Is.EqualTo("move_signal"));
			Assert.That(synchro.Type, Is.EqualTo(EOrderType.synchro));
			Assert.That(synchro.Report(mover.Owner)[0], Is.EqualTo("synchro move_signal"));
			Assert.That(move.ConditionalOrders[0], Is.SameAs(synchro));
			Assert.That(move.Report(mover.Owner)[0], Does.StartWith("-move"));
		}

		[Test]
		public void SaveAndLoadXml_KeepsTag()
		{
			ModuleStack mover = this.game.ModuleStacks["100001"];
			this.Assign(new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"synchro move_signal",
				"#end"
			});

			SynchroOrder synchro = (SynchroOrder)mover.Orders[0];
			XmlDocument doc = new XmlDocument();
			XmlElement saved = synchro.SaveXml(doc, "modulestack");

			ModuleStack other = new ModuleStack(mover.Parent, mover.Owner, mover.ModuleType, "synxml");
			SynchroOrder loaded = new SynchroOrder(other);
			loaded.LoadXml(saved);

			Assert.That(loaded.Tag, Is.EqualTo("move_signal"));
		}

		[Test]
		public void Execute_LoneSynchro_DoesNotFire()
		{
			ModuleStack mover = this.game.ModuleStacks["100001"];
			this.Assign(new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"synchro move_signal",
				"-move R00002",
				"#end"
			});

			this.RunWeek();

			Assert.That(mover.Orders.Count, Is.EqualTo(2));
			Assert.That(mover.MovingTo, Is.Null);
			Assert.That(((SynchroOrder)mover.Orders[0]).Executed, Is.False);
		}

		[Test]
		public void Execute_DifferentTags_DoNotRendezvous()
		{
			ModuleStack mover = this.game.ModuleStacks["100001"];
			ModuleStack other = this.Tanks("syncot", 2);
			this.Assign(new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"synchro move_signal",
				"-move R00002",
				"#modulestack syncot",
				"synchro other_signal",
				"#end"
			});

			this.RunWeek();

			Assert.That(mover.MovingTo, Is.Null);
			Assert.That(other.Orders.Count, Is.EqualTo(1));
		}

		[Test]
		public void Execute_MoveOnSignal_WaitsForTwoTanksThenBothFire()
		{
			ModuleStack mover = this.game.ModuleStacks["100001"];
			ModuleStack tanks = this.Tanks("synctk", 1);
			Region destination = Region.All["R00002"];
			this.Assign(new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"synchro move_signal",
				"-move R00002",
				"#modulestack synctk",
				"has 2 tanks",
				"-synchro move_signal",
				"#end"
			});

			this.RunWeek();

			Assert.That(mover.MovingTo, Is.Null);
			Assert.That(mover.Orders.Count, Is.EqualTo(2));
			Assert.That(tanks.Orders.Count, Is.EqualTo(2));

			tanks.AddModule();
			this.RunWeek();

			Assert.That(mover.MovingTo, Is.EqualTo(destination));
			Assert.That(this.HasSynchro(mover, "move_signal"), Is.False);
			Assert.That(this.HasSynchro(tanks, "move_signal"), Is.False);
			Assert.That(tanks.Orders.Count, Is.EqualTo(0));
			Assert.That(this.HasEvent(mover, "synchronized move_signal."), Is.True);
			Assert.That(this.HasEvent(tanks, "synchronized move_signal."), Is.True);
		}

		[Test]
		public void Execute_ThirdCopy_HoldsTheSignalUntilItIsReady()
		{
			ModuleStack mover = this.game.ModuleStacks["100001"];
			ModuleStack ready = this.Tanks("syncrd", 1);
			ModuleStack waiting = this.Tanks("syncwt", 1);
			this.Assign(new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"synchro move_signal",
				"-alias \"gone\"",
				"#modulestack syncrd",
				"synchro move_signal",
				"#modulestack syncwt",
				"has 2 tanks",
				"-synchro move_signal",
				"#end"
			});

			this.RunWeek();

			Assert.That(mover.Alias, Is.Not.EqualTo("2_gone"));
			Assert.That(this.HasSynchro(mover, "move_signal"), Is.True);

			waiting.AddModule();
			this.RunWeek();

			Assert.That(mover.Alias, Is.EqualTo("2_gone"));
			Assert.That(this.HasSynchro(mover, "move_signal"), Is.False);
			Assert.That(this.HasSynchro(ready, "move_signal"), Is.False);
			Assert.That(this.HasSynchro(waiting, "move_signal"), Is.False);
		}

		[Test]
		public void Execute_CrossFactionSignals_GiveThenReleaseSecondOrder()
		{
			ModuleStack mover = this.game.ModuleStacks["100001"];
			Faction otherFaction = this.game.Factions["1"];
			Region region = (Region)mover.Parent;
			ModuleStack giver = new ModuleStack(region, otherFaction, ModuleType.All["cargob"], "syncg1");
			giver.AddModule();
			giver.ItemStacks.Add(new ItemStack(ItemType.All["iron"], 5));
			ModuleStack receiver = new ModuleStack(region, otherFaction, ModuleType.All["cargob"], "syncr1");
			receiver.AddModule();

			this.Assign(new List<string>
			{
				"#faction 2",
				"#modulestack 100001",
				"move R00002",
				"+synchro arrived",
				"synchro given",
				"-alias \"handed\"",
				"#faction 1",
				"#modulestack syncg1",
				"synchro arrived",
				"-give 2 iron to syncr1",
				"--synchro given",
				"#end"
			});

			this.RunWeek();

			Assert.That(mover.MovingTo, Is.EqualTo(Region.All["R00002"]));
			Assert.That(mover.Alias, Is.EqualTo("2_handed"));
			Assert.That(receiver.ItemStacks[ItemType.All["iron"]].Quantity, Is.EqualTo(2));
			Assert.That(giver.ItemStacks[ItemType.All["iron"]].Quantity, Is.EqualTo(3));
			Assert.That(this.HasSynchro(mover, "arrived"), Is.False);
			Assert.That(this.HasSynchro(mover, "given"), Is.False);
			Assert.That(this.HasSynchro(giver, "arrived"), Is.False);
			Assert.That(this.HasSynchro(giver, "given"), Is.False);
		}

		private ModuleStack Tanks(string name, int quantity)
		{
			ModuleStack tanks = new ModuleStack(
				this.game.ModuleStacks["100001"].Parent,
				this.game.Factions["2"],
				ModuleType.All["tanks"],
				name);
			tanks.AddModules(quantity);
			return tanks;
		}

		private void Assign(List<string> commands)
		{
			OrdersReader ordersReader = new OrdersReader(this.game);
			ordersReader.AssignOrders(commands);
		}

		private void RunWeek()
		{
			this.game.ClearExecutedLongOrder();
			this.game.ClearExecutedImmediateOrders();
			this.game.ExecuteOrders();
		}

		private bool HasSynchro(ModuleStack stack, string tag)
		{
			foreach (Order order in stack.Orders)
			{
				SynchroOrder synchro = order as SynchroOrder;
				if (synchro != null && string.Equals(synchro.Tag, tag, System.StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		private bool HasEvent(ModuleStack stack, string description)
		{
			foreach (EventReport report in stack.EventReports)
			{
				if (report.Description == description)
				{
					return true;
				}
			}
			return false;
		}
	}
}
