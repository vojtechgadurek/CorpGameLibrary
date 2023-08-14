using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameCorpLib.Tradables
{
	public struct Money
	{
		public Money(double amount)
		{
			Amount = amount;
		}
		public double Amount;
		public static implicit operator Money(R<Money> d) => new Money(d.Amount);
		public static implicit operator R<Money>(Money d) => new R<Money>(d.Amount);
	}
	public class Oil
	{

	}
	public class Capacity<TResource>
	{

	}
	public struct R<TResource>
	{
		public double Amount;
		public R(double amount)
		{
			Amount = amount;
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
		public override string ToString()
		{
			return Amount.ToString();
		}

		public R<Capacity<TResource>> GetCapacity()
		{
			return new R<Capacity<TResource>>(Amount);
		}

	}

}
public static class RExtensions
{

	public static bool WillFit<TResource>(this R<Capacity<TResource>> capacity, R<TResource> resourceHeld)
	{
		return capacity.Amount >= resourceHeld.Amount;
	}
}
public static class IntExtenskon
{
	public static R<TResource> Create<TResource>(this int number)
	{
		return new R<TResource>(number);
	}
	public static R<TResource> Create<TResource>(this double number)
	{
		return new R<TResource>(number);
	}
}
