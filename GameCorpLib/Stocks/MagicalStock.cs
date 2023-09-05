using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Stocks
{
	public class MagicalStock : Stock
	{
		public MagicalStock() { 
		}


		public override R<TResourceType> GetResource<TResourceType>()
		{
			return new R<TResourceType>(double.PositiveInfinity);
		}

		public override bool TrySetResource<TResourceType>(R<TResourceType> resource)
		{
			return true;
		}
		public override bool TryAddResource<TResourceType>(R<TResourceType> resource)
		{
			return true;
		}
		public override bool TryLockResource<TResourceType>(R<TResourceType> resource)
		{
			return true;
		}
		public override void UnlockResource<TResourceType>(R<TResourceType> resource)
		{
		}
		public override bool TryBlockResourceCapacity<TResourceType>(R<Capacity<TResourceType>> resource)
		{
			return true;
		}
		public override void UnblockResourceCapacity<TResourceType>(R<Capacity<TResourceType>> resource)
		{

		}
		public override void FillBlockedResourceCapacity<TResourceType>(R<TResourceType> resource)
		{
		}
		public override void RemoveLockedResource<TResourceType>(R<TResourceType> resource)
		{
		}
		public override void ForceIncreaseResources<TResourceType>(R<TResourceType> resource)
		{
		}
		public override bool TrySetResourceCapacity<TResourceType>(R<Capacity<TResourceType>> resource)
		{
			return true;
		}

		public override Locked<R<TResourceType>> TryGetLockOnResource<TResourceType>(R<TResourceType> amount)
		{
			throw new NotImplementedException();
		}

		public override Blocked<TResourceType> TryGetBlockOnResourceCapacity<TResourceType>(R<Capacity<TResourceType>> amount)
		{
			throw new NotImplementedException();
		}
	}
}
