using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Transactions
{
	public class HoldedResourceManager<TResourceType>
	{

		private Action<TResourceType> releaseDelegate;
		public HoldedResourceManager(Action<TResourceType> releaseDelegate)
		{
			this.releaseDelegate = releaseDelegate;
		}

		public void Release(Holded<TResourceType> holded, TResourceType holdedAmount)
		{
			lock (this)
			{
				releaseDelegate.Invoke(holdedAmount);
			}
		}

	}
	public class LockedResourceManager<TResourceType> : HoldedResourceManager<TResourceType>
	{
		protected Action<TResourceType> takeDelegate;

		public LockedResourceManager(Action<TResourceType> releaseDelegate, Action<TResourceType> takeDelegate) : base(releaseDelegate)
		{
			this.takeDelegate = takeDelegate;
		}
		public Locked<TResourceType> CreateLocked(TResourceType amount)
		{
			var locked = new Locked<TResourceType>(amount, this);
			return locked;
		}
		public void Take(Locked<TResourceType> locked)
		{
			lock (this)
			{
				takeDelegate.Invoke(locked.Amount);
			}
		}
	}

	public class LockedResource_ResourceManager<TResource> : LockedResourceManager<TResource> where TResource : IOrderedVector<TResource>
	{
		public LockedResource_ResourceManager(Action<TResource> releaseDelegate, Action<TResource> takeDelegate) : base(releaseDelegate, takeDelegate)
		{
		}
		public LockedResource<TResource> CreateLockedResource(TResource amount)
		{
			var locked = new LockedResource<TResource>(amount, this);
			return locked;
		}
		public void PartialTake(LockedResource<TResource> locked, TResource amountToUse)
		{
			//This is extreme bullshit, 
			//like I need to switch who is master
			//and who is slave
			lock (this)
			{
				if (amountToUse.CompareTo(locked.Amount) == 1) throw new InvalidOperationException($"Taken {amountToUse} to large to {locked.Amount}");
				takeDelegate.Invoke(amountToUse);
				//This may not be best idea, but is it like 12:00 and I'm tired
			}
		}
	}

	public class BlockedResourceManager<TBlocked, TBlockedFor> : HoldedResourceManager<TBlocked>
	{
		private Action<TBlockedFor> useDelegate;
		public BlockedResourceManager(Action<TBlocked> releaseDelegate, Action<TBlockedFor> useDelegate) : base(releaseDelegate)
		{
			this.useDelegate = useDelegate;
		}
		public Blocked<TBlocked, TBlockedFor> CreateBlocked(TBlocked amount)
		{
			var blocked = new Blocked<TBlocked, TBlockedFor>(amount, this);
			return blocked;
		}

		public void Use(Blocked<TBlocked, TBlockedFor> blockedCapacity, TBlockedFor resource)
		{
			lock (this)
			{
				useDelegate.Invoke(resource);
				return;
			}
		}

	}
}
