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

		Dictionary<Player, Resource> _loans = new Dictionary<Player, Resource>();
		public Bank(double interestPerRound)
		{
			_interestPerRound = interestPerRound;
		}
		public void TakeLoan(Player player, Resource cashToBorrow)
		{
			if (cashToBorrow.Type != ResourceType.Money) throw new InvalidOperationException();
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

		public Resource GetCashLend(Player player)
		{
			if (!_loans.ContainsKey(player)) _loans.Add(player, Resource.CreateMoney(0));
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
