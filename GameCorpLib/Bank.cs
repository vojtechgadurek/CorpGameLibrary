using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib
{
	public class Bank
	{
		private double _interestPerRound = 10;
		public double InterestPerRound { get => _interestPerRound; }

		Dictionary<Player, R<Money>> _loans = new Dictionary<Player, R<Money>>();
		public Bank(double interestPerRound)
		{
			_interestPerRound = interestPerRound;
		}
		public void TakeLoan(Player player, R<Money> cashToBorrow)
		{
			if (_loans.ContainsKey(player))
			{
				_loans[player] += cashToBorrow;
			}
			else
			{
				_loans.Add(player, cashToBorrow);
			}
			player.Stock.ForceIncreaseResources(cashToBorrow);
		}

		public R<Money> GetCashLend(Player player)
		{
			if (!_loans.ContainsKey(player)) _loans.Add(player, new R<Money>(0));
			return _loans[player];
		}

		public void Update()
		{
			foreach (var loan in _loans)
			{
				_loans[loan.Key] += loan.Value * _interestPerRound / 100;
			}
		}
	}
}
