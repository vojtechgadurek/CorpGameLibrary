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
