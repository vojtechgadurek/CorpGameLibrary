using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Stocks
{
	public class Silo<TResourceType>
	{
		/// <summary>
		/// Silo is used for storing resources, it has a capacity and enforces it. It maybe be possible to overfill or underfill with force methods
		/// For normal interactions is expected to use try methods, if you want to be sure a such resource will fit the capacity.
		/// You can also lock resource via lock methods, which will prevent it from being used in normal interactions.
		/// Addionaly it may be good idea in some sort of transaction to have guranteed a capacity for such resource will be availible.
		/// Blocking and storing resources is not currently managed, so it is expected to be done by the functions.
		/// TODO
		/// It may be good idea to provide some sort of tracking for blocked resources, so it may be easier to debug etc.
		/// </summary>
		private LimitedDouble limitedDouble;

		//Implement locking and blocking
		private LockedResourceManager<TResourceType> lockedResourceManager;
		private BlockedResourceManager<TResourceType> blockedResourceManager;
		Action<R<TResourceType>> releaseResource;
		Action<R<TResourceType>> takeResource;
		Action<R<TResourceType>> useResource;


		R<Capacity<TResourceType>> _capacity;
		R<TResourceType> _spill = new R<TResourceType>(0); //This is quick fix for over and underfilling the silo
														   //Potencional solution is force market sale or buy or forced loan
		public Silo(R<Capacity<TResourceType>> capacity)
		{
			_capacity = capacity;
			limitedDouble = new LimitedDouble(0, capacity.Amount, 0);
			releaseResource = (R<TResourceType> resourceToRelease) => { UnlockResource(resourceToRelease); };
			takeResource = (R<TResourceType> resourceToTake) => { UseLockedResource(resourceToTake); };
			lockedResourceManager = new LockedResourceManager<TResourceType>(releaseResource, takeResource);
			useResource = (R<TResourceType> resourceToUse) => { UseBlockedResourceCapacity(resourceToUse); };
			blockedResourceManager = new BlockedResourceManager<TResourceType>(releaseResource, useResource);

		}
		/// <summary>
		/// Shows how much resources are in the silo, locked and unlocked combined
		/// </summary>
		public R<TResourceType> Amount
		{
			get => new R<TResourceType>(limitedDouble.Value);
		}

		public double BlockedCapacity
		{
			get => _capacity.Amount - limitedDouble.UpperLimit;
		}
		/// <summary>
		/// Shows how much resources are locked and are not available for general use 
		/// </summary>
		public double LockedResource
		{
			get => limitedDouble.LowerLimit;
		}
		/// <summary>
		/// Show capacity not blocked or not used by reasources 
		/// </summary>
		public double FreeCapacity
		{
			get => limitedDouble.UpperLimit - limitedDouble.Value;
		}

		/// <summary>
		/// Tries to set amount of resource in the silo, if it has enough capacity otherwise fails
		/// </summary>
		/// <param name="resource"></param>
		/// <returns></returns>
		public bool TrySetAmount(R<TResourceType> resource)
		{
			return limitedDouble.TrySetNewValue(resource.Amount);
		}
		/// <summary>
		/// Tries to increase amount of resource in the silo by the amount specified, if it has enough capacity otherwise fails. Amount may be negative.
		/// </summary>
		/// <param name="resource"></param>
		/// <returns></returns>
		public bool TryIncreaseAmount(R<TResourceType> resource)
		{
			return limitedDouble.TryIncreaseValue(resource.Amount);
		}
		/// <summary>
		/// Tries to block capacity for the resource, if it has enough capacity otherwise fails, amount can not be negative.
		/// </summary>
		/// <param name="resource"></param>
		/// <returns></returns>
		public bool TryBlockCapacity(R<Capacity<TResourceType>> resource)
		{
			if (resource.Amount < 0)
			{
				throw new InvalidOperationException("You can not block negative capacity");
			}
			return limitedDouble.TryIncreaseUpperLimit(-resource.Amount);
		}
		/// <summary>
		/// Unblock capacity for the resource, amount can not be negative.
		/// </summary>
		/// <param name="resource"></param>
		public void UnblockCapacity(R<Capacity<TResourceType>> resource)
		{
			//Should never fail
			if (!limitedDouble.TryIncreaseUpperLimit(resource.Amount))
			{
				throw new InvalidOperationException("You can not unblock capacity that was not blocked");
			}
			limitedDouble.TryIncreaseUpperLimit(resource.Amount);
		}
		/// <summary>
		/// Locks resource, so it can not be used in normal interactions, amount can not be negative.
		/// </summary>
		/// <param name="resource"></param>
		/// <returns></returns>
		public bool TryLockResource(R<TResourceType> resource)
		{
			if (resource.Amount < 0)
			{
				throw new InvalidOperationException("You can not lock negative amount of resource");
			}
			return limitedDouble.TryIncreaseLowerLimit(resource.Amount);
		}
		/// <summary>
		/// Forces increase of resource in the silo, it may overfill or underfill the silo
		/// </summary>
		/// <param name="resource"></param>
		public void ForceIncreaseResource(R<TResourceType> resource)
		{
			double spill = limitedDouble.IncreaseWithSpill(resource.Amount);
			if (spill > 0) HandleSpill(new R<TResourceType>(spill));
		}
		/// <summary>
		/// Unlocks resource, so it can be used in normal interactions, amount can not be negative.
		/// </summary>
		/// <param name="resource"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public void UnlockResource(R<TResourceType> resource)
		{
			if (resource.Amount < 0) throw new InvalidOperationException($"resource {resource.Amount} may not be negative");
			limitedDouble.TryIncreaseLowerLimit(-resource.Amount);
			//Lower limit should not be negative and it means that to much resources was unlocked
			if (limitedDouble.LowerLimit < 0) throw new InvalidOperationException("Lower limit was set to negative values");
		}

		/// <summary>
		/// Use previously blocked capacity, should be used only when capacity was blocked before, resource amount can not be negative.
		/// </summary>
		/// <param name="resource"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public void UseBlockedResourceCapacity(R<TResourceType> resource)
		{
			lock (this)
			{
				if (resource.Amount < 0) throw new InvalidOperationException("Amount of resource to use blocked capacity should be positive");
				bool ok = true;
				ok &= limitedDouble.TryIncreaseUpperLimit(resource.Amount);
				ok &= limitedDouble.TryIncreaseValue(resource.Amount);
				if (!ok) throw new InvalidOperationException("Could not use blocked capacity");
			}
		}

		/// <summary>
		/// Use previously locked resource, should be used only when resource was locked before, resource amount can not be negative.
		/// </summary>
		/// <param name="resource"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public void UseLockedResource(R<TResourceType> resource)
		{
			lock (this)
			{
				if (resource.Amount < 0) throw new InvalidOperationException("Amount of resource to use locked resources should be positive");
				bool ok = false;
				ok |= limitedDouble.TryIncreaseLowerLimit(-resource.Amount);
				ok |= limitedDouble.TryIncreaseValue(-resource.Amount);
				if (!ok) throw new InvalidOperationException("Could not use locked resources");
			}
		}
		public bool TrySetCapacity(R<Capacity<TResourceType>> capacity)
		{
			_capacity = capacity;
			return limitedDouble.TrySetNewUpperLimit(capacity.Amount);
		}

		public Locked<TResourceType>? TryGetLockOnResource(R<TResourceType> resource)
		{
			if (!TryLockResource(resource)) return null;
			return lockedResourceManager.CreateLocked(resource);
		}

		public Blocked<TResourceType>? TryGetBlockOnCapacity(R<TResourceType> capacity)
		{
			if (!TryBlockCapacity(capacity.GetCapacity())) return null;
			return blockedResourceManager.CreateBlocked(capacity);
		}

		public virtual void HandleSpill(R<TResourceType> spill)
		{
			_spill += spill;
		}
	}
}
