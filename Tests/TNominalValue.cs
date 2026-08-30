using System.IO;
using System.Xml;
using NUnit.Framework;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TNominalValue : TTest
	{
		[SetUp]
		public void setup()
		{
			this.dataFile = new DataFile(Directory.GetCurrentDirectory());
			this.game = new Game();
		}

		[TearDown]
		public void teardown()
		{
			this.ClearGame();
		}

		[Test]
		public void LoadConfiguration_ReadsItemNominalValue()
		{
			this.dataFile.LoadConfiguration(Directory.GetCurrentDirectory());
			this.ReloadNominalCatalog();

			Assert.That(ItemType.All["food"].NominalValue, Is.EqualTo(2));
			Assert.That(ItemType.All["iron"].NominalValue, Is.EqualTo(3));
			Assert.That(ItemType.All["ztest"].NominalValue, Is.EqualTo(0));
		}

		[Test]
		public void GetPrice_FallsBackToNominalWhenNoHistory()
		{
			this.LoadNominalWorld();
			Market market = Region.All["R00001"].Market;

			Assert.That(market.GetPrice(ItemType.All["food"]), Is.EqualTo(2));
			Assert.That(market.GetPrice(ItemType.All["iron"]), Is.EqualTo(3));
			Assert.That(market.GetPrice(ItemType.All["ztest"]), Is.EqualTo(0));
		}

		[Test]
		public void GetPrice_RegionalAverageBeatsNominal()
		{
			this.LoadNominalWorld();
			Region.All["R00002"].Market.AddPrice(ItemType.All["food"], 7);
			Market market = Region.All["R00001"].Market;

			Assert.That(market.GetPrice(ItemType.All["food"]), Is.EqualTo(7));
		}

		[Test]
		public void GenerateOffers_UsesNominalForNewListing()
		{
			this.LoadNominalWorld();
			ModuleStack city = ModuleStack.All["un001"];

			this.game.GenerateOffers();

			Offer ironOffer = this.game.Offers[EOfferType.SellItems][ItemType.All["iron"]][city].GetIndex(0);
			Assert.That(ironOffer.Quantity, Is.EqualTo(10));
			Assert.That(ironOffer.Price, Is.EqualTo(3));
		}

		private string FixtureDir()
		{
			return Path.Combine(Directory.GetCurrentDirectory(), "fixtures", "nominal-value");
		}

		private void ReloadNominalCatalog()
		{
			XmlDocument extra = new XmlDocument();
			extra.Load(Path.Combine(this.FixtureDir(), "data-nominal-items.xml"));
			CatalogLoader loader = new CatalogLoader(this.dataFile);
			loader.LoadItems(extra, this.dataFile.Game, true);
			loader.LoadItems(extra, this.dataFile.Game, false);
		}

		private void LoadNominalWorld()
		{
			string fixtureDir = this.FixtureDir();
			this.dataFile = new DataFile(fixtureDir);
			this.dataFile.LoadGameDocument(fixtureDir, "gamein.xml");
			this.dataFile.LoadConfiguration(Directory.GetCurrentDirectory());
			this.ReloadNominalCatalog();
			this.dataFile.LoadFactions();
			this.dataFile.LoadGalaxy();
			this.game = this.dataFile.Game;
		}
	}
}
