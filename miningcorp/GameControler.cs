using GameCorpLib.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib
{
	public class GameControler : IUserMaintainer
	{
		Game Game = new Game();

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
			return Game.Registers.PlayersRegister.TryCreateNewPlayer(name, password);
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
		public void ForceNewRound(Player player)
		{
			if (player.Admin)
			{
				Game.NewRound();
			}
		}

		public IDictionary<string, Player> GetPlayers()
		{
			return Game.Registers.PlayersRegister._Players;
		}
		public void GetPriceForTrade(Resource resource)
		{
			MarketWrapper.market.DeterminePriceForTrade(resource);
		}

	}
}
