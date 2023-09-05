using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Stocks
{
	public static class StockFactory
	{
		public Stock CreateNormalPlayerStock(Player player, double BaseHardResourceCapacity, Bank bank, SpotMarket spotMarket)
		{
			IDictionary<Type, object> _silos = new Dictionary<Type, object>();
			foreach (var resource in Resources.Resources)
			{
				_silos.Add(resource, new Silo(resource, BaseHardResourceCapacity, bank, player, spotMarket));
			}

		}
	}
	public class Stock
	{
		protected IDictionary<Type, object> _silos;

		public Stock(IDictionary<Type, object> silos)
		{
			this._silos = silos;
		}

		private Silo<TResourceType> GetSilo<TResourceType>()
		{
			var silo = _silos[typeof(TResourceType)];
			if (silo == null)
			{
				throw new InvalidOperationException(
				"Silo for resource type " + typeof(TResourceType).Name + " not found."
				);
			}
			return (Silo<TResourceType>)silo;
		}

		public R<TResourceType> GetResource<TResourceType>()
		{
			return GetSilo<TResourceType>().Amount;

		}
		public bool TrySetResource<TResourceType>(R<TResourceType> resource)
		{
			return GetSilo<TResourceType>().TrySetAmount(resource);
		}
		public bool TryAddResource<TResourceType>(R<TResourceType> resource)
		{
			return GetSilo<TResourceType>().TryIncreaseAmount(resource);
		}
		public void ForceIncreaseResources<TResourceType>(R<TResourceType> resource)
		{
			GetSilo<TResourceType>().ForceIncreaseResource(resource);
		}
		public bool TrySetResourceCapacity<TResourceType>(R<Capacity<TResourceType>> resource)
		{
			return GetSilo<TResourceType>().TrySetCapacity(resource);
		}

		public Locked<R<TResourceType>>? TryGetLockOnResource<TResourceType>(R<TResourceType> amount)
		{
			return GetSilo<TResourceType>().TryGetLockOnResource(amount);
		}

		public Blocked<TResourceType>? TryGetBlockOnResourceCapacity<TResourceType>(R<TResourceType> amount)
		{
			return GetSilo<TResourceType>().TryGetBlockOnCapacity(amount);
		}
	}
}
