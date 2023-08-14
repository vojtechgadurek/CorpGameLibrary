using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Transactions
{
	public record class ResourceTransfer : ITransactionItem
	{
		public Trader _from;
		public Trader _to;
		public Resource Resource { get => _resource; }
		private Resource _resource;
		bool _resourceLocked = false;
		bool _capacityBlocked = false;
		bool _transferCompleted = false;
		public bool TransferCompleted { get { return _transferCompleted; } }
		bool _transferSetUpFailed = false;
		public bool TransferSetupFailed { get { return _transferSetUpFailed; } }

		public ResourceTransfer(Trader from, Trader to, Resource resource)
		{
			_from = from;
			_to = to;
			_resource = resource;
			_resourceLocked = from.Stock.TryLockResource(resource);
			_capacityBlocked = to.Stock.TryBlockResourceCapacity(resource);
			if (!_resourceLocked || !_capacityBlocked)
			{
				ReleaseResources();
				_transferSetUpFailed = true;
			}
		}

		public bool TryIncreaseTransferSize(Resource resource)
		{
			lock (this)
			{
				if (resource.Amount < 0) throw new InvalidOperationException("Resource amount must be positive");
				var resourceTransfer = new ResourceTransfer(_from, _to, resource);
				if (resourceTransfer.TransferSetupFailed)
				{
					resourceTransfer.ReleaseResources();
					return false;
				}
				_resource.Amount += resource.Amount;
				return true;
			}
		}

		public bool TryExecutePartialTransfer(Resource resource)
		{
			lock (this)
			{
				if ((resource.Amount > _resource.Amount)) return false;

				if (!(_resourceLocked && _capacityBlocked && !_transferCompleted)) return false;

				_resource -= resource;
				_from.Stock.RemoveLockedResource(resource);
				_to.Stock.FillBlockedResourceCapacity(resource);

				if (_resource.Amount == 0)
				{
					_transferCompleted = true;
				}
				return true;
			}

		}


		public void ExecuteTransfer()
		{
			lock (this)
			{
				TryExecutePartialTransfer(_resource);
			}
		}

		public void ReleaseResources()
		{
			lock (this)
			{
				if (!_transferCompleted)
				{
					if (_resourceLocked)
					{
						_from.Stock.UnlockResource(_resource);
					}
					if (_capacityBlocked)
					{
						_to.Stock.UnblockResourceCapacity(_resource);
					}
					_transferCompleted = true;
				}
			}
		}
	}

}
