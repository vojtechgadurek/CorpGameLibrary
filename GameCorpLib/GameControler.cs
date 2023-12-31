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
	//Tohle je v plánu přemyslet, protože v současné architektuře úplně nedává smysl
	public class GameControler : IUserMaintainer
	{
		public readonly Game Game = new Game();

		public R<Money> OilFieldPrice { get => Game.MinePrice; }

		public GameControler()
		{
			Console.WriteLine("GameControler created");
		}
		#region PlayersLogin
		/// <summary>
		/// Logins player with set name and password, if unsuccesful returns null. It is thread safe.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public Player? TryLoginPlayer(string name, string password)
		{
			return Game.Registers.PlayersRegister.TryLoginPlayer(name, password);
		}
		/// <summary>
		/// Creates new player with set name, if unsuccesful returns null. It is thread safe.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public Player? TryRegisterNewPlayer(string name, string password)
		{
			return Game.Registers.PlayersRegister.TryCreateNewPlayer(name, password, Game.Bank, Game.BaseStockSize, Game.SpotMarket);
		}
		public Player? TryGetPlayerByName(string name)
		{
			return Game.Registers.PlayersRegister.TryGetPlayerByName(name);
		}

		#endregion
		public int GetRoundNumber()
		{
			return Game.Round;
		}
		public void ForceNewRound(Player? player)
		{
			if (player == null)
			{
				return;
			}
			if (player.Admin)
			{
				Game.NewRound();
			}
		}

		public bool TryProspectNewOilField(Player? player)
		{
			if (player == null)
			{
				return false;
			}
			return Game.OilMineProspector.TryProspectNewMine(player.Trader);
		}

		public IDictionary<string, Player> GetPlayers()
		{
			return Game.Registers.PlayersRegister._Players;
		}
	}
}
