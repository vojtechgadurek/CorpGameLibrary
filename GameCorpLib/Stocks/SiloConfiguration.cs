using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Stocks
{
	public class SiloConfiguration<TResourceType> where TResourceType : IResource
	{
		public R<Capacity<TResourceType>>? Capacity { get; private set; } = null;
		public R<TResourceType>? Resource { get; private set; } = null;
		public ISpillHandler<TResourceType>? SpillHandler { get; private set; } = null;
		public IUnderfillHandler<TResourceType>? UnderfillHandler { get; private set; } = null;
		public R<Capacity<TResourceType>>? FloorCapacity { get; private set; } = null;

		public SiloConfiguration<TResourceType> SetCapacity(R<Capacity<TResourceType>> capacity)
		{
			Capacity = capacity;
			return this;
		}
		public SiloConfiguration<TResourceType> SetStartingResource(R<TResourceType> resource)
		{
			Resource = resource;
			return this;
		}

		public SiloConfiguration<TResourceType> SetSpillHandler(ISpillHandler<TResourceType> spillHandler)
		{
			SpillHandler = spillHandler;
			return this;
		}
		public SiloConfiguration<TResourceType> SetUnderfillHandler(IUnderfillHandler<TResourceType> underfillHandler)
		{
			UnderfillHandler = underfillHandler;
			return this;
		}



		SiloConfiguration<TResourceType> SetFloorCapacity(R<Capacity<TResourceType>> floorCapacity)
		{
			//It is important to remember setting floor capacity to other value then 0 will not reflect in change of capacity
			//Floar -1, capacity 1, means capacity will still be 1
			FloorCapacity = floorCapacity;
			return this;
		}

		public SiloConfiguration<TResourceType> SetInfiniteCapacity()
		{
			return SetCapacity(new R<Capacity<TResourceType>>(double.PositiveInfinity));
		}

		public SiloConfiguration<TResourceType> SetNoFloarCapacity()
		{
			SetFloorCapacity(new R<Capacity<TResourceType>>(double.NegativeInfinity));
			return this;
		}

		public SiloConfiguration<TResourceType> SetNoLimits()
		{
			return SetInfiniteCapacity().SetNoFloarCapacity();
		}

		public Silo<TResourceType> CreateSiloInstance()
		{
			return new Silo<TResourceType>(this);
		}
	}
}
