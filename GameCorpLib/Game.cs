using GameCorpLib.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameCorpLib.Persons;
using GameCorpLib.Markets;

namespace GameCorpLib
{
	public class Game
	{
		// Game setup variables
		public readonly R<Money> MinePrice = new R<Money>(1000);
		public readonly R<Money> PlayerStartingMoney = new R<Money>(10000);
		public readonly R<Money> MinimalOilPriceOnMarket = new R<Money>(10);
		public readonly double InterestPerRound = 10;
		public readonly double BaseMoneyForNormalPlayer = 10000;
		public readonly double BaseStockSize = 10000;


		public int Round = 0;
		public Registers Registers = new Registers();
		public OilFieldProspector OilMineProspector;
		public SpotMarketInResource<Oil> SpotMarket;
		public Bank Bank;



		public Game()
		{
			SpotMarket = new SpotMarketInResource<Oil>(MinimalOilPriceOnMarket);
			Bank = new Bank(InterestPerRound);
			OilMineProspector = new OilFieldProspector(Registers.PropertyRegister, MinePrice);
			CreateAdminAccount();
		}
		void CreateAdminAccount()
		{
			var player = new Player("admin", "admin", Registers.PlayersRegister, Bank, BaseStockSize, SpotMarket);
			player.Admin = true;
			player.Stock.TrySetResourceCapacity(new R<Capacity<Money>>(100000000));
			player.Stock.TrySetResource(new R<Money>(1000000));
		}
		public void NewRound()
		{
			Round++;
			Registers.PropertyRegister.Update();
			Bank.Update();
		}
	}
}
