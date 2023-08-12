using GameCorpLib.Tradables;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Stock
{
	//Stock by asi chtělo přepsat




	public abstract class Stock
	{
		public IDictionary<ResourceType, Silo> resources = new Dictionary<ResourceType, Silo>();

		public virtual Resource GetResource(ResourceType resourceType)
		{
			return new Resource(resourceType, resources[resourceType].Amount);
		}

		public virtual bool TrySetResource(Resource resource)
		{
			return resources[resource.Type].TrySetAmount(resource);
		}
		public virtual bool TryAddResource(Resource resource)
		{
			return resources[resource.Type].TryIncreaseAmount(resource);
		}
		public virtual bool TryLockResource(Resource resource)
		{
			return resources[resource.Type].TryLockResource(resource);
		}
		public virtual void UnlockResource(Resource resource)
		{
			resources[resource.Type].UnlockResource(resource);
		}
		public virtual bool TryBlockResourceCapacity(Resource resource)
		{
			return resources[resource.Type].TryBlockCapacity(resource);
		}
		public virtual void UnblockResourceCapacity(Resource resource)
		{
			resources[resource.Type].UnblockCapacity(resource);
		}
		public virtual void FillBlockedResourceCapacity(Resource resource)
		{
			resources[resource.Type].UseBlockedResourceCapacity(resource);
		}
		public virtual void RemoveLockedResource(Resource resource)
		{
			resources[resource.Type].UseLockedResource(resource);
		}
		public virtual void ForceIncreaseResources(Resource resource)
		{
			resources[resource.Type].ForceIncreaseResource(resource);
		}
		public virtual bool TrySetResourceCapacity(Resource resource)
		{
			return resources[resource.Type].TrySetCapacity(resource);
		}
	}












