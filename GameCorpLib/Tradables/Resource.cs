using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameCorpLib.Tradables
{


	public struct R<TResource> : IOrderedVector<R<TResource>> where TResource : IResource
	{
		public double Amount;
		public R(double amount)
		{
			Amount = amount;
		}
		public bool IsZero()
		{
			return Amount == 0;
		}


		public static R<TResource> operator +(R<TResource> a, R<TResource> b)
		{
			return new R<TResource>(a.Amount + b.Amount);
		}
		public static R<TResource> operator -(R<TResource> a, R<TResource> b)
		{
			return new R<TResource>(a.Amount - b.Amount);
		}
		public static R<TResource> operator *(R<TResource> a, double b)
		{
			return new R<TResource>(a.Amount * b);
		}
		public static R<TResource> operator *(double a, R<TResource> b)
		{
			return new R<TResource>(a * b.Amount);
		}
		public static R<TResource> operator /(R<TResource> a, double b)
		{
			return new R<TResource>(a.Amount / b);
		}

		public static bool operator >=(R<TResource> a, R<TResource> b)
		{
			return a.Amount >= b.Amount;
		}
		public static R<TResource> operator -(R<TResource> a)
		{
			return new R<TResource>(-a.Amount);
		}
		public static bool operator <=(R<TResource> a, R<TResource> b)
		{
			return a.Amount <= b.Amount;
		}

		public static bool operator >(R<TResource> a, R<TResource> b)
		{
			return a.Amount > b.Amount;
		}

		public static bool operator <(R<TResource> a, R<TResource> b)
		{
			return a.Amount < b.Amount;
		}

		public static bool operator ==(R<TResource> a, R<TResource> b)
		{
			return a.Amount == b.Amount;
		}

		public static bool operator !=(R<TResource> a, R<TResource> b)
		{
			return a.Amount != b.Amount;
		}
		public override string ToString()
		{
			return Amount.ToString();
		}

		public R<Capacity<TResource>> ToCapacity()
		{
			return new R<Capacity<TResource>>(Amount);
		}

		public int CompareTo(R<TResource> other)
		{
			if (this > other)
			{
				return 1;
			}
			else if (this < other)
			{
				return -1;
			}
			else
			{
				return 0;
			}
		}

		public IVector<R<TResource>> Add(R<TResource> value)
		{
			return this + value;
		}

		public IVector<R<TResource>> Subtract(R<TResource> value)
		{
			return this - value;
		}

		public IVector<R<TResource>> ScalarMultiply(double value)
		{
			return this * value;
		}
	}

}
public static class RExtensions
{

	public static bool WillFit<TResource>(this R<Capacity<TResource>> capacity, R<TResource> resourceHeld) where TResource : IResource
	{
		return capacity.Amount >= resourceHeld.Amount;
	}
}
public static class IntExtenskon
{
	public static R<TResource> Create<TResource>(this int number) where TResource : IResource
	{
		return new R<TResource>(number);
	}
	public static R<TResource> Create<TResource>(this double number) where TResource : IResource
	{
		return new R<TResource>(number);
	}
}
