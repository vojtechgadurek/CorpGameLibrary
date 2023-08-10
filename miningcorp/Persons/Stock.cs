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
		public Stock()
		{
			foreach (var resource in Enum.GetValues<ResourceType>()) { resources.Add(resource, new Silo()); };
		}

		public bool TryChangeResources(Resource resource)
		{
			return resources[resource.Type].TryChangeAmount(resource);
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

		public Silo(double capacity)
		{
			_capacity = capacity;
			container = new LimitedDouble(0, capacity, 0);
		}
		public bool TryChangeAmount(Resource resource)
		{
			return container.TryIncreaseValue(resource.Amount);
		}
		public bool TryBlockCapacity(double amount)
		{
			return container.TryIncreaseUpperLimit(-amount);
		}
		public bool TryUnblockCapacity(double amount)
		{
			return container.TryIncreaseUpperLimit(amount);
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


	}
}
