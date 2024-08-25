using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

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
			this.datafile = new DataFile(Directory.GetCurrentDirectory());
			this.datafile.LoadConfiguration();
			this.datafile.LoadGame();
			this.game = this.datafile.Game;
		}

		[TearDown]
		public void TeardownOrder()
		{
			this.game.ClearDictionaries();
			this.game = null;
			this.datafile = null;
		}

		[Test]
		public void SetupTeardown()
		{
			Assert.IsTrue(true);
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
            Assert.AreEqual(200, buyOffer.Quantity);

            Offer sellOffer = this.game.Offers[EOfferType.SellItems][food][seller].GetIndex(0);
            Assert.AreEqual(40, sellOffer.Quantity);

            Offer foundOffer = market.FindMatch(buyOffer);
            Assert.IsNull(foundOffer);

            seller.Owner = Faction.All["2"];
            foundOffer = market.FindMatch(buyOffer);
            // can work only if buyer and seller are from different factions
            Assert.AreEqual(sellOffer, foundOffer);
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
            Assert.AreEqual(9, Offer.All.Count);

            Assert.AreEqual(40, seller.ItemStacks[food].Quantity);

            Offer buyOffer = this.game.Offers[EOfferType.BuyItems][food][buyer].GetIndex(0);
            Assert.AreEqual(200, buyOffer.Quantity);

            Offer sellOffer = this.game.Offers[EOfferType.SellItems][food][seller].GetIndex(0);
            Assert.AreEqual(40, sellOffer.Quantity);

            Assert.IsFalse(market.PriceList.ContainsKey(food), "without prior transaction, pricelist should be empty");
            Assert.AreEqual(0, market.GetPrice(food), "with getPrice initiated and no other prices on other markets, price should be set to 0 - any price");

            // market.process
            // assert orders are matched and done
            // assert matched orders are removed
            this.game.Week = 1;

            market.ProcessOffer(this.game.Week, buyOffer);
            Assert.AreEqual(160, buyOffer.Quantity);

            Assert.AreEqual(0, this.game.Offers[EOfferType.SellItems][food][seller].Count);
            
            // assert items transfers is taking place
            ReceivingItems foodTransfer = (ReceivingItems)buyer.Effects[0];            
            Assert.AreEqual(40, foodTransfer.ItemStack.Quantity);
            Assert.AreEqual(food, foodTransfer.ItemStack.ItemType);

            this.consoleOutReport("food transfer event", foodTransfer, buyer.Owner);

            foodTransfer.Execute(this.game.Week);
            
            // received
            Assert.AreEqual(40, buyer.ItemStacks[food].Quantity);

            // assert prices get updated accordingly
            Assert.AreEqual(2, market.PriceList[food]);
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

            List<string> testcommands = new List<string>();
            testcommands.Add("#faction 2");
            testcommands.Add("#modulestack 000004");
            testcommands.Add("@buy all terran everywhere");
            testcommands.Add("#end");

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            // assert existing offers
            this.consoleOutReport("Existing offers", Offer.All, buyerFaction);
            Assert.AreEqual(9, Offer.All.Count);

            // assert buyer and seller are existing and in a single place
            this.consoleOutReport("Location of buy/seller1", buyer.Location.Market, buyerFaction);
            this.consoleOutReport("Location of seller2", seller2.Location.Market, buyerFaction);

            Assert.AreEqual(10, seller1.ItemStacks[terran].Quantity);
            Assert.AreEqual(20, seller2.ItemStacks[terran].Quantity);

            // assert that buyer has cash
            Assert.AreEqual(300, buyer.ItemStacks[cash].Quantity);
            // 300 cash would suffice for 6 terrans at 50 for cash, but we're using bank account anyway

            #region buyer.Execute(this.game.Week);
            BuyOrder order = (BuyOrder)buyer.Orders[0];
            Assert.IsTrue(order.AllQuantity);
            Assert.IsTrue(order.Repeat < 0);

            order.Execute(this.game.Week);
            buyer.Orders.RemoveExecuted();
            #endregion

            // assert existing offers
            this.consoleOutReport("Existing offers", Offer.All, buyerFaction);
            Assert.IsFalse(seller1.ItemStacks.ContainsKey(terran));
            Assert.IsFalse(seller2.ItemStacks.ContainsKey(terran));

            // there are still only 20 terrans in place (10 already received), the rest is in effects (transferring)
            Assert.AreEqual(20, buyer.ItemStacks[terran].Quantity);

            // but the cash is already gone 10 local, 20 other region, transferCost for different regions transaction
            Assert.AreEqual(20000 - 10 * 50 - 20 * 50 - 100, buyer.Owner.Bank.AvailableFunds);

            Assert.AreEqual(2, buyer.Effects.Count);

            // first transfers reached 
            Assert.AreEqual(20, buyer.ItemStacks[terran].Quantity);
            // second transfer has still 5 week of duration
            Receiving receivingEffect = (Receiving)buyer.Effects[1];
            Assert.AreEqual(5, receivingEffect.Duration);

            // assert remaining offers
            this.consoleOutReport("Location of buyer/seller1", buyer.Location.Market, buyerFaction);
            this.consoleOutReport("Location of seller2", seller2.Location.Market, buyerFaction);
            Assert.AreEqual(8, Offer.All.Count); // two sell offers done, one new buy offer in place

            // TODO: this will require further work to remove possibility to add offers without orders, as of now it works the both ways, which can be useful
            // assert orders are matched and done
            // assert matched orders are removed
            
            #region buyer.Execute(this.game.Week) - continued2;
            Assert.IsTrue(order.Executed);
            Assert.AreEqual(1, buyer.Orders.Count);
            buyer.Orders.RemoveExecuted();
            Assert.AreEqual(1, buyer.Orders.Count); // stays the same, because the order is unlimited
            #endregion

            // assert prices get updated accordingly
            Assert.IsTrue(buyer.Location.Market.PriceList.ContainsKey(terran));
            Assert.IsTrue(seller2.Location.Market.PriceList.ContainsKey(terran));
            Assert.AreEqual(50, buyer.Location.Market.GetPrice(terran));
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
            Assert.AreEqual(9, Offer.All.Count);

            // there should be 10 existing windplants in seller and buyer shouldn't (yet) have any windplants for sale
            Assert.AreEqual(10, buyer.Modules.Count);
            Assert.IsFalse(seller.HasModuleStacks(windplant));

            // setup sell order
            List<string> testcommands = new List<string>();
            testcommands.Add("#faction 2");
            testcommands.Add("#modulestack 000004");
            testcommands.Add("sell all wnplnt");
            testcommands.Add("#end");

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            // execute order - should fail, because seller has no windplants
            seller.Orders[0].Execute(this.game.Week);
            Assert.IsFalse(seller.Orders[0].Executed);

            // add the windplant and execute again, should go, but no sell done, since there is no matching order/offer
            ModuleStack windplantStack = new ModuleStack(seller, seller.Owner, windplant);
            windplantStack.AddModule();
            this.consoleOutReport("seller", seller, seller.Owner);
            Assert.IsTrue(seller.HasModuleStacks(windplant));
            seller.Orders[0].Execute(this.game.Week + 1);

            // check if an offer appeared
            Offer sellOffer = this.game.Offers[EOfferType.SellModules][windplant][seller].GetIndex(0);
            Assert.IsTrue(sellOffer.AllQuantity);
            Assert.IsFalse(seller.Orders[0].Executed);

            // setup buy order
            testcommands = new List<string>();
            testcommands.Add("#faction 1");
            testcommands.Add("#modulestack 000009");
            testcommands.Add("buy 1 wnplnt");
            testcommands.Add("#end");

            ordersReader.AssignOrders(testcommands);

            // execute order - should go through - but there is not enough cash, so it should reach the bank account
            buyer.Orders[0].Execute(this.game.Week + 1);
            this.consoleOutReport("Buyer", buyer, buyer.Owner);
            this.consoleOutReport("Seller", seller, seller.Owner);
  
            Assert.AreEqual(
                "week 2: received wind powerplant [wnplnt] from factory [000004].", 
                buyer.EventReports[0].Report(buyer.Owner)[0]);
            Assert.AreEqual(
                "week 2: transferred wind powerplant [wnplnt] to Warsaw wind powerplants [000009].", 
                seller.EventReports[0].Report(seller.Owner)[0]);

            // check if both offers are no longer on the market, and that both orders are marked as executed          
            Assert.AreEqual(0, this.game.Offers[EOfferType.BuyModules][windplant][buyer].Count);
            Assert.AreEqual(0, this.game.Offers[EOfferType.SellModules][windplant][seller].Count);

            Assert.IsTrue(seller.Orders[0].Executed);
            Assert.IsTrue(buyer.Orders[0].Executed);

            // check if pricelist got updated
            Assert.IsFalse(market.PriceList.ContainsKey(windplant), "without prior transaction, pricelist should be empty");
            Assert.AreEqual(-1, market.GetPrice(windplant), "with getPrice initiated and no other prices on other markets, price should be set to 0 - any price");
           
            // received, factory again has no modulestack
            Assert.AreEqual(11, buyer.Modules.Count);
            Assert.IsFalse(seller.HasModuleStacks(windplant));
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
            Assert.AreEqual(9, Offer.All.Count);

            // there shouldn't be technology in seller nor in buyers 
            Assert.AreEqual(0, buyer1.Technologies.Count);
            Assert.AreEqual(0, buyer2.Technologies.Count);
            Assert.AreEqual(0, seller.Technologies.Count);
            Assert.IsFalse(seller.HasTechnology(cityPlanning));

            // setup sell order
            List<string> testcommands = new List<string>();
            testcommands.Add("#faction 2");
            testcommands.Add("#modulestack 000112");
            testcommands.Add("sell ctypln at 500");
            testcommands.Add("#end");

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);

            // execute order - should fail, because seller has no technology
            seller.Orders[0].Execute(this.game.Week);
            Assert.IsFalse(seller.Orders[0].Executed);

            // add the technology and execute again, should go, but no sell done, since there is no matching order/offer
            // the existing one is too low and in bad region
            seller.Technologies.Add(cityPlanning);
            this.consoleOutReport("seller", seller, seller.Owner);
            this.consoleOutReport("seller orders", seller.Orders, seller.Owner);
            Assert.IsTrue(seller.HasTechnology(cityPlanning));
            seller.Orders[0].Execute(this.game.Week + 1);

            // check if an offer appeared
            this.consoleOutReport("Existing offers", Offer.All, seller.Owner);
            Offer sellOffer = this.game.Offers[EOfferType.SellTechnologies][cityPlanning][seller].GetIndex(0);
            Assert.AreEqual(cityPlanning.Name, sellOffer.Technology.Name);
            Assert.IsFalse(seller.Orders[0].Executed);

            // change the external buyer to increase the price
            Offer buyer2Offer = this.game.Offers[EOfferType.BuyTechnologies][cityPlanning][buyer2].GetIndex(0);
            buyer2Offer.Price = 600;

            // execute again, should go, but no sell done, since there is no matching order/offer
            // the existing one is in a bad region 
            seller.Orders[0].Execute(this.game.Week + 2);

            // setup buy order
            testcommands = new List<string>();
            testcommands.Add("#faction 1");
            testcommands.Add("#modulestack 000001");
            testcommands.Add("buy ctypln at 525");
            testcommands.Add("#end");

            ordersReader.AssignOrders(testcommands);

            // execute order - should go through - but there is not enough cash, so it should reach the bank account
            buyer1.Orders[0].Execute(this.game.Week + 3);
            this.consoleOutReport("Buyer1", buyer1, buyer1.Owner);
            this.consoleOutReport("Seller", seller, seller.Owner);
            this.consoleOutReport("Market", seller.Location.Market, seller.Owner);

            Assert.AreEqual(
                "week 4: received copy of city planning [ctypln] technology from Caste Prime Headquarters [000112].", 
                buyer1.EventReports[0].Report(buyer1.Owner)[0]);
            Assert.AreEqual(
                "week 4: copied city planning [ctypln] technology to Warszawa [000001].", 
                seller.EventReports[0].Report(seller.Owner)[0]);

            // check if both offers are no longer on the market, and that both orders are marked as executed          
            Assert.AreEqual(0, this.game.Offers[EOfferType.BuyTechnologies][cityPlanning][buyer1].Count);
            Assert.AreEqual(0, this.game.Offers[EOfferType.SellTechnologies][cityPlanning][seller].Count);

            Assert.IsTrue(seller.Orders[0].Executed);
            Assert.IsTrue(buyer1.Orders[0].Executed);
            // an assert should be there to prove buyer2 not executed order, but there wasn't an order in the first place, 
            // just an auto generated offer

            // check if pricelist got updated
            Console.WriteLine("Pricelist:");
            foreach (NamedType namedType in market.PriceList.Keys)
            {
                Console.WriteLine(namedType.ReportName + " " + market.PriceList[namedType]);
            }
            Assert.IsTrue(market.PriceList.ContainsKey(cityPlanning), "with prior transaction, pricelist should have a single entry");
            Assert.AreEqual(
                500, 
                market.GetPrice(cityPlanning), 
                "with getPrice initiated and no other prices on other markets, price should be set to last selling price");

            // received, we have technology in two places now
            Assert.IsTrue(buyer1.HasTechnology(cityPlanning));
            Assert.IsFalse(buyer2.HasTechnology(cityPlanning));
            Assert.IsTrue(seller.HasTechnology(cityPlanning));
        }

        [Test, Ignore]
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
            Assert.AreEqual(200, buyOffer.Quantity);

            Offer sellOffer = this.game.Offers[EOfferType.SellItems][food][seller].GetIndex(0);
            Assert.AreEqual(40, sellOffer.Quantity);

            seller.Owner = buyer.Owner;

            Offer foundOffer = market.FindMatch(buyOffer);
            Assert.IsNull(foundOffer);
        }
        // prices changes
	}
}
				