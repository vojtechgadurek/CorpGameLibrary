using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Transactions
{
	public class HoldedResourceManager<TResourceType>
	{

		protected Action<TResourceType> ReleaseDelegate;
		public HoldedResourceManager(Action<TResourceType> releaseDelegate)
		{
			this.ReleaseDelegate = releaseDelegate;
		}

	}
	public class LockedResourceManager<TResourceType> : HoldedResourceManager<TResourceType>
	{
		protected Action<TResourceType> TakeDelegate;

		public LockedResourceManager(Action<TResourceType> releaseDelegate, Action<TResourceType> takeDelegate) : base(releaseDelegate)
		{
			this.TakeDelegate = takeDelegate;
		}
		public Locked<TResourceType> CreateLocked(TResourceType amount)
		{
			var locked = new Locked<TResourceType>(amount, TakeDelegate, ReleaseDelegate);
			return locked;
		}
	}

	public class LockedResource_ResourceManager<TResource> : LockedResourceManager<R<TResource>> where TResource : IResource
	{
		public LockedResource_ResourceManager(Action<R<TResource>> releaseDelegate, Action<R<TResource>> takeDelegate) : base(releaseDelegate, takeDelegate)
		{
		}
		public LockedResource<TResource> CreateLockedResource(R<TResource> amount)
		{
			var locked = new LockedResource<TResource>(amount, TakeDelegate, ReleaseDelegate);
			return locked;
		}
	}
	public class BlockedResourceManager<TBlocked, TBlockedFor> : HoldedResourceManager<TBlocked>
	{
		protected Action<TBlockedFor> UseDelegate;
		public BlockedResourceManager(Action<TBlocked> releaseDelegate, Action<TBlockedFor> useDelegate) : base(releaseDelegate)
		{
			UseDelegate = useDelegate;
		}
		public Blocked<TBlocked, TBlockedFor> CreateBlocked(TBlocked amount)
		{
			var blocked = new Blocked<TBlocked, TBlockedFor>(amount, UseDelegate, ReleaseDelegate);
			return blocked;
		}

	}
	public class BlockedResourceCapacity_ResourceManager<TResource> : BlockedResourceManager<R<Capacity<TResource>>, R<TResource>> where TResource : IResource
	{
		public BlockedResourceCapacity_ResourceManager(Action<R<Capacity<TResource>>> releaseDelegate, Action<R<TResource>> useDelegate) : base(releaseDelegate, useDelegate)
		{
		}

		public BlockedResourceCapacity<TResource> CreateBlocked(R<Capacity<TResource>> amount)
		{
			var blocked = new BlockedResourceCapacity<TResource>(amount, UseDelegate, ReleaseDelegate);
			return blocked;
		}
	}
}
