using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Stock
{
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
					double spill = -(amount + Value - _lowerLimit);
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
}
