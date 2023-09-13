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
			foreach (var tupleKeyResource in Resources.ResourcesList)
			{
				var resource = tupleKeyResource.Value;
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
				var type = typeof(SiloFactory);
				var method = type.GetMethod("CreateNoLimitsSilo");
				var GenericMethod = method.MakeGenericMethod(resource.Value.TheType);
				var silo = GenericMethod.Invoke(null, new object[] { });
				_silos.Add(
					resource.Value.TheType, silo)
					;
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

		private Silo<TResourceType> GetSilo<TResourceType>() where TResourceType : IResource
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

		public R<TResourceType> GetResource<TResourceType>() where TResourceType : IResource
		{
			return GetSilo<TResourceType>().Amount;

		}
		public bool TrySetResource<TResourceType>(R<TResourceType> resource) where TResourceType : IResource
		{
			return GetSilo<TResourceType>().TrySetAmount(resource);
		}
		public bool TryAddResource<TResourceType>(R<TResourceType> resource) where TResourceType : IResource
		{
			return GetSilo<TResourceType>().TryIncreaseAmount(resource);
		}
		public void ForceIncreaseResources<TResourceType>(R<TResourceType> resource) where TResourceType : IResource
		{
			GetSilo<TResourceType>().ForceIncreaseResource(resource);
		}
		public bool TrySetResourceCapacity<TResourceType>(R<Capacity<TResourceType>> resource) where TResourceType : IResource
		{
			return GetSilo<TResourceType>().TrySetCapacity(resource);
		}

		public LockedResource<TResourceType>? TryGetLockOnResource<TResourceType>(R<TResourceType> amount) where TResourceType : IResource
		{
			return GetSilo<TResourceType>().TryGetLockOnResource(amount);
		}

		public BlockedResourceCapacity<TResourceType>? TryGetBlockOnResourceCapacity<TResourceType>(R<Capacity<TResourceType>> amount) where TResourceType : IResource
		{
			return GetSilo<TResourceType>().TryGetBlockOnCapacity(amount);
		}

		public R<Capacity<TResourceType>> GetCapacity<TResourceType>() where TResourceType : IResource
		{
			return GetSilo<TResourceType>().Capacity;
		}
		public R<Capacity<TResourceType>> GetFreeCapacity<TResourceType>() where TResourceType : IResource
		{
			return GetSilo<TResourceType>().FreeCapacity;
		}

		public R<Capacity<TResourceType>> GetBlockedCapacity<TResourceType>() where TResourceType : IResource
		{
			return GetSilo<TResourceType>().BlockedCapacity;
		}
		public R<TResourceType> GetLockedResourceAmount<TResourceType>() where TResourceType : IResource
		{
			return GetSilo<TResourceType>().LockedResource;
		}


	}
}
