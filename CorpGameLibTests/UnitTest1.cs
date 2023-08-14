namespace CorpGameLibTests
{
	using System.ComponentModel;
	using System.Numerics;
	using System.Security.Cryptography.X509Certificates;
	using GameCorpLib;
	using GameCorpLib.Markets;
	using GameCorpLib.State;
	using GameCorpLib.Stocks;
	using GameCorpLib.Tradables;
	using GameCorpLib.Transactions;

	namespace MinnigCorpTests
	{
		static public class Utils
		{
			public class BasicGame
			{
				/// <summary>
				/// Create a basic game for testing purposes
				/// </summary>
				public GameControler GameControler;
				//Rich should have infinite money
				public Player? Rich;
				//Poor should have no money
				public Player? Poor;
				public BasicGame()
				{
					GameControler = new GameControler();
					bool ok = true;
					Rich = GameControler.TryRegisterNewPlayer("rich", "");
					ok &= Rich.Stock.TrySetResourceCapacity(Resource.CreateMoney(double.PositiveInfinity));
					ok &= Rich.Stock.TrySetResource(new Resource(ResourceType.Money, double.MaxValue));
					Poor = GameControler.TryRegisterNewPlayer("poor", "");
					ok &= Poor.Stock.TrySetResource(new Resource(ResourceType.Money, 0));
					ok &= Rich != null;
					ok &= Poor != null;
					if (!ok) throw new System.Exception("BasicGame initialization failed");
				}
			}
		}

		public class TestSilo
		{
			[Theory]
			[InlineData(10000, 1000, true)]
			[InlineData(10000, 100000, false)]
			[InlineData(10000, 10000, true)]
			public void TestSiloAdd(double capacity, double resourceAmountToAdd, bool ansver)
			{
				Silo silo = new Silo(Resource.CreateOil(capacity));
				bool result = silo.TryIncreaseAmount(Resource.CreateMoney(resourceAmountToAdd));
				Assert.Equal(ansver, result);
				if (result) Assert.Equal(resourceAmountToAdd, silo.Amount);
			}

			[Theory]
			[InlineData(10000, 1000, true)]
			[InlineData(10000, 100000, false)]
			[InlineData(10000, 10000, true)]

			public void TestSiloBlock(double capacity, double capacityToBlock, bool ansver)
			{
				Silo silo = new Silo(Resource.CreateOil(capacity));
				bool result = silo.TryBlockCapacity(Resource.CreateMoney(capacityToBlock));
				Assert.Equal(ansver, result);
				if (result) Assert.Equal(capacityToBlock, silo.BlockedCapacity);
			}

			[Theory]
			[InlineData(10000, 1000, 10)]

			public void TestUseBlockedCapacity(double capacity, double capacityToBlock, double capacityToFill)
			{
				Silo silo = new Silo(Resource.CreateOil(capacity));
				silo.TryBlockCapacity(Resource.CreateMoney(capacityToBlock));
				silo.UseBlockedResourceCapacity(Resource.CreateMoney(capacityToFill));
				Assert.Equal(capacityToBlock - capacityToFill, silo.BlockedCapacity);
				Assert.Equal(capacityToFill, silo.Amount);
			}
		}
		public class TestUserManagment
		{
			[Fact]
			public void TestCreatePlayer()
			{
				GameControler gameControler = new GameControler();
				Player? player = gameControler.TryRegisterNewPlayer("test", "test");
				Player? loggedPlayer = gameControler.TryLoginPlayer("test", "tets");
				Assert.Null(loggedPlayer);
				loggedPlayer = gameControler.TryLoginPlayer("test", "test");
				Assert.Equal(player, loggedPlayer);
			}
		}
		public class MockProperty : Property
		{
			public MockProperty(Trader owner, PropertyRegister propertyRegister) : base(owner, propertyRegister) { }
			public override void Update()
			{
				throw new NotImplementedException();
			}
		}
		public class TestPropertyManagment
		{
			[Fact]
			public void TestProspectNewOilField()
			{
				//Rich property buy should be succesful
				//Poor property buy should be unsuccesful
				//This test is testing if prospesting works correctly

				//There is double test for rich to see if double buy works correctly
				Utils.BasicGame basicGame = new Utils.BasicGame();

				GameControler gameControler = basicGame.GameControler;
				//Rich has infinite money, so should succeed
				Assert.True(gameControler.TryProspectNewOilField(basicGame.Rich));
				//Rich has infinite money, so should succeed
				Assert.True(gameControler.TryProspectNewOilField(basicGame.Rich));
				//Poor has no money, so should fail
				Assert.False(gameControler.TryProspectNewOilField(basicGame.Poor));

			}

			[Fact]
			public void TestChangeOwner()
			{
				/*
				var basicGame = new Utils.BasicGame();
				MockProperty newProperty = new MockProperty(basicGame.Rich, basicGame.GameControler.Game.Registers.PropertyRegister);
				*/

			}
		}

		public class TestRegisters
		{
			[Fact]
			public void TestIssueId()
			{
				Register<int> register = new Register<int>();
				int first;
				register.RegisterItem(1, out first);
				int second;
				register.RegisterItem(2, out second);
				Assert.Equal(0, first);
				Assert.Equal(1, second);
			}
		}

		public class TestTransasctions
		{
			[Fact]
			public void TestPartialTransactionSimple1()
			{
				var Game = new Utils.BasicGame();
				Assert.True(Game.Poor.Stock.TrySetResource(Resource.CreateOil(1000)));
				var transaction = new ProportionalTransaction(
						Resource.CreateOil(100),
						Resource.CreateMoney(100),
						Game.Poor.Trader,
						Game.Rich.Trader);
				Assert.True(transaction.TryExecuteProportional(0.5));
				Assert.Equal(50, Game.Rich.Stock.GetResource(ResourceType.Oil).Amount);
				Assert.Equal(950, Game.Poor.Stock.GetResource(ResourceType.Oil).Amount);

				Assert.True(transaction.TryExecuteProportional(1));
				Assert.False(transaction.TryExecuteProportional(0));
				Assert.False(transaction.TryExecuteProportional(0.5));

			}

			[Fact]
			public void TestPartialTranfer()
			{
				var Game = new Utils.BasicGame();
				var transfer = new ResourceTransfer(Game.Rich.Trader, Game.Poor.Trader, Resource.CreateMoney(1000));

				Assert.True(transfer.TryExecutePartialTransfer(Resource.CreateMoney(100)));
				Assert.Equal(100, Game.Poor.Stock.GetResource(ResourceType.Money).Amount);

				Assert.True(transfer.TryExecutePartialTransfer(Resource.CreateMoney(900)));
				Assert.Equal(1000, Game.Poor.Stock.GetResource(ResourceType.Money).Amount);

				Assert.False(transfer.TryExecutePartialTransfer(Resource.CreateMoney(0)));
				Assert.Equal(1000, Game.Poor.Stock.GetResource(ResourceType.Money).Amount);


			}
		}


		public class TestSpotMarket
		{
			[Fact]
			public void TestCreateTrade()
			{
				var Game = new Utils.BasicGame();
				var market = Game.GameControler.Game.SpotMarket;
				Game.Poor.Stock.TryAddResource(Resource.CreateOil(1000));
				Assert.True(market.TryCreateNewTradeOffer(Resource.CreateOil(10), Resource.CreateMoney(10), Game.Poor.Trader, SpotMarketOfferType.Sell));
				Assert.Equal(1, market.SellOffers.Count);
				Assert.True(market.TryCreateNewTradeOffer(Resource.CreateOil(10), Resource.CreateMoney(10), Game.Poor.Trader, SpotMarketOfferType.Sell));
				Assert.Equal(2, market.SellOffers.Count);

				Assert.True(market.TryCreateNewTradeOffer(Resource.CreateOil(10), Resource.CreateMoney(10), Game.Rich.Trader, SpotMarketOfferType.Buy));

				Assert.Equal(1, market.SellOffers.Count);
			}
		}
	}
}