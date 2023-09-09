using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace GameCorpLib.Stocks
{

	public abstract class Silo { }
	public class Silo<TResourceType> : Silo where TResourceType : IResource
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
		/// 
		#region Constants
		readonly R<TResourceType> startingResources = new R<TResourceType>(0);
		readonly R<Capacity<TResourceType>> _floorCapacity = new R<Capacity<TResourceType>>(0);
		#endregion

		#region Data
		R<Capacity<TResourceType>> _capacity;
		private LimitedDouble _limitedDouble;
		#endregion

		private ISpillHandler<TResourceType> spillHandler;
		private IUnderfillHandler<TResourceType> underfillHandler;

		#region Blocking_and_locking
		private LockedResource_ResourceManager<R<TResourceType>> lockedResourceManager;
		private BlockedResourceManager<R<Capacity<TResourceType>>, R<TResourceType>> blockedResourceManager;
		Action<R<TResourceType>> releaseLockedResources;
		Action<R<Capacity<TResourceType>>> releaseBlockedCapacity;
		Action<R<TResourceType>> takeResource;
		Action<R<TResourceType>> consumeBlockedCapacity;
		#endregion

		#region Configuration 
		// Not implemented wholly
		bool _safeFill = true;
		#endregion



		public Silo(SiloConfiguration<TResourceType> siloConfiguration)
		{
			_capacity = siloConfiguration.Capacity ?? throw new InvalidOperationException("Capacity not set");
			//Create limited double
			_limitedDouble = new LimitedDouble(
				(siloConfiguration.FloorCapacity ?? _floorCapacity).Amount,
				_capacity.Amount,
				(siloConfiguration.Resource ?? startingResources).Amount
				);

			//Setup locking and blocking
			releaseLockedResources = (R<TResourceType> resourceToRelease) => { UnlockResource(resourceToRelease); };
			takeResource = (R<TResourceType> resourceToTake) => { UseLockedResource(resourceToTake); };
			lockedResourceManager = new LockedResource_ResourceManager<R<TResourceType>>(releaseLockedResources, takeResource);
			releaseBlockedCapacity = (R<Capacity<TResourceType>> resourceToRelease) => { UnblockCapacity(resourceToRelease); };
			consumeBlockedCapacity = (R<TResourceType> resourceToUse) => { UseBlockedResourceCapacity(resourceToUse); };
			blockedResourceManager = new BlockedResourceManager<R<Capacity<TResourceType>>, R<TResourceType>>(releaseBlockedCapacity, consumeBlockedCapacity);

			this.spillHandler = siloConfiguration.SpillHandler ?? new BasicSpillHandler<TResourceType>();
			this.underfillHandler = siloConfiguration.UnderfillHandler ?? new BasicSpillHandler<TResourceType>();
		}

		public void SetFillPolicy(bool safeFill)
		{
			_safeFill = safeFill;
		}
		/// <summary>
		/// Shows how much resources are in the silo, locked and unlocked combined
		/// </summary>
		public R<TResourceType> Amount
		{
			get => new R<TResourceType>(_limitedDouble.Value);
		}

		public R<Capacity<TResourceType>> BlockedCapacity
		{
			get => (_capacity.Amount - _limitedDouble.UpperLimit).Create<TResourceType>().ToCapacity();
		}
		/// <summary>
		/// Shows how much resources are locked and are not available for general use 
		/// </summary>
		public R<TResourceType> LockedResource
		{
			get => (_limitedDouble.LowerLimit).Create<TResourceType>();
		}
		/// <summary>
		/// Show capacity not blocked or not used by reasources 
		/// </summary>
		public R<Capacity<TResourceType>> FreeCapacity
		{
			get => (_limitedDouble.UpperLimit - _limitedDouble.Value).Create<TResourceType>().ToCapacity();
		}

		public R<Capacity<TResourceType>> Capacity => _capacity;

		/// <summary>
		/// Tries to set amount of resource in the silo, if it has enough capacity otherwise fails
		/// </summary>
		/// <param name="resource"></param>
		/// <returns></returns>
		public bool TrySetAmount(R<TResourceType> resource)
		{
			return _limitedDouble.TrySetNewValue(resource.Amount);
		}
		/// <summary>
		/// Tries to increase amount of resource in the silo by the amount specified, if it has enough capacity otherwise fails. Amount may be negative.
		/// </summary>
		/// <param name="resource"></param>
		/// <returns></returns>
		public bool TryIncreaseAmount(R<TResourceType> resource)
		{
			if (!_safeFill)
			{
				HandleSpill(_limitedDouble.IncreaseWithSpill(resource.Amount).Create<TResourceType>());
				return true;
			}
			return _limitedDouble.TryIncreaseValue(resource.Amount);
		}
		/// <summary>
		/// Tries to block capacity for the resource, if it has enough capacity otherwise fails, amount can not be negative.
		/// </summary>
		/// <param name="resource"></param>
		/// <returns></returns>
		bool TryBlockCapacity(R<Capacity<TResourceType>> resource)
		{
			if (resource.Amount < 0)
			{
				throw new InvalidOperationException("You can not block negative capacity");
			}
			return _limitedDouble.TryIncreaseUpperLimit(-resource.Amount);
		}
		/// <summary>
		/// Unblock capacity for the resource, amount can not be negative.
		/// </summary>
		/// <param name="resource"></param>
		void UnblockCapacity(R<Capacity<TResourceType>> resource)
		{
			//Should never fail
			if (!_limitedDouble.TryIncreaseUpperLimit(resource.Amount))
			{
				throw new InvalidOperationException("You can not unblock capacity that was not blocked");
			}
			_limitedDouble.TryIncreaseUpperLimit(resource.Amount);
		}
		/// <summary>
		/// Locks resource, so it can not be used in normal interactions, amount can not be negative.
		/// </summary>
		/// <param name="resource"></param>
		/// <returns></returns>
		bool TryLockResource(R<TResourceType> resource)
		{
			if (resource.Amount < 0)
			{
				throw new InvalidOperationException("You can not lock negative amount of resource");
			}
			return _limitedDouble.TryIncreaseLowerLimit(resource.Amount);
		}
		/// <summary>
		/// Forces increase of resource in the silo, it may overfill or underfill the silo
		/// </summary>
		/// <param name="resource"></param>
		public void ForceIncreaseResource(R<TResourceType> resource)
		{
			double spill = _limitedDouble.IncreaseWithSpill(resource.Amount);
			if (spill != 0) HandleSpill(new R<TResourceType>(spill));
		}
		/// <summary>
		/// Unlocks resource, so it can be used in normal interactions, amount can not be negative.
		/// </summary>
		/// <param name="resource"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		void UnlockResource(R<TResourceType> resource)
		{
			if (resource.Amount < 0) throw new InvalidOperationException($"resource {resource.Amount} may not be negative");
			_limitedDouble.TryIncreaseLowerLimit(-resource.Amount);
		}

		/// <summary>
		/// Use previously blocked capacity, should be used only when capacity was blocked before, resource amount can not be negative.
		/// </summary>
		/// <param name="resource"></param>
		/// <exception cref="InvalidOperationException"></exception>
		void UseBlockedResourceCapacity(R<TResourceType> resource)
		{
			lock (this)
			{
				if (resource.Amount < 0) throw new InvalidOperationException("Amount of resource to use blocked capacity should be positive");
				bool ok = true;
				ok &= _limitedDouble.TryIncreaseUpperLimit(resource.Amount);
				ok &= _limitedDouble.TryIncreaseValue(resource.Amount);
				if (!ok) throw new InvalidOperationException("Could not use blocked capacity");
			}
		}


		/// <summary>
		/// Use previously locked resource, should be used only when resource was locked before, resource amount can not be negative.
		/// </summary>
		/// <param name="resource"></param>
		/// <exception cref="InvalidOperationException"></exception>
		void UseLockedResource(R<TResourceType> resource)
		{
			lock (this)
			{
				if (resource.Amount < 0) throw new InvalidOperationException("Amount of resource to use locked resources should be positive");
				bool ok = false;
				ok |= _limitedDouble.TryIncreaseLowerLimit(-resource.Amount);
				ok |= _limitedDouble.TryIncreaseValue(-resource.Amount);
				if (!ok) throw new InvalidOperationException("Could not use locked resources");
			}
		}
		public bool TrySetCapacity(R<Capacity<TResourceType>> capacity)
		{
			lock (this)
			{
				bool ansver = _limitedDouble.TrySetNewUpperLimit(capacity.Amount);
				if (ansver) _capacity = capacity;
				return ansver;
			}
		}

		public LockedResource<R<TResourceType>>? TryGetLockOnResource(R<TResourceType> resource)
		{
			if (!TryLockResource(resource)) return null;
			return lockedResourceManager.CreateLockedResource(resource);
		}

		public Blocked<R<Capacity<TResourceType>>, R<TResourceType>>? TryGetBlockOnCapacity(R<Capacity<TResourceType>> capacity)
		{
			if (!TryBlockCapacity(capacity)) return null;
			return blockedResourceManager.CreateBlocked(capacity);
		}

		public void HandleSpill(R<TResourceType> spill)
		{
			if (spill > 0.Create<TResourceType>()) { spillHandler.HandleSpill(spill); } else { underfillHandler.HandleUnderfill(-spill); }
		}
	}
}
