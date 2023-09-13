using GameCorpLib.Tradables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Transactions
{

	public class Holded<TResourceType>
	{
		protected Action<TResourceType> ReleaseDelegate;

		protected private TResourceType _resourceHeld;
		public TResourceType Amount => _resourceHeld;
		protected bool _disposed = false;
		public bool Disposed => _disposed;
		public Holded(TResourceType amount, Action<TResourceType> releaseDelegate)
		{
			_resourceHeld = amount;
			ReleaseDelegate = releaseDelegate;
		}

		public void Release()
		{
			lock (this)
			{
				if (_disposed) return;
				ReleaseDelegate.Invoke(_resourceHeld);
				_disposed = true;
			}
		}
	}

	public class Blocked<TBlocked, TBlockedFor> : Holded<TBlocked>
	{
		public TBlocked Amount => _resourceHeld;
		protected Action<TBlockedFor> UseDelegate;
		public Blocked(TBlocked amount, Action<TBlockedFor> useDelegate, Action<TBlocked> releaseDelegate) : base(amount, releaseDelegate)
		{
			UseDelegate = useDelegate;
		}

		public void Use(TBlockedFor blockageToUse)
		{
			lock (this)
			{
				if (_disposed) throw new InvalidOperationException("Blocked resource was already disposed of");
				UseDelegate.Invoke(blockageToUse);
				_disposed = true;
			}
		}
	}

	public class BlockedResourceCapacity<TResourceType> : Blocked<R<Capacity<TResourceType>>, R<TResourceType>> where TResourceType : IResource
	{

		public BlockedResourceCapacity(R<Capacity<TResourceType>> amount, Action<R<TResourceType>> useDelegate, Action<R<Capacity<TResourceType>>> releaseDelegate) : base(amount, useDelegate, releaseDelegate)
		{
		}
		public bool PartialUse(R<TResourceType> blockageToUse)
		{
			lock (this)
			{
				if (_disposed) return false;
				if (_resourceHeld < blockageToUse.ToCapacity()) return false;
				UseDelegate.Invoke(blockageToUse);
				_resourceHeld -= blockageToUse.ToCapacity();
				if (_resourceHeld.IsZero()) _disposed = true;
				return true;
			}
		}
	}
	public class Locked<TResourceType> : Holded<TResourceType>
	{
		protected Action<TResourceType> TakeDelegate;
		public Locked(TResourceType amount, Action<TResourceType> takeDelegate, Action<TResourceType> releaseDelegate) : base(amount, releaseDelegate)
		{
			TakeDelegate = takeDelegate;
		}
		public TResourceType Get()
		{
			lock (this)
			{
				if (_disposed) throw new InvalidOperationException("Locked reource was already disposed of");
				_disposed = true;
				TakeDelegate.Invoke(_resourceHeld);
				return _resourceHeld;
			}
		}
	}

	public class LockedResource<TResource> : Locked<R<TResource>> where TResource : IResource
	{
		public LockedResource(R<TResource> amount, Action<R<TResource>> takeDelegate, Action<R<TResource>> releaseDelegate) : base(amount, takeDelegate, releaseDelegate)
		{
		}



		public R<TResource>? TryGetPartial(R<TResource> amountToGet)
		{
			lock (this)
			{
				if (amountToGet > _resourceHeld) return null;
				//
				_resourceHeld -= amountToGet;
				TakeDelegate.Invoke(amountToGet);

				//Dispose if is empty
				if (_resourceHeld.IsZero()) Release();

				return amountToGet;

			}
		}

		public R<TResource> GetPartial(double proportion)
		{
			if (proportion is < 0 or > 1) throw new ArgumentOutOfRangeException("Proportion must be between 0 and 1");

			lock (this)
			{
				if (_disposed) throw new InvalidOperationException("Locked resource was already disposed of");
				//Null should not be returned
				return TryGetPartial(_resourceHeld * proportion) ?? throw new InvalidOperationException("Null was returned from TryGetPartial()");
			}

		}
	}
}
