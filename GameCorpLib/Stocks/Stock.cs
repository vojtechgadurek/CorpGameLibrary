using GameCorpLib.Markets;
using GameCorpLib.Tradables;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Stocks
{
	public static class StockFactory
	{
		public static Stock CreateNormalPlayerStock(Player player, double BaseHardResourceCapacity, Bank bank, SpotMarket spotMarket)
		{
			IDictionary<Type, object> _silos = new Dictionary<Type, object>();
			foreach (var resource in Resources.ResourcesList)
			{
				if (resource.TypeOfResourceType == TypeOfResourceType.Cash)
				{
					if (resource.TheType != typeof(Money)) throw new NotImplementedException("This would require additional generalization of bank etc");
					_silos.Add(resource.TheType, SiloFactory.CreateCashSilo(player, bank));
				}
				else if (resource.TypeOfResourceType == TypeOfResourceType.HardResource)
				{
					_silos.Add(
						resource.TheType,
						 typeof(SiloFactory)
						.GetMethod("CreateHardResourceSilo", new Type[] { typeof(double), typeof(Player), typeof(SpotMarket) })
						.MakeGenericMethod(resource.TheType).Invoke(null, new object[] { BaseHardResourceCapacity, player, spotMarket })
					);
				}
				else
				{
					throw new InvalidOperationException($"Not expected type {resource.TheType}");
				}
			}
			return new Stock(_silos);
		}

		public static Stock CreatePriviligedTraderStock()
		{
			IDictionary<Type, object> _silos = new Dictionary<Type, object>();
			foreach (var resource in Resources.ResourcesList)
			{
				_silos.Add(
					resource.TheType,
					typeof(SiloFactory)
					.GetMethod("CreateNoLimitSilo", new Type[] { typeof(Type) })
					.MakeGenericMethod(resource.TheType)
					.Invoke(null, new object[] { })
					);
			}
			return new Stock(_silos);
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

		public LockedResource<R<TResourceType>>? TryGetLockOnResource<TResourceType>(R<TResourceType> amount)
		{
			return GetSilo<TResourceType>().TryGetLockOnResource(amount);
		}

		public Blocked<R<Capacity<TResourceType>>, R<TResourceType>>? TryGetBlockOnResourceCapacity<TResourceType>(R<Capacity<TResourceType>> amount)
		{
			return GetSilo<TResourceType>().TryGetBlockOnCapacity(amount);
		}
	}
}
