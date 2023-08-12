using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Stock
{
	public class NormalStock : Stock
	{
		public NormalStock(Bank bank, Player owner, double hardStockSize)
		{
			foreach (var resource in Enum.GetValues<ResourceType>())
			{
				if (resource == ResourceType.Money) resources[resource] = new CashSilo(bank, owner);
				else resources[resource] = new Silo(new Resource(resource, hardStockSize));
			};
		}
	}
}
