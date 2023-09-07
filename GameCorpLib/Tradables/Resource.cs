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
	public interface IVector<T>
	{
		public IVector<T> Add(T value);
		public IVector<T> Subtract(T value);
		public IVector<T> ScalarMultiply(double value);
		public bool IsZero();
	}

	public interface IOrderedVector<T> : IComparable<T>, IVector<T>
	{

	}

	public interface IHardResource : IResource
	{

	}
	public interface ICash : IResource
	{

	}

	public interface IResource
	{

	}
	public enum TypeOfResourceType
	{
		HardResource,
		Cash
	}
	public class ResourceAttribute : System.Attribute
	{
		public TypeOfResourceType TypeOfResourceType { get; private set; }
		public string Name { get; private set; }
		public Type TheType { get; private set; } //I do not have any idea how to name it normaly
		public ResourceAttribute(string name, TypeOfResourceType typeOfResourceType, Type type)
		{
			Name = name;
			TypeOfResourceType = typeOfResourceType;
			TheType = type;


		}
	}
	static class Resources
	{
		public static IList<ResourceAttribute> ResourcesList = new List<ResourceAttribute>();
		static Resources()
		{
			foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
			{
				var resourceAttribute = type.GetCustomAttribute<ResourceAttribute>();
				if (resourceAttribute != null)
				{
					ResourcesList.Add(resourceAttribute);
				}
			}
		}

	}
	[Resource("Money", TypeOfResourceType.Cash, typeof(Money))]
	public struct Money : ICash
	{
		public Money(double amount)
		{
			Amount = amount;
		}
		public double Amount;
		public static implicit operator Money(R<Money> d) => new Money(d.Amount);
		public static implicit operator R<Money>(Money d) => new R<Money>(d.Amount);
	}
	[Resource("Oil", TypeOfResourceType.HardResource, typeof(Oil))]
	public struct Oil : IHardResource
	{

	}
	public struct Capacity<TResource> : IResource
	{
	}
	public struct R<TResource> : IOrderedVector<R<TResource>>
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
