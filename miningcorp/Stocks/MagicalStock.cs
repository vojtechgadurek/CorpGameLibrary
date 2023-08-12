using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Stocks
{
	public class MagicalStock : Stock
	{
		public override Resource GetResource(ResourceType resourceType)
		{
			return new Resource(resourceType, double.PositiveInfinity);
		}

		public override bool TrySetResource(Resource resource)
		{
			return true;
		}
		public override bool TryAddResource(Resource resource)
		{
			return true;
		}
		public override bool TryLockResource(Resource resource)
		{
			return true;
		}
		public override void UnlockResource(Resource resource)
		{
		}
		public override bool TryBlockResourceCapacity(Resource resource)
		{
			return true;
		}
		public override void UnblockResourceCapacity(Resource resource)
		{

		}
		public override void FillBlockedResourceCapacity(Resource resource)
		{
		}
		public override void RemoveLockedResource(Resource resource)
		{
		}
		public override void ForceIncreaseResources(Resource resource)
		{
		}
		public override bool TrySetResourceCapacity(Resource resource)
		{
			return true;
		}
	}
}
