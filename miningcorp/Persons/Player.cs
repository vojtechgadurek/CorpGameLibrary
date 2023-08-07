using GameCorpLib.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib
{
	public class Player : Trader
	{
		public string Name;
		public readonly int Id;
		public bool Admin;
		string Password;
		IDictionary<int, Company> companiesControled = new Dictionary<int, Company>();
		public Player(string name, string password, PlayersRegister playersRegister)
		{
			Name = name;
			Money = 1000;
			Admin = false;
			playersRegister.RegisterItem(this, out int id);
			Password = password;
		}
		public bool CheckPassword(string password)
		{
			return password == Password;
		}
	}
}
