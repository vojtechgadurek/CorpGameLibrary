using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Tradables
{
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
}
