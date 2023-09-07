using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Stocks
{
	public static class SiloFactory
	{
		public static Silo<Money> CreateCashSilo(Player player, Bank bank)
		{
			BankMoneySpillHandler bankMoneySpillHandler = new BankMoneySpillHandler(bank, player);
			return new SiloConfiguration<Money>()
				.SetInfiniteCapacity()
				.SetSpillHandler(bankMoneySpillHandler)
				.SetUnderfillHandler(bankMoneySpillHandler)
				.CreateSiloInstance();
		}

		public static Silo<TResourceType> CreateHardResourceSilo<TResourceType>(double capacity, Player player, SpotMarket spotMarket)
		{
			ResourceSpillHandler<TResourceType> resourceSpillHandler = new ResourceSpillHandler<TResourceType>(player, spotMarket.GetSpotMarket<TResourceType>());
			return new SiloConfiguration<TResourceType>()
				.SetCapacity(capacity.Create<TResourceType>().ToCapacity())
				.SetSpillHandler(resourceSpillHandler)
				.SetUnderfillHandler(resourceSpillHandler)
				.CreateSiloInstance();
		}

		public static Silo<TResourceType> CreateNoLimitsSilo<TResourceType>()
		{
			return new SiloConfiguration<TResourceType>()
				.SetNoLimits()
				.CreateSiloInstance();
		}
	}
}
