using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Persons
{
	public record class ExchangeContract(Resource Resource, Resource Payment, Trader Writer, Trader Holder, int EndRound)
	{
		public void Execute(Game game)
		{
			if (game.Round == EndRound)
			{

			}
		}
	}
	public class ContractMaker
	{

	}
}
