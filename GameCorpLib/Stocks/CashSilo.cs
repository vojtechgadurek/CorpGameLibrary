using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Stocks
{
	public class CashSilo : Silo<Money>
	{
		Bank _bank;
		Player _player;
		public CashSilo(Bank bank, Player player) : base(new SiloConfiguration<Money>().SetInfiniteCapacity())
		{
		}
		{
			_bank = bank;
			_player = player;
		}
	public override void HandleSpill(R<Money> spill)
	{
		_bank.TakeLoan(_player, -spill);
	}
}
}
