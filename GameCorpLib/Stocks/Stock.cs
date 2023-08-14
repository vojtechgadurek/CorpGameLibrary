using GameCorpLib.Tradables;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Stocks
{
	//Stock by asi chtělo přepsat

	public abstract class Stock
	{
		public abstract R<TResourceType> GetResource<TResourceType>();

		public abstract bool TrySetResource<TResourceType>(R<TResourceType> resource);

		public abstract bool TryAddResource<TResourceType>(R<TResourceType> resource);

		public abstract bool TryLockResource<TResourceType>(R<TResourceType> resource);

		public abstract void UnlockResource<TResourceType>(R<TResourceType> resource);

		public abstract bool TryBlockResourceCapacity<TResourceType>(R<Capacity<TResourceType>> resource);

		public abstract void UnblockResourceCapacity<TResourceType>(R<Capacity<TResourceType>> resource);

		public abstract void FillBlockedResourceCapacity<TResourceType>(R<TResourceType> resource);

		public abstract void RemoveLockedResource<TResourceType>(R<TResourceType> resource);

		public abstract void ForceIncreaseResources<TResourceType>(R<TResourceType> resource);
		public abstract bool TrySetResourceCapacity<TResourceType>(R<Capacity<TResourceType>> resource);

	}
}












