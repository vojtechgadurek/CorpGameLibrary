﻿using GameCorpLib.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameCorpLib.Tradables;
using GameCorpLib.Persons;

namespace GameCorpLib
{
	internal class Game
	{
		public int Round = 0;
		public Registers Registers = new Registers();
		public OilFieldProspector OilMineProspector;

		// Game setup variable
		public Resource MinePrice = Resource.CreateMoney(1000);
		public Game()
		{
			OilMineProspector = new OilFieldProspector(Registers.PropertyRegister, MinePrice);
			CreateAdminAccount();
		}
		void CreateAdminAccount()
		{
			new Player("admin", "admin", Registers.PlayersRegister).Stock.TryAddResource(new Resource(ResourceType.Money, 1000000));
		}
		public void NewRound()
		{
			Round++;
			Registers.PropertyRegister.Update();
		}
	}
}
