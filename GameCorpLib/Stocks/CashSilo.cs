using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Stocks
{
	public class CashSilo : Silo
	{
		Bank _bank;
		Player _player;
		public CashSilo(Bank bank, Player player) : base(Resource.CreateMoney(double.PositiveInfinity))
		{
			_bank = bank;
			_player = player;
		}
		public override void HandleSpill(double spill)
		{
			_bank.TakeLoan(_player, Resource.CreateMoney(-spill));
		}
	}
}
