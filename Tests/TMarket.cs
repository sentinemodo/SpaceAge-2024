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
	public class TMarket : TTest
	{
		public TMarket()
		{			
		}
			
		[SetUp]
		public void SetupOrder()
		{
			this.LoadDefaultGame();
		}

		[TearDown]
		public void TeardownOrder()
		{
			this.ClearGame();
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
            Assert.That(foundOffer, Is.Null);

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
        public void ProcessOffer_SellQuantityExceedsInventory_ClampsAndDoesNotThrow()
        {
            Region region = this.game.Regions["R00001"];
            Market market = region.Market;
            ItemType food = ItemType.All["food"];
            ModuleStack buyer = this.game.ModuleStacks["000005"];
            ModuleStack seller = this.game.ModuleStacks["000008"];
            seller.Owner = Faction.All["2"];
            seller.ItemStacks.Remove(new ItemStack(food, 30));
            Assert.That(seller.ItemStacks[food].Quantity, Is.EqualTo(10));

            Offer buyOffer = this.game.Offers[EOfferType.BuyItems][food][buyer].GetIndex(0);
            buyOffer.AllQuantity = true;

            Assert.DoesNotThrow(() => market.ProcessOffer(this.game.Week, buyOffer));
            Assert.That(seller.ItemStacks.Quantity(food), Is.EqualTo(0));
            ReceivingItems foodTransfer = (ReceivingItems)buyer.Effects[0];
            Assert.That(foodTransfer.ItemStack.Quantity, Is.EqualTo(10));
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

        [Test]
        public void ProcessGenerateAutoOffers()
        {
            ModuleStack berlin = this.game.ModuleStacks["000005"];
            ModuleStack farms = this.game.ModuleStacks["000008"];
            ItemType food = ItemType.All["food"];
            ItemType terran = ItemType.All["terran"];
            ItemType iron = ItemType.All["iron"];
            ItemType cash = ItemType.All.Cash;

            Offer standingBuyFood = this.game.Offers[EOfferType.BuyItems][food][berlin].GetIndex(0);
            Assert.That(standingBuyFood.Quantity, Is.EqualTo(200));
            Offer standingSellTerran = this.game.Offers[EOfferType.SellItems][terran][berlin].GetIndex(0);
            Assert.That(standingSellTerran.Quantity, Is.EqualTo(20));
            Assert.That(standingSellTerran.Price, Is.EqualTo(50));
            Offer standingFarmFood = this.game.Offers[EOfferType.SellItems][food][farms].GetIndex(0);
            Assert.That(standingFarmFood.Quantity, Is.EqualTo(40));

            berlin.ItemStacks.Add(new ItemStack(iron, 15));
            berlin.Location.Market.AddPrice(iron, 10);

            this.game.GenerateOffers();

            Assert.That(this.game.Offers[EOfferType.BuyItems][food][berlin].GetIndex(0).Quantity, Is.EqualTo(200));
            Offer terranAfter = this.game.Offers[EOfferType.SellItems][terran][berlin].GetIndex(0);
            Assert.That(terranAfter.Quantity, Is.EqualTo(20));
            Assert.That(terranAfter.Price, Is.EqualTo(50));
            Assert.That(this.game.Offers[EOfferType.SellItems][food][farms].GetIndex(0).Quantity, Is.EqualTo(40));
            Assert.That(this.game.Offers[EOfferType.SellItems][food][berlin].Count, Is.EqualTo(0),
                "city already buying food must not also auto-sell it");
            Assert.That(this.game.Offers[EOfferType.SellItems][cash][berlin].Count, Is.EqualTo(0),
                "cash is not listed for sale");

            Offer ironOffer = this.game.Offers[EOfferType.SellItems][iron][berlin].GetIndex(0);
            Assert.That(ironOffer.Quantity, Is.EqualTo(15));
            Assert.That(ironOffer.Price, Is.EqualTo(10));
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
            Assert.That(foundOffer, Is.Null);
        }

		[Test]
		public void AssignBuyOrder()
		{
			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

			List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "buy 5 terran at 5",
                "#end"
            };

            Assert.That(Offer.All.Count, Is.EqualTo(9));

			OrdersReader ordersReader = new OrdersReader(game);
			ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is BuyOrder);
			BuyOrder buyOrder = (BuyOrder)testModuleStack.Orders[0];
            Assert.That(buyOrder.ItemType, Is.EqualTo(ItemType.All["terran"]));
            Assert.That(buyOrder.Repeat, Is.EqualTo(1));

            Assert.That(Offer.All.Count, Is.EqualTo(9), "assign shouldn't change number of offers");
		}


		[Test]
		public void ExecuteBuyOrder_ImmediateRange()
		{
			this.AssignBuyOrder();

			Faction testFaction = this.game.Factions["2"];
			ModuleStack testModuleStack = this.game.ModuleStacks["000004"];
			Market market = testModuleStack.Location.Market;
			BuyOrder order = (BuyOrder)testModuleStack.Orders[0];
			ItemType terran = ItemType.All["terran"];
			ItemType cash = ItemType.All["cash"];

            // check if the world state changes corretly	
            Assert.That(testModuleStack.ItemStacks[terran].Quantity, Is.EqualTo(10));
            Assert.That(testModuleStack.ItemStacks[cash].Quantity, Is.EqualTo(300));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.Executing, Is.False);
            Assert.That(order.Executed, Is.False);
            Assert.That(testModuleStack.Effects.Count, Is.EqualTo(0));
			Console.WriteLine("existing offers");
			List<string> lines = testModuleStack.Location.Market.Report(testFaction);

			foreach (string line in lines)
			{
				Console.WriteLine(line);
			}

            Assert.That(Offer.All[market].Count, Is.EqualTo(4));

			testModuleStack.Execute(this.game.Week);
            Assert.That(testModuleStack.ItemStacks[terran].Quantity, Is.EqualTo(10), "shouldn'teardownOrder execute - offers are to expensive");
            Assert.That(testModuleStack.ItemStacks[cash].Quantity, Is.EqualTo(300));
            Assert.That(testModuleStack.Owner.Bank.AvailableFunds, Is.EqualTo(20000));
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));
            Assert.That(order.Executing, Is.False, "it's immediate, can't be continously executing");
            Assert.That(order.Executed, Is.False, "offer is placed but not completed - not executed");

			Console.WriteLine("existing offers stage 2");
			lines = testModuleStack.Location.Market.Report(testFaction);
			foreach (string line in lines)
			{
				Console.WriteLine(line);
			}

            Assert.That(Offer.All[market].Count, Is.EqualTo(5));


			Offers testOffers = Offer.All[testModuleStack][EOfferType.BuyItems];
            Assert.That(testOffers.Count, Is.EqualTo(1));
			foreach (Offer offer in testOffers)
			{
                Assert.That(offer.Offerent, Is.EqualTo(testModuleStack));
			}

			order.Buy.Price = 50;
			testModuleStack.Execute(this.game.Week);
            Assert.That(testModuleStack.ItemStacks[terran].Quantity, Is.EqualTo(15));
            Assert.That(testModuleStack.ItemStacks[cash].Quantity, Is.EqualTo(300));
            Assert.That(testModuleStack.Owner.Bank.AvailableFunds, Is.EqualTo(19750)); // used bank account for transaction
            Assert.That(testModuleStack.Effects.Count, Is.EqualTo(0), "there was some kind of effect planned");
            Assert.That(Offer.All[market].Count, Is.EqualTo(4));
		}

		[Test]
		public void Execute_RepeatedIdenticalBuy_DoesNotDuplicateOffer()
		{
			ModuleStack buyer = this.game.ModuleStacks["000004"];
			ItemType terran = ItemType.All["terran"];
			int before = Offer.All[buyer][EOfferType.BuyItems][terran].Count;

			BuyOrder first = new BuyOrder(buyer);
			first.Parse("all terran");
			first.Repeat = -1;
			first.Execute(1);

			BuyOrder second = new BuyOrder(buyer);
			second.Parse("all terran");
			second.Repeat = -1;
			second.Execute(1);

			Assert.That(Offer.All[buyer][EOfferType.BuyItems][terran].Count, Is.EqualTo(before + 1));
			Assert.That(second.Buy, Is.SameAs(first.Buy));
		}

		[Test]
		public void Execute_RepeatedIdenticalSell_DoesNotDuplicateOffer()
		{
			ModuleStack seller = this.game.ModuleStacks["000004"];
			ItemType terran = ItemType.All["terran"];
			int before = Offer.All[seller][EOfferType.SellItems][terran].Count;

			SellOrder first = new SellOrder(seller);
			first.Parse("5 terran at 50");
			first.Execute(1);

			SellOrder second = new SellOrder(seller);
			second.Parse("5 terran at 50");
			second.Execute(1);

			Assert.That(Offer.All[seller][EOfferType.SellItems][terran].Count, Is.EqualTo(before + 1));
			Assert.That(second.Sell, Is.SameAs(first.Sell));
		}

		[Test]
		public void LoadXml_DuplicateBuyingNodes_CollapsesToOneOffer()
		{
			ModuleStack buyer = this.game.ModuleStacks["000004"];
			ItemType terran = ItemType.All["terran"];
			int before = Offer.All[buyer][EOfferType.BuyItems][terran].Count;

			XmlDocument doc = new XmlDocument();
			XmlElement holder = doc.CreateElement("modulestack");
			for (int i = 0; i < 3; i++)
			{
				XmlElement buying = doc.CreateElement("buying");
				buying.SetAttribute("item", "terran");
				buying.SetAttribute("quantity", "all");
				buying.SetAttribute("price", "any");
				holder.AppendChild(buying);
			}

			new Offers().LoadXml(holder, buyer.Location.Market, buyer);

			Assert.That(Offer.All[buyer][EOfferType.BuyItems][terran].Count, Is.EqualTo(before + 1));
		}

		[Test]
		public void Execute_LoadedBuyingNodesAndTwoLeftoverBuys_DoesNotDuplicateOffer()
		{
			ModuleStack buyer = this.game.ModuleStacks["000004"];
			ItemType terran = ItemType.All["terran"];
			int before = Offer.All[buyer][EOfferType.BuyItems][terran].Count;

			XmlDocument doc = new XmlDocument();
			XmlElement holder = doc.CreateElement("modulestack");
			for (int i = 0; i < 3; i++)
			{
				XmlElement buying = doc.CreateElement("buying");
				buying.SetAttribute("item", "terran");
				buying.SetAttribute("quantity", "all");
				buying.SetAttribute("price", "any");
				holder.AppendChild(buying);
			}
			new Offers().LoadXml(holder, buyer.Location.Market, buyer);

			for (int i = 0; i < 2; i++)
			{
				BuyOrder leftover = new BuyOrder(buyer);
				leftover.Parse("all terran");
				leftover.Repeat = -1;
			}

			for (int week = 1; week <= 13; week++)
			{
				foreach (Order order in new List<Order>(buyer.Orders))
				{
					BuyOrder buy = order as BuyOrder;
					if (buy != null)
					{
						buy.Execute(week);
					}
				}
			}

			Assert.That(Offer.All[buyer][EOfferType.BuyItems][terran].Count, Is.EqualTo(before + 1));
			int listed = 0;
			foreach (string line in buyer.Location.Market.Report(buyer.Owner))
			{
				if (line.IndexOf("buy all terrans") >= 0 && line.IndexOf(buyer.ReportName) >= 0)
				{
					listed++;
				}
			}
			Assert.That(listed, Is.EqualTo(1), "the same standing buy must appear once on the market");
		}

        [Test]
        public void AssignSellOrder()
        {
            Faction testFaction = this.game.Factions["2"];
            ModuleStack testModuleStack = this.game.ModuleStacks["000004"];

            List<string> testcommands = new List<string>
            {
                "#faction 2",
                "#modulestack 000004",
                "sell 5 terran at 5",
                "#end"
            };

            Assert.That(Offer.All.Count, Is.EqualTo(9));

            OrdersReader ordersReader = new OrdersReader(game);
            ordersReader.AssignOrders(testcommands);
            Assert.That(testModuleStack.Orders.Count, Is.EqualTo(1));

            Assert.That(testModuleStack.Orders[0] is SellOrder);
            SellOrder sellOrder = (SellOrder)testModuleStack.Orders[0];
            Assert.That(sellOrder.ItemType, Is.EqualTo(ItemType.All["terran"]));
            Assert.That(sellOrder.Repeat, Is.EqualTo(1));

            Assert.That(Offer.All.Count, Is.EqualTo(9), "assign shouldn't change number of offers");
        }

        // prices changes
	}

	[TestFixture]
	public class TContract : TTest
	{
		[SetUp]
		public void Setup()
		{
			this.LoadDefaultGame();
		}

		[TearDown]
		public void Teardown()
		{
			this.game.Week = 1;
			this.ClearGame();
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

		[Test]
		public void Parse_ResearchWreckage_UnitRewardTitleFlavour()
		{
			ModuleStack wreck = new ModuleStack(Region.All["R00002"], Faction.All["1"], ModuleType.All["alnhul"], "200");
			wreck.AddModule();
			List<string> commands = new List<string>
			{
				"#faction 1",
				"CONTRACT R00002 research 200 points 5 REWARD 200 unit TITLE \"Wake the wreck\" FLAVOUR \"Activating the systems.\"",
				"#end"
			};
			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(commands);

			ContractOrder create = (ContractOrder)Faction.All["1"].Orders[0];
			Assert.That(create.ResearchTarget.Name, Is.EqualTo("200"));
			Assert.That(create.ResearchPoints, Is.EqualTo(5));
			Assert.That(create.RewardStack.Name, Is.EqualTo("200"));
			Assert.That(create.Title, Is.EqualTo("Wake the wreck"));
			Assert.That(create.Flavour, Is.EqualTo("Activating the systems."));
			create.Execute(1);

			Assert.That(Contract.All.Count, Is.EqualTo(1));
			Contract published = Contract.All[0];
			Assert.That(published.Title, Is.EqualTo("Wake the wreck"));
			Assert.That(published.RewardStack.Name, Is.EqualTo("200"));
			Assert.That(published.Trigger, Is.InstanceOf<ResearchWreckageTrigger>());
		}

		[Test]
		public void Parse_Press_BetweenTurns()
		{
			List<string> commands = new List<string>
			{
				"#faction 2",
				"PRESS TITLE \"Moon shot\" FLAVOUR \"We go to Luna.\"",
				"#end"
			};
			OrdersReader reader = new OrdersReader(this.game);
			reader.AssignOrders(commands);
			PressOrder press = (PressOrder)Faction.All["2"].Orders[Faction.All["2"].Orders.Count - 1];
			Assert.That(press.AllowedBetweenTurns, Is.True);
			Assert.That(press.Title, Is.EqualTo("Moon shot"));
			this.game.ExecuteBetweenTurnOrders();
			Assert.That(PressRelease.All.Count, Is.EqualTo(1));
			Assert.That(PressRelease.All[0].Issuer.Name, Is.EqualTo("2"));
		}

		[Test]
		public void XmlRoundTrip_PersistsResearchUnitContract()
		{
			ModuleStack wreck = new ModuleStack(Region.All["R00002"], Faction.All["1"], ModuleType.All["alnhul"], "200");
			wreck.AddModule();
			ResearchWreckageTrigger trigger = new ResearchWreckageTrigger(wreck, 5);
			Contract published = new Contract("CT0200", Region.All["R00002"], Faction.All["1"], trigger, wreck);
			published.Title = "Wake the wreck";
			published.Flavour = "Activating the systems.";

			string testdir = Directory.GetCurrentDirectory();
			string testfile = "gameout.researchcontract.xml";
			this.dataFile.SaveGame(testdir, testfile);

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

			Contract loaded = Contract.All["CT0200"];
			Assert.That(loaded, Is.Not.Null);
			Assert.That(loaded.Title, Is.EqualTo("Wake the wreck"));
			Assert.That(loaded.RewardStack.Name, Is.EqualTo("200"));
			ResearchWreckageTrigger loadedTrigger = (ResearchWreckageTrigger)loaded.Trigger;
			Assert.That(loadedTrigger.RequiredPoints, Is.EqualTo(5));
			Assert.That(loadedTrigger.Target.Name, Is.EqualTo("200"));
		}
	}
}
