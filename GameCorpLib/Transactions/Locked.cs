﻿using GameCorpLib.Tradables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Transactions
{
	public class HoldedResourceManager<TResourceType>
	{

		protected HashSet<Holded<TResourceType>> _holdedResource = new HashSet<Holded<TResourceType>>();
		private Action<TResourceType> releaseDelegate;
		public HoldedResourceManager(Action<TResourceType> releaseDelegate)
		{
			this.releaseDelegate = releaseDelegate;
		}

		public void Release(Holded<TResourceType> holded, TResourceType holdedAmount)
		{
			lock (this)
			{
				if (!_holdedResource.Contains(holded)) throw new InvalidOperationException("Holded resource not found");
				releaseDelegate.Invoke(holdedAmount);
				_holdedResource.Remove(holded);
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
			_holdedResource.Add(locked);
			return locked;
		}
		public void Take(Locked<TResourceType> locked)
		{
			lock (this)
			{
				if (!_holdedResource.Contains(locked)) throw new InvalidOperationException("Locked resource not found");
				takeDelegate.Invoke(locked.Amount);
				_holdedResource.Remove(locked);
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
			_holdedResource.Add(locked);
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
				if (!_holdedResource.Contains(locked)) throw new InvalidOperationException("Locked resource not found");
				takeDelegate.Invoke(amountToUse);
				//This may not be best idea, but is it like 12:00 and I'm tired
				if (locked.Amount.IsZero())
				{
					_holdedResource.Remove(locked);
				}
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
			_holdedResource.Add(blocked);
			return blocked;
		}

		public void Use(Blocked<TBlocked, TBlockedFor> blockedCapacity, TBlockedFor resource)
		{
			lock (this)
			{
				if (!_holdedResource.Contains(blockedCapacity)) throw new InvalidOperationException("Blocked resource not found");
				useDelegate.Invoke(resource);
				_holdedResource.Remove(blockedCapacity);
				return;
			}
		}

	}

	public class Holded<TResourceType>
	{
		protected HoldedResourceManager<TResourceType> _resourceOwner;

		protected private TResourceType _resourceHeld;
		public TResourceType Amount => _resourceHeld;
		protected bool _disposed = false;
		public bool Disposed => _disposed;
		public Holded(TResourceType amount)
		{
			_resourceHeld = amount;
		}

		public void Release()
		{
			lock (this)
			{
				if (_disposed) return;
				_resourceOwner.Release(this, _resourceHeld);
				_disposed = true;
			}
		}
	}

	public class Blocked<TBlocked, TBlockedFor> : Holded<TBlocked>
	{
		public TBlocked Amount => _resourceHeld;
		public Blocked(TBlocked amount, BlockedResourceManager<TBlocked, TBlockedFor> resourceowner) : base(amount)
		{
			_resourceOwner = resourceowner;
		}

		public void Use(TBlockedFor blockageToUse)
		{
			lock (this)
			{
				if (_disposed) throw new InvalidOperationException("Blocked resource was already disposed of");
				((BlockedResourceManager<TBlocked, TBlockedFor>)_resourceOwner).Use(this, blockageToUse);
			}
		}
	}
	public class Locked<TResourceType> : Holded<TResourceType>
	{

		public Locked(TResourceType amount, LockedResourceManager<TResourceType> resourceOwner) : base(amount)
		{
			_resourceOwner = resourceOwner;
		}
		public TResourceType Get()
		{
			lock (this)
			{
				if (_disposed) throw new InvalidOperationException("Locked reource was already disposed of");
				_disposed = true;
				((LockedResourceManager<TResourceType>)_resourceOwner).Take(this);
				return _resourceHeld;
			}
		}
	}

	public class LockedResource<TResource> : Locked<TResource> where TResource : IOrderedVector<TResource>
	{
		public LockedResource(TResource amount, LockedResourceManager<TResource> resourceOwner) : base(amount, resourceOwner)
		{
		}



		public (bool, TResource?) TryGetPartial(TResource amountToGet)
		{
			lock (this)
			{
				if (amountToGet.CompareTo(_resourceHeld) == 1) return (false, default(TResource));


				((LockedResource_ResourceManager<TResource>)_resourceOwner).PartialTake(this, amountToGet);
				//
				_resourceHeld = (TResource)_resourceHeld.Subtract(amountToGet);


				//Dispose if is empty
				if (_resourceHeld.IsZero()) Release();

				return (true, amountToGet);

			}
		}

		public TResource GetPartial(double proportion)
		{
			if (proportion is < 0 or > 1) throw new ArgumentOutOfRangeException("Proportion must be between 0 and 1");

			lock (this)
			{
				if (_disposed) throw new InvalidOperationException("Locked resource was already disposed of");
				//Null should not be returned
				return TryGetPartial((TResource)_resourceHeld.ScalarMultiply(proportion)).Item2 ?? throw new InvalidOperationException("Null was returned and this is not expected behavior according to contract of the function TryGetPartial(TResource amountToGet)");
			}

		}
	}
}
