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

		public void Release(Holded<TResourceType> locked, TResourceType lockedAmount)
		{
			lock (this)
			{
				if (!_holdedResource.Contains(locked)) throw new InvalidOperationException("Locked resource not found");
				releaseDelegate.Invoke(lockedAmount);
				_holdedResource.Remove(locked);
			}
		}

	}
	public class LockedResourceManager<TResourceType> : HoldedResourceManager<TResourceType>
	{
		private Action<TResourceType> takeDelegate;

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
		public void Take(Locked<TResourceType> locked, TResourceType lockedAmount)
		{
			lock (this)
			{
				if (!_holdedResource.Contains(locked)) throw new InvalidOperationException("Locked resource not found");
				takeDelegate.Invoke(lockedAmount);
				_holdedResource.Remove(locked);
			}
		}
	}

	public class BlockedResourceManager<TBlocked> : HoldedResourceManager<R<TBlocked>>
	{
		private Action<R<TBlocked>> useDelegate;
		public BlockedResourceManager(Action<R<TBlocked>> releaseDelegate, Action<R<TBlocked>> useDelegate) : base(releaseDelegate)
		{
			this.useDelegate = useDelegate;
		}
		public Blocked<TBlocked> CreateBlocked(R<TBlocked> amount)
		{
			var blocked = new Blocked<TBlocked>(amount, this);
			_holdedResource.Add(blocked);
			return blocked;
		}

		public void Use(Blocked<TBlocked> _blockedCapacity, R<TBlocked> resource)
		{
			lock (this)
			{
				if (!_holdedResource.Contains(_blockedCapacity)) throw new InvalidOperationException("Blocked resource not found");
				useDelegate.Invoke(resource);
				_holdedResource.Remove(_blockedCapacity);
				return;
			}
		}

	}

	public class Holded<TResourceType>
	{
		protected HoldedResourceManager<TResourceType> _resourceOwner;

		protected private TResourceType _amount;
		protected bool _disposed = false;
		public bool Disposed => _disposed;
		public Holded(TResourceType amount)
		{
			_amount = amount;
		}

		public void Release()
		{
			lock (this)
			{
				if (_disposed) return;
				_resourceOwner.Release(this, _amount);
				_disposed = true;
			}
		}
	}

	public class Blocked<TResourceType> : Holded<R<TResourceType>>
	{
		public R<TResourceType> Amount => _amount;
		public Blocked(R<TResourceType> amount, BlockedResourceManager<TResourceType> resourceowner) : base(amount)
		{
			_resourceOwner = resourceowner;
		}

		public void Use(R<TResourceType> blockageToUse)
		{
			lock (this)
			{
				if (_disposed) throw new InvalidOperationException("Blocked resource was already disposed of");
				if (blockageToUse > _amount) throw new InvalidOperationException($"{blockageToUse} is to big for space of {_amount}");
				_disposed = true;
				((BlockedResourceManager<TResourceType>)_resourceOwner).Use(this, blockageToUse);
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
				((LockedResourceManager<TResourceType>)_resourceOwner).Take(this, _amount);
				return _amount;
			}
		}
	}
}
