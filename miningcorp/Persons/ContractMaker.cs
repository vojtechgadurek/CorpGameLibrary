using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Persons
{
	public record class ExchangeContract(Resource Resource, Resource Payment, Trader Writer, Trader Holder, int EndRound, Resource FineForNotCompleting)
	{
		public void Execute(Game game)
		{
			if (game.Round == EndRound)
			{
				bool result = new TwoPartyTransaction(Writer, Holder)
					.AddTransactionItem(Resource, TransactionDirection.FromBuyerToSeller)
					.AddTransactionItem(Payment, TransactionDirection.FromSellerToBuyer)
					.TryExecute();
				if (result) return;
				Writer.Stock.ForceIncreaseResources(-FineForNotCompleting);
			}
		}
	}
	public class ContractMaker
	{

	}
}
