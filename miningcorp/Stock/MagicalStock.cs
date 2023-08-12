using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Stock
{
	public class MagicalStock : Stock
	{
		public virtual Resource GetResource(ResourceType resourceType)
		{
			return new Resource(resourceType, double.PositiveInfinity);
		}

		public virtual bool TrySetResource(Resource resource)
		{
			return true;
		}
		public virtual bool TryAddResource(Resource resource)
		{
			return true;
		}
		public virtual bool TryLockResource(Resource resource)
		{
			return true;
		}
		public virtual void UnlockResource(Resource resource)
		{
		}
		public virtual bool TryBlockResourceCapacity(Resource resource)
		{
			return true;
		}
		public virtual void UnblockResourceCapacity(Resource resource)
		{

		}
		public virtual void FillBlockedResourceCapacity(Resource resource)
		{
		}
		public virtual void RemoveLockedResource(Resource resource)
		{
		}
		public virtual void ForceIncreaseResources(Resource resource)
		{
		}
		public virtual bool TrySetResourceCapacity(Resource resource)
		{
			return true;
		}
	}
}
