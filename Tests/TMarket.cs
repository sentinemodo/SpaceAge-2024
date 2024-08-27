using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TMarket : TTest
	{
		public TMarket()
		{			
		}
			
		[SetUp]
		public void SetupOrder()
		{
			this.dataFile = new DataFile(Directory.GetCurrentDirectory());
			this.dataFile.LoadConfiguration();
			this.dataFile.LoadGame();
			this.game = this.dataFile.Game;
		}

		[TearDown]
		public void TeardownOrder()
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


        [Test]
        public void FindMatchLocal()
        {
            // buy 200 food
            // assert that buys sell orders are in place standing
            // berlin [000005] wants to buy 200 food
            // berlin farms [000008] wants to sell 40 food
            // TODO: this shouldn't be happening, either city or farms are greater need/output not both
            Region region = this.game.Regions["R00001"];
            Market market = region.Market;

            ItemType food = ItemType.All["food"];
            ItemType cash = ItemType.All["cash"];

            ModuleStack buyer = this.game.ModuleStacks["000005"];
            ModuleStack seller = this.game.ModuleStacks["000008"];

            Offer buyOffer = this.game.Offers[EOfferType.BuyItems][food][buyer].GetIndex(0);
            Assert.That(buyOffer.Quantity, Is.EqualTo(200));

            Offer sellOffer = this.game.Offers[EOfferType.SellItems][food][seller].GetIndex(0);
            Assert.That(sellOffer.Quantity, Is.EqualTo(40));

            Offer foundOffer = market.FindMatch(buyOffer);
            ClassicAssert.IsNull(foundOffer);

            seller.Owner = Faction.All["2"];
            foundOffer = market.FindMatch(buyOffer);
            // can work only if buyer and seller are from different factions
            Assert.That(foundOffer, Is.EqualTo(sellOffer));
        }

        [Test]
        public void ProcessBuySellItemStack()
        {
            // buy 200 food
            // assert that buys sell orders are in place standing
            // berlin [000005] wants to buy 200 food
            // berlin farms [000008] wants to sell 40 food
            // note: this shouldn't be happening, either city or farms are greater need/output not both
			Region region = this.game.Regions["R00001"];
			Market market = region.Market;

            ItemType food = ItemType.All["food"];
            ItemType cash = ItemType.All["cash"];

            ModuleStack buyer = this.game.ModuleStacks["000005"];
            ModuleStack seller = this.game.ModuleStacks["000008"];

            // changing owner faction so the findMatch will find the seller offer
            seller.Owner = Faction.All["2"];

            // assert existing offers
            this.consoleOutReport("Existing offers", Offer.All, buyer.Owner);
            Assert.That(Offer.All.Count, Is.EqualTo(9));

            Assert.That(seller.ItemStacks[food].Quantity, Is.EqualTo(40));

            Offer buyOffer = this.game.Offers[EOfferType.BuyItems][food][buyer].GetIndex(0);
            Assert.That(buyOffer.Quantity, Is.EqualTo(200));

            Offer sellOffer = this.game.Offers[EOfferType.SellItems][food][seller].GetIndex(0);
            Assert.That(sellOffer.Quantity, Is.EqualTo(40));

            Assert.That(market.PriceList.ContainsKey(food), Is.False, "without prior transaction, pricelist should be empty");
            Assert.That(market.GetPrice(food), Is.EqualTo(0), "with getPrice initiated and no other prices on other markets, price should be set to 0 - any price");

            // market.process
            // assert orders are matched and done
            // assert matched orders are removed
            this.game.Week = 1;

            market.ProcessOffer(this.game.Week, buyOffer);
            Assert.That(buyOffer.Quantity, Is.EqualTo(160));

            Assert.That(this.game.Offers[EOfferType.SellItems][food][seller].Count, Is.EqualTo(0));
            
            // assert items transfers is taking place
            ReceivingItems foodTransfer = (ReceivingItems)buyer.Effects[0];
            Assert.That(foodTransfer.ItemStack.Quantity, Is.EqualTo(40));
            Assert.That(foodTransfer.ItemStack.ItemType, Is.EqualTo(food));

            this.consoleOutReport("food transfer event", foodTransfer, buyer.Owner);

            foodTransfer.Execute(this.game.Week);

            // received
            Assert.That(buyer.ItemStacks[food].Quantity, Is.EqualTo(40));

            // assert prices get updated accordingly
            Assert.That(market.PriceList[food], Is.EqualTo(2));
        }

        [Test]
        public void ProcessBuySellItemStackUnlimited()
        {
            // @buy all terran everywhere
            // berlin [000005] wants to sell 20 terrans

            // this should work like this.
            // 1. buy order starts to execute, an offer appear
            // 2. if the order is fullfilled (if it can be fullfilled) it is removed and the 
            //    matching offer is also removed
            // 3. if the order cannot be fullfilled the offer and order stays
            // 4. when the offer is fullfilled at some later point, the order is also removed
            // 5. if the order is removed (like the time has passed, or the order was set only 
            //    for a given week, or the turn has ended and the order didn't appear in next 
            //    turn set of orders) the offer is removed as well
            // 6. if the buy order matched and some sell order was fullfilled both the offer
            //    and the sell order are removed
            Region region = this.game.Regions["R00001"];
            Market market = region.Market;

            ItemType terran = ItemType.All["terran"];
            ItemType cash = ItemType.All["cash"];

            ModuleStack seller1 = this.game.ModuleStacks["000001"]; // note: this is in location of buyer's
            ModuleStack seller2 = this.game.ModuleStacks["000005"]; // note: this is in location other than buyer's
            ModuleStack buyer = this.game.ModuleStacks["000004"];

            Faction buyerFaction = this.game.Factions["2"];

            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "@buy all terran everywhere",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            // assert existing offers
            this.consoleOutReport("Existing offers", Offer.All, buyerFaction);
            Assert.That(Offer.All.Count, Is.EqualTo(9));

            // assert buyer and seller are existing and in a single place
            this.consoleOutReport("Location of buy/seller1", buyer.Location.Market, buyerFaction);
            this.consoleOutReport("Location of seller2", seller2.Location.Market, buyerFaction);

            Assert.That(seller1.ItemStacks[terran].Quantity, Is.EqualTo(10));
            Assert.That(seller2.ItemStacks[terran].Quantity, Is.EqualTo(20));

            // assert that buyer has cash
            Assert.That(buyer.ItemStacks[cash].Quantity, Is.EqualTo(300));
            // 300 cash would suffice for 6 terrans at 50 for cash, but we're using bank account anyway

            #region buyer.Execute(this.game.Week);
            BuyOrder order = (BuyOrder)buyer.Orders[0];
            Assert.That(order.AllQuantity);
            Assert.That(order.Repeat < 0);

            order.Execute(this.game.Week);
            buyer.Orders.RemoveExecuted();
            #endregion

            // assert existing offers
            this.consoleOutReport("Existing offers", Offer.All, buyerFaction);
            Assert.That(seller1.ItemStacks.ContainsKey(terran), Is.False);
            Assert.That(seller2.ItemStacks.ContainsKey(terran), Is.False);

            // there are still only 20 terrans in place (10 already received), the rest is in effects (transferring)
            Assert.That(buyer.ItemStacks[terran].Quantity, Is.EqualTo(20));

            // but the cash is already gone 10 local, 20 other region, transferCost for different regions transaction
            Assert.That(buyer.Owner.Bank.AvailableFunds, Is.EqualTo(20000 - 10 * 50 - 20 * 50 - 100));

            Assert.That(buyer.Effects.Count, Is.EqualTo(2));

            // first transfers reached 
            Assert.That(buyer.ItemStacks[terran].Quantity, Is.EqualTo(20));
            // second transfer has still 5 week of duration
            Receiving receivingEffect = (Receiving)buyer.Effects[1];
            Assert.That(receivingEffect.Duration, Is.EqualTo(5));

            // assert remaining offers
            this.consoleOutReport("Location of buyer/seller1", buyer.Location.Market, buyerFaction);
            this.consoleOutReport("Location of seller2", seller2.Location.Market, buyerFaction);
            Assert.That(Offer.All.Count, Is.EqualTo(8)); // two sell offers done, one new buy offer in place

            // TODO: this will require further work to remove possibility to add offers without orders, as of now it works the both ways, which can be useful
            // assert orders are matched and done
            // assert matched orders are removed

            #region buyer.Execute(this.game.Week) - continued2;
            Assert.That(order.Executed);
            Assert.That(buyer.Orders.Count, Is.EqualTo(1));
            buyer.Orders.RemoveExecuted();
            Assert.That(buyer.Orders.Count, Is.EqualTo(1)); // stays the same, because the order is unlimited
            #endregion

            // assert prices get updated accordingly
            Assert.That(buyer.Location.Market.PriceList.ContainsKey(terran));
            Assert.That(seller2.Location.Market.PriceList.ContainsKey(terran));
            Assert.That(buyer.Location.Market.GetPrice(terran), Is.EqualTo(50));
        }

        [Test]
        public void ProcessBuySellModulestack()
        {
            Sequence.Ints.Push(100);
            Sequence.Ints.Push(101);
            Sequence.Ints.Push(102);

            // buy 1 windplant
            // assert that buys sell orders are in place standing
            // market.process
            // assert orders are matched and done
            // assert matched orders are removed
            // assert prices get updated accordingly
            Region region = this.game.Regions["R00001"];
            Market market = region.Market;

            ModuleType windplant = ModuleType.All["wnplnt"];
            ItemType cash = ItemType.All["cash"];

            ModuleStack buyer = this.game.ModuleStacks["000009"];
            ModuleStack seller = this.game.ModuleStacks["000004"];

            // assert existing offers, there should be 9 offers and no windplants offers
            this.consoleOutReport("Existing offers", Offer.All, buyer.Owner);
            Assert.That(Offer.All.Count, Is.EqualTo(9));

            // there should be 10 existing windplants in seller and buyer shouldn't (yet) have any windplants for sale
            Assert.That(buyer.Modules.Count, Is.EqualTo(10));
            Assert.That(seller.HasModuleStacks(windplant), Is.False);

            // setup sell order
            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "sell all wnplnt",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            // execute order - should fail, because seller has no windplants
            seller.Orders[0].Execute(this.game.Week);
            Assert.That(seller.Orders[0].Executed, Is.False);

            // add the windplant and execute again, should go, but no sell done, since there is no matching order/offer
            ModuleStack windplantStack = new ModuleStack(seller, seller.Owner, windplant);
            windplantStack.AddModule();
            this.consoleOutReport("seller", seller, seller.Owner);
            Assert.That(seller.HasModuleStacks(windplant));
            seller.Orders[0].Execute(this.game.Week + 1);

            // check if an offer appeared
            Offer sellOffer = this.game.Offers[EOfferType.SellModules][windplant][seller].GetIndex(0);
            Assert.That(sellOffer.AllQuantity);
            Assert.That(seller.Orders[0].Executed, Is.False);

            // setup buy order
            testcommands = new List<string>
            {
                "#faction 1",
                "#modulestack 000009",
                "buy 1 wnplnt",
                "#end"
            };

            ordersReader.AssignOrders(testcommands);

            // execute order - should go through - but there is not enough cash, so it should reach the bank account
            buyer.Orders[0].Execute(this.game.Week + 1);
            this.consoleOutReport("Buyer", buyer, buyer.Owner);
            this.consoleOutReport("Seller", seller, seller.Owner);

            Assert.That(
                buyer.EventReports[0].Report(buyer.Owner)[0],
                Is.EqualTo("week 2: received wind powerplant [wnplnt] from factory [000004]."));
            Assert.That(
                seller.EventReports[0].Report(seller.Owner)[0],
                Is.EqualTo("week 2: transferred wind powerplant [wnplnt] to Warsaw wind powerplants [000009]."));

            // check if both offers are no longer on the market, and that both orders are marked as executed          
            Assert.That(this.game.Offers[EOfferType.BuyModules][windplant][buyer].Count, Is.EqualTo(0));
            Assert.That(this.game.Offers[EOfferType.SellModules][windplant][seller].Count, Is.EqualTo(0));

            Assert.That(seller.Orders[0].Executed);
            Assert.That(buyer.Orders[0].Executed);

            // check if pricelist got updated
            Assert.That(market.PriceList.ContainsKey(windplant), Is.False, "without prior transaction, pricelist should be empty");
            Assert.That(market.GetPrice(windplant), Is.EqualTo(-1), "with getPrice initiated and no other prices on other markets, price should be set to 0 - any price");

            // received, factory again has no modulestack
            Assert.That(buyer.Modules.Count, Is.EqualTo(11));
            Assert.That(seller.HasModuleStacks(windplant), Is.False);
        }

        [Test]
        public void ProcessBuySellTechnology()
        {
            // sell ctypln
            // assert that buys sell orders are in place standing
            // market.process
            // assert orders are matched and done
            // assert matched orders are removed
            // assert prices get updated accordingly
            Region region = this.game.Regions["R00002"];
            Market market = region.Market;

            Technology cityPlanning = Technology.All["ctypln"];

            ModuleStack seller = this.game.ModuleStacks["000112"]; 
            ModuleStack buyer1 = this.game.ModuleStacks["000001"]; // same region
            ModuleStack buyer2 = this.game.ModuleStacks["000005"]; // other region and too low price
            
            // assert existing offers, there should be 9 offers and no windplants offers
            this.consoleOutReport("Existing offers", Offer.All, seller.Owner);
            Assert.That(Offer.All.Count, Is.EqualTo(9));

            // there shouldn't be technology in seller nor in buyers 
            Assert.That(buyer1.Technologies.Count, Is.EqualTo(0));
            Assert.That(buyer2.Technologies.Count, Is.EqualTo(0));
            Assert.That(seller.Technologies.Count, Is.EqualTo(0));
            Assert.That(seller.HasTechnology(cityPlanning), Is.False);

            // setup sell order
            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000112",
                "sell ctypln at 500",
                "#end"
            };

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            // execute order - should fail, because seller has no technology
            seller.Orders[0].Execute(this.game.Week);
            Assert.That(seller.Orders[0].Executed, Is.False);

            // add the technology and execute again, should go, but no sell done, since there is no matching order/offer
            // the existing one is too low and in bad region
            seller.Technologies.Add(cityPlanning);
            this.consoleOutReport("seller", seller, seller.Owner);
            this.consoleOutReport("seller orders", seller.Orders, seller.Owner);
            Assert.That(seller.HasTechnology(cityPlanning));
            seller.Orders[0].Execute(this.game.Week + 1);

            // check if an offer appeared
            this.consoleOutReport("Existing offers", Offer.All, seller.Owner);
            Offer sellOffer = this.game.Offers[EOfferType.SellTechnologies][cityPlanning][seller].GetIndex(0);
            Assert.That(sellOffer.Technology.Name, Is.EqualTo(cityPlanning.Name));
            Assert.That(seller.Orders[0].Executed, Is.False);

            // change the external buyer to increase the price
            Offer buyer2Offer = this.game.Offers[EOfferType.BuyTechnologies][cityPlanning][buyer2].GetIndex(0);
            buyer2Offer.Price = 600;

            // execute again, should go, but no sell done, since there is no matching order/offer
            // the existing one is in a bad region 
            seller.Orders[0].Execute(this.game.Week + 2);

            // setup buy order
            testcommands = new List<string>
            {
                "#faction 1",
                "#modulestack 000001",
                "buy ctypln at 525",
                "#end"
            };

            ordersReader.AssignOrders(testcommands);

            // execute order - should go through - but there is not enough cash, so it should reach the bank account
            buyer1.Orders[0].Execute(this.game.Week + 3);
            this.consoleOutReport("Buyer1", buyer1, buyer1.Owner);
            this.consoleOutReport("Seller", seller, seller.Owner);
            this.consoleOutReport("Market", seller.Location.Market, seller.Owner);

            Assert.That(
                buyer1.EventReports[0].Report(buyer1.Owner)[0],
                Is.EqualTo("week 4: received copy of city planning [ctypln] technology from Caste Prime Headquarters [000112]."));
            Assert.That(
                seller.EventReports[0].Report(seller.Owner)[0],
                Is.EqualTo("week 4: copied city planning [ctypln] technology to Warszawa [000001]."));

            // check if both offers are no longer on the market, and that both orders are marked as executed          
            Assert.That(this.game.Offers[EOfferType.BuyTechnologies][cityPlanning][buyer1].Count, Is.EqualTo(0));
            Assert.That(this.game.Offers[EOfferType.SellTechnologies][cityPlanning][seller].Count, Is.EqualTo(0));

            Assert.That(seller.Orders[0].Executed);
            Assert.That(buyer1.Orders[0].Executed);
            // an assert should be there to prove buyer2 not executed order, but there wasn't an order in the first place, 
            // just an auto generated offer

            // check if pricelist got updated
            Console.WriteLine("Pricelist:");
            foreach (NamedType namedType in market.PriceList.Keys)
            {
                Console.WriteLine(namedType.ReportName + " " + market.PriceList[namedType]);
            }
            Assert.That(market.PriceList.ContainsKey(cityPlanning), "with prior transaction, pricelist should have a single entry");
            Assert.That(
                market.GetPrice(cityPlanning),
                Is.EqualTo(500),
                "with getPrice initiated and no other prices on other markets, price should be set to last selling price");

            // received, we have technology in two places now
            Assert.That(buyer1.HasTechnology(cityPlanning));
            Assert.That(buyer2.HasTechnology(cityPlanning), Is.False);
            Assert.That(seller.HasTechnology(cityPlanning));
        }

        [Test, Ignore("not ready")]
        public void ProcessGenerateAutoOffers()
        {
            // assert that buys sell orders are in place standing
            // market.process
            // assert that new buys sell orders are added
            // assert prices are proper 
            Assert.Fail();
        }

        [Test]
        public void DoNotFindOwnOffer()
        {
            // buy 200 food
            // assert that buys sell orders are in place standing
            // berlin [000005] wants to buy 200 food
            // berlin farms [000008] wants to sell 40 food
            // TODO: this shouldn't be happening, either city or farms are greater need/output not both
            Region region = this.game.Regions["R00001"];
            Market market = region.Market;

            ItemType food = ItemType.All["food"];
            ItemType cash = ItemType.All["cash"];

            ModuleStack buyer = this.game.ModuleStacks["000005"];
            ModuleStack seller = this.game.ModuleStacks["000008"];

            Offer buyOffer = this.game.Offers[EOfferType.BuyItems][food][buyer].GetIndex(0);
            Assert.That(buyOffer.Quantity, Is.EqualTo(200));

            Offer sellOffer = this.game.Offers[EOfferType.SellItems][food][seller].GetIndex(0);
            Assert.That(sellOffer.Quantity, Is.EqualTo(40));

            seller.Owner = buyer.Owner;

            Offer foundOffer = market.FindMatch(buyOffer);
            ClassicAssert.IsNull(foundOffer);
        }
        // prices changes
	}
}
				