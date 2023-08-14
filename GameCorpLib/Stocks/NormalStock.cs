using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Stocks
{
	public class NormalStock : Stock
	{
		protected IDictionary<Type, object> silos = new Dictionary<Type, object>();
		public NormalStock(Bank bank, Player owner, double hardStockSize, SpotMarket<Oil> spotMarket)
		{

			silos.Add(typeof(Money), new CashSilo(bank, owner));
			silos.Add(typeof(Oil), new HardStuffSilo<Oil>(new R<Capacity<Oil>>(hardStockSize), spotMarket, owner));
		}

		private Silo<TResourceType> GetSilo<TResourceType>()
		{
			var silo = silos[typeof(TResourceType)];
			if (silo == null)
			{
				throw new InvalidOperationException(
				"Silo for resource type " + typeof(TResourceType).Name + " not found."
				);
			}
			return (Silo<TResourceType>)silo;
		}

		public override R<TResourceType> GetResource<TResourceType>()
		{
			return GetSilo<TResourceType>().Amount;

		}

		public override bool TrySetResource<TResourceType>(R<TResourceType> resource)
		{
			return GetSilo<TResourceType>().TrySetAmount(resource);
		}
		public override bool TryAddResource<TResourceType>(R<TResourceType> resource)
		{
			return GetSilo<TResourceType>().TryIncreaseAmount(resource);
		}
		public override bool TryLockResource<TResourceType>(R<TResourceType> resource)
		{
			return GetSilo<TResourceType>().TryLockResource(resource);
		}
		public override void UnlockResource<TResourceType>(R<TResourceType> resource)
		{
			GetSilo<TResourceType>().UnlockResource(resource);
		}
		public override bool TryBlockResourceCapacity<TResourceType>(R<Capacity<TResourceType>> resource)
		{
			return GetSilo<TResourceType>().TryBlockCapacity(resource);
		}
		public override void UnblockResourceCapacity<TResourceType>(R<Capacity<TResourceType>> resource)
		{
			GetSilo<TResourceType>().UnblockCapacity(resource);
		}
		public override void FillBlockedResourceCapacity<TResourceType>(R<TResourceType> resource)
		{
			GetSilo<TResourceType>().UseBlockedResourceCapacity(resource);
		}
		public override void RemoveLockedResource<TResourceType>(R<TResourceType> resource)
		{
			GetSilo<TResourceType>().UseLockedResource(resource);
		}
		public override void ForceIncreaseResources<TResourceType>(R<TResourceType> resource)
		{
			GetSilo<TResourceType>().ForceIncreaseResource(resource);
		}
		public override bool TrySetResourceCapacity<TResourceType>(R<Capacity<TResourceType>> resource)
		{
			return GetSilo<TResourceType>().TrySetCapacity(resource);
		}
	}
}
