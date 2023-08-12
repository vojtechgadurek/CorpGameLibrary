using GameCorpLib.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameCorpLib.Tradables;
using GameCorpLib.Persons;

namespace GameCorpLib
{
	public class Game
	{
		public int Round = 0;
		public Registers Registers = new Registers();
		public OilFieldProspector OilMineProspector;
		public SpotMarket SpotMarket = new SpotMarket(Resource.CreateMoney(0));
		public Bank Bank = new Bank(10);

		// Game setup variables
		public readonly Resource MinePrice = Resource.CreateMoney(1000);
		public readonly Resource PlayerStartingMoney = Resource.CreateMoney(10000);
		public Game()
		{
			OilMineProspector = new OilFieldProspector(Registers.PropertyRegister, MinePrice);
			CreateAdminAccount();
		}
		void CreateAdminAccount()
		{
			var player = new Player("admin", "admin", Registers.PlayersRegister);
			player.Admin = true;
			player.Stock.TrySetResourceCapacity(Resource.CreateMoney(100000000));
			player.Stock.TrySetResource(new Resource(ResourceType.Money, 1000000));
		}
		public void NewRound()
		{
			Round++;
			Registers.PropertyRegister.Update();
			Bank.Update();
		}
	}
}
