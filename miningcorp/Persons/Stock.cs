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
			return new Resource(resourceType, resources[resourceType].Amount));
		}
		public Stock()
		{
			foreach (var resource in Enum.GetValues<ResourceType>()) { resources.Add(resource, new Silo(10000)); };
		}

		public bool TryChangeResource(Resource resource)
		{
			return resources[resource.Type].TryChangeAmount(resource);
		}
		public bool TryLockResource(Resource resource)
		{
			return resources[resource.Type].TryLockResource(resource);
		}
		public bool UnlockResource(Resource resource)
		{
			return resources[resource.Type].TryUnlockResource(resource);
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



		public LimitedDouble(double lowerLimit, double upperLimit, double value)
		{
			_lowerLimit = lowerLimit;
			_upperLimit = upperLimit;
			_value = value;
		}

		public bool TryIncreaseUpperLimit(double amount)
		{
			return TrySetNewUpperLimit(_upperLimit + amount);
		}
		public bool TryIncreaseLowerLimit(double amount)
		{
			return TrySetNewUpperLimit(_lowerLimit + amount);
		}

		public bool TrySetNewLowerLimit(double newLowerLimit)
		{
			lock (this)
			{
				if (newLowerLimit > _upperLimit) { return false; }
				_lowerLimit = newLowerLimit;
				return true;
			}
		}

		public bool TrySetNewUpperLimit(double newUpperLimit)
		{
			lock (this)
			{
				if (newUpperLimit < _lowerLimit) { return false; }
				_upperLimit = newUpperLimit;
				return true;
			}
		}

		public bool TryIncreaseValue(double amount)
		{
			return TrySetNewValue(_value + amount);
		}
		public bool TrySetNewValue(double newValue)
		{
			lock (this)
			{
				if (newValue > _upperLimit || newValue < _lowerLimit) { return false; }
				_value = newValue;
				return true;
			}
		}
	}

	public class Silo
	{
		private LimitedDouble container;

		double _capacity;
		public double Amount
		{
			get => container.Value;
		}

		public Silo(double capacity)
		{
			_capacity = capacity;
			container = new LimitedDouble(0, capacity, 0);
		}
		public bool TryChangeAmount(Resource resource)
		{
			return container.TryIncreaseValue(resource.Amount);
		}
		public bool TryBlockCapacity(Resource resource)
		{
			return container.TryIncreaseUpperLimit(-resource.Amount);
		}
		public void UnblockCapacity(Resource resource)
		{
			//Should never fail
			container.TryIncreaseUpperLimit(resource.Amount);
		}
		public bool TryLockResource(Resource resource)
		{
			return container.TryIncreaseLowerLimit(resource.Amount);
		}

		public bool TryUnlockResource(Resource resource)
		{
			bool ansver = container.TryIncreaseLowerLimit(-resource.Amount);
			//Lower limit should not be negative and it means that to much resources was unlocked
			if (container.LowerLimit < 0) throw new InvalidOperationException("Lower limit was set to negative values");
			return ansver;
		}

		/// <summary>
		/// Used previously blocked capacity, should be used only when capacity was blocked before
		/// </summary>
		/// <param name="resource"></param>
		/// <exception cref="InvalidOperationException"></exception>
		public void UseBlockedResourceCapacity(Resource resource)
		{
			lock (this)
			{
				if (resource.Amount < 0) throw new InvalidOperationException("Amount of resource to use blocked capacity should be positive");
				bool ok = false;
				ok |= container.TryIncreaseUpperLimit(resource.Amount);
				ok |= container.TryIncreaseValue(_capacity);
				if (!ok) throw new InvalidOperationException("Could not use blocked capacity");
			}
		}

		public void UseLockedResource(Resource resource)
		{
			lock (this)
			{
				if (resource.Amount < 0) throw new InvalidOperationException("Amount of resource to use locked resources should be positive");
				bool ok = false;
				ok |= container.TryIncreaseLowerLimit(-resource.Amount);
				ok |= container.TryIncreaseValue(-resource.Amount);
				if (!ok) throw new InvalidOperationException("Could not use locked resources");
			}
		}
	}
}
