using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameCorpLib;

namespace GameCorpLib
{
	public interface IUserMaintainer
	{
		public Player? TryLoginPlayer(string name, string password);
		public Player? TryRegisterNewPlayer(string name, string password);
		public Player? TryGetPlayerByName(string name);
	}
}

