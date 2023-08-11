using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Persons
{
	public class Stock
	{
		public IDictionary<ResourceType, Silo> resources = new Dictionary<ResourceType, Silo>();
		public Resource GetResource(ResourceType resourceType)
		{
			return new Resource(resourceType, resources[resourceType].Amount);
		}
		public Stock()
		{
			foreach (var resource in Enum.GetValues<ResourceType>()) { resources.Add(resource, new Silo(10000)); };
		}
		public bool TrySetResource(Resource resource)
		{
			return resources[resource.Type].TrySetAmount(resource);
		}
		public bool TryAddResource(Resource resource)
		{
			return resources[resource.Type].TryIncreaseAmount(resource);
		}
		public bool TryLockResource(Resource resource)
		{
			return resources[resource.Type].TryLockResource(resource);
		}
		public void UnlockResource(Resource resource)
		{
			resources[resource.Type].UnlockResource(resource);
		}
		public bool TryBlockResourceCapacity(Resource resource)
		{
			return resources[resource.Type].TryBlockCapacity(resource);
		}
		public void UnblockResourceCapacity(Resource resource)
		{
			resources[resource.Type].UnblockCapacity(resource);
		}
		public void FillBlockedResourceCapacity(Resource resource)
		{
			resources[resource.Type].UseBlockedResourceCapacity(resource);
		}
		public void RemoveLockedResource(Resource resource)
		{
			resources[resource.Type].UseLockedResource(resource);
		}
		public void ForceIncreaseResources(Resource resource)
		{
			resources[resource.Type].ForceIncreaseResource(resource);
		}
		public bool TrySetResourceCapacity(Resource resource)
		{
			return resources[resource.Type].TrySetCapacity(resource);
		}
	}


	public class LimitedDouble
	{
		/// <summary>
		/// Limited double allows to set a lower and upper limit for a double value, which are than enforced.
		/// </summary>
		double _upperLimit;
		double _lowerLimit;
		double _value;
		public double LowerLimit
		{
			get => _lowerLimit;
		}
		public double UpperLimit
		{
			get => _upperLimit;
		}

		public double Value
		{
			get => _value;
		}

		public bool TestIfConformsToInvariants(double upperLimit, double value, double lowerLimit)
		{
			lock (this)
			{
				return upperLimit >= value && value >= lowerLimit;
			}
		}
		public bool TrySetNewValues(double newUpperLimit, double newValue, double newLowerLimits)
		{
			lock (this)
			{
				if (TestIfConformsToInvariants(newUpperLimit, newValue, newLowerLimits))
				{
					_upperLimit = newUpperLimit;
					_value = newValue;
					_lowerLimit = newLowerLimits;
					return true;
				}
				else
				{
					return false;
				}
			}
		}
		public LimitedDouble(double lowerLimit, double upperLimit, double value)
		{
			_lowerLimit = lowerLimit;
			_upperLimit = upperLimit;
			_value = value;
			if (TestIfConformsToInvariants(upperLimit, value, lowerLimit) == false)
			{
				throw new Exception("Invariants does not hold");
			}
		}

		public double IncreaseWithSpill(double amount)
		{
			lock (this)
			{
				if (_value + amount > _upperLimit)
				{
					double spill = _value + amount - _upperLimit;
					_value = _upperLimit;
					return spill;
				}
				else if (_value + amount < _lowerLimit)
				{
					double spill = -((amount + Value) - _lowerLimit);
					_value = _lowerLimit;
					return spill;
				}
				else
				{
					_value += amount;
					return 0;
				}
			}
		}
		public bool TryIncreaseUpperLimit(double amount)
		{
			return TrySetNewUpperLimit(_upperLimit + amount);
		}
		public bool TryIncreaseLowerLimit(double amount)
		{
			return TrySetNewLowerLimit(_lowerLimit + amount);
		}

		public bool TrySetNewLowerLimit(double newLowerLimit)
		{

			return TrySetNewValues(_upperLimit, _value, newLowerLimit);
		}

		public bool TrySetNewUpperLimit(double newUpperLimit)
		{
			return TrySetNewValues(newUpperLimit, _value, _lowerLimit);
		}

		public bool TryIncreaseValue(double amount)
		{
			return TrySetNewValue(_value + amount);
		}
		public bool TrySetNewValue(double newValue)
		{
			return TrySetNewValues(_upperLimit, newValue, _lowerLimit);
		}
	}

	public class Silo
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
		private LimitedDouble container;

		double _capacity;
		double _spill = 0; //This is quick fix for over and underfilling the silo
						   //Potencional solution is force market sale or buy or forced loan

		/// <summary>
		/// Shows how much resources are in the silo, locked and unlocked combined
		/// </summary>
		public double Amount
		{
			get => container.Value;
		}

		public double BlockedCapacity
		{
			get => _capacity - container.UpperLimit;
		}
		/// <summary>
		/// Shows how much resources are locked and are not available for general use 
		/// </summary>
		public double LockedResource
		{
			get => container.LowerLimit;
		}
		/// <summary>
		/// Show capacity not blocked or not used by reasources 
		/// </summary>
		public double FreeCapacity
		{
			get => container.UpperLimit - container.Value;
		}

		public Silo(double capacity)
		{
			_capacity = capacity;
			container = new LimitedDouble(0, capacity, 0);
		}
		/// <summary>
		/// Tries to set amount of resource in the silo, if it has enough capacity otherwise fails
		/// </summary>
		/// <param name="resource"></param>
		/// <returns></returns>
		public bool TrySetAmount(Resource resource)
		{
			return container.TrySetNewValue(resource.Amount);
		}
		/// <summary>
		/// Tries to increase amount of resource in the silo by the amount specified, if it has enough capacity otherwise fails. Amount may be negative.
		/// </summary>
		/// <param name="resource"></param>
		/// <returns></returns>
		public bool TryIncreaseAmount(Resource resource)
		{
			return container.TryIncreaseValue(resource.Amount);
		}
		/// <summary>
		/// Tries to block capacity for the resource, if it has enough capacity otherwise fails, amount can not be negative.
		/// </summary>
		/// <param name="resource"></param>
		/// <returns></returns>
		public bool TryBlockCapacity(Resource resource)
		{
			if (resource.Amount < 0)
			{
				throw new InvalidOperationException("You can not block negative capacity");
			}
			return container.TryIncreaseUpperLimit(-resource.Amount);
		}
		/// <summary>
		/// Unblock capacity for the resource, amount can not be negative.
		/// </summary>
		/// <param name="resource"></param>
		public void UnblockCapacity(Resource resource)
		{
			//Should never fail
			if (!container.TryIncreaseUpperLimit(resource.Amount))
			{
				throw new InvalidOperationException("You can not unblock capacity that was not blocked");
			}
			container.TryIncreaseUpperLimit(resource.Amount);
		}
		/// <summary>
		/// Locks resource, so it can not be used in normal interactions, amount can not be negative.
		/// </summary>
		/// <param name="resource"></param>
		/// <returns></returns>
		public bool TryLockResource(Resource resource)
		{
			if (resource.Amount < 0)
			{
				throw new InvalidOperationException("You can not lock negative amount of resource");
			}
			return container.TryIncreaseLowerLimit(resource.Amount);
		}
		/// <summary>
		/// Forces increase of resource in the silo, it may overfill or underfill the silo
		/// </summary>
		/// <param name="resource"></param>
		public void ForceIncreaseResource(Resource resource)
		{
			container.IncreaseWithSpill(resource.Amount);
		}
		/// <summary>
		/// Unlocks resource, so it can be used in normal interactions, amount can not be negative.
		/// </summary>
		/// <param name="resource"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public void UnlockResource(Resource resource)
		{
			if (resource.Amount < 0) throw new InvalidOperationException($"resource {resource.Amount} may not be negative");
			container.TryIncreaseLowerLimit(-resource.Amount);
			//Lower limit should not be negative and it means that to much resources was unlocked
			if (container.LowerLimit < 0) throw new InvalidOperationException("Lower limit was set to negative values");
		}

		/// <summary>
		/// Use previously blocked capacity, should be used only when capacity was blocked before, resource amount can not be negative.
		/// </summary>
		/// <param name="resource"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public void UseBlockedResourceCapacity(Resource resource)
		{
			lock (this)
			{
				if (resource.Amount < 0) throw new InvalidOperationException("Amount of resource to use blocked capacity should be positive");
				bool ok = true;
				ok &= container.TryIncreaseUpperLimit(resource.Amount);
				ok &= container.TryIncreaseValue(resource.Amount);
				if (!ok) throw new InvalidOperationException("Could not use blocked capacity");
			}
		}

		/// <summary>
		/// Use previously locked resource, should be used only when resource was locked before, resource amount can not be negative.
		/// </summary>
		/// <param name="resource"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public void UseLockedResource(Resource resource)
		{
			lock (this)
			{
				if (resource.Amount < -1) throw new InvalidOperationException("Amount of resource to use locked resources should be positive");
				bool ok = false;
				ok |= container.TryIncreaseLowerLimit(-resource.Amount);
				ok |= container.TryIncreaseValue(-resource.Amount);
				if (!ok) throw new InvalidOperationException("Could not use locked resources");
			}
		}
		public bool TrySetCapacity(Resource capacity)
		{
			_capacity = capacity.Amount;
			return container.TrySetNewUpperLimit(capacity.Amount);
		}
	}
}
