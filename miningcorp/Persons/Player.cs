using GameCorpLib.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib
{
	public class Player
	{
		public string Name;
		public readonly int Id;
		public bool Admin;
		string Password;
		IDictionary<int, Company> companiesControled = new Dictionary<int, Company>();
		public readonly Trader Trader;
		public Stock Stock => Trader.Stock;
		public Player(string name, string password, PlayersRegister playersRegister, Bank bank, double hardStockSize)
		{
			Name = name;
			Admin = false;
			playersRegister.RegisterItem(this, out int id);
			Password = password;
			Trader = new Trader(new NormalStock(bank, this, hardStockSize));
		}
		public bool CheckPassword(string password)
		{
			return password == Password;
		}
	}
}
