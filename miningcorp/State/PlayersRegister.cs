using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.State
{
	public class PlayersRegister : Register<Player>
	{
		public IDictionary<string, Player> _Players = new Dictionary<string, Player>();

		/// <summary>
		/// Creates new player with set name, if unsuccesful returns null
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public Player? TryCreateNewPlayer(string name, string password)
		{
			lock (_Players)
			{
				if (_Players.ContainsKey(name)) return null;
				Player player = new Player(name, password, this);
				return player;
			}
		}
		internal override void AfterRegisterItem(Player player)
		{
			_Players.Add(player.Name, player);
		}
		/// <summary>
		/// Tries to login player with set name and password, if unsuccesful returns null. It is thread safe.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public Player? TryLoginPlayer(string name, string password)
		{
			if (!_Players.ContainsKey(name)) return null;
			Player player = _Players[name];
			if (!player.CheckPassword(password)) return null;
			return player;
		}
		public Player? TryGetPlayerByName(string name)
		{
			if (!_Players.ContainsKey(name)) return null;
			return _Players[name];
		}

	}
}
