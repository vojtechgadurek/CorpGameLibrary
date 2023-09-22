using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Transactions
{

	public class ResourceTransfer<TResourceType> : ITransactionItem where TResourceType : IResource
	{
		public ITrader _from;
		public ITrader _to;
		private LockedResource<TResourceType>? _lockedResource;
		private BlockedResourceCapacity<TResourceType>? _blockedCapacity;
		public R<TResourceType> AmountToTransfer { get; private set; }
		private bool _disposed = false;
		public bool Disposed { get => _disposed; }

		public bool TransferCompleted { get; private set; } = false;
		public ResourceTransfer(ITrader from, ITrader to, R<TResourceType> resource)
		{
			_from = from;
			_to = to;

			//It is not possible to lock negative amount of resources
			//So the solution is to switch direction
			if (resource < 0.Create<TResourceType>())
			{
				_from = to;
				_to = from;
				resource = -resource;
			}

			_lockedResource = _from.Stock.TryGetLockOnResource(resource);
			_blockedCapacity = _to.Stock.TryGetBlockOnResourceCapacity(resource.ToCapacity());
			AmountToTransfer = resource;
			if (!CheckSetupIsOk()) ReleaseResources();
		}

		public bool CheckSetupIsOk()
		{
			return _lockedResource != null && _blockedCapacity != null && !Disposed;
		}

		public void ReleaseResources()
		{
			lock (this)
			{
				_lockedResource?.Release();
				_blockedCapacity?.Release();
				_disposed = true;
			}
		}
		void CompleteTrade()
		{
			TransferCompleted = true;
			ReleaseResources();
		}

		public void ExecutePartialTransfer(double proportion)
		{
			if (proportion < 0 || proportion > 1) throw new InvalidOperationException("Proportion must be between 0 and 1");


			lock (this)
			{
				TryExecutePartialTransfer(AmountToTransfer * proportion);
			}
		}
		public bool TryExecutePartialTransfer(double proportion)
		{
			if (proportion < 0 || proportion > 1) throw new InvalidOperationException("Proportion must be between 0 and 1");


			lock (this)
			{
				return TryExecutePartialTransfer(AmountToTransfer * proportion);
			}
		}


		public bool TryExecutePartialTransfer(R<TResourceType> resource)
		{
			lock (this)
			{

				if (!CheckSetupIsOk()) return false;

				var ansver = _lockedResource.TryGetPartial(resource); //This cannot be null
				if (ansver is null) return false; //Resource requested was to large

				R<TResourceType> resourcesFromLock = (R<TResourceType>)ansver;
				_blockedCapacity.PartialUse(resourcesFromLock); //This cannot be null


				AmountToTransfer -= resourcesFromLock;
				if (AmountToTransfer == 0.Create<TResourceType>()) CompleteTrade();
				if (AmountToTransfer < 0.Create<TResourceType>()) throw new InvalidOperationException("Amount to transfer cannot be negative");

				return true;

			}
		}

		public void ExecuteTransfer()
		{
			lock (this)
			{
				ExecutePartialTransfer(1);
			}
		}

		public bool TryExecuteTransfer()
		{
			lock (this)
			{
				return TryExecutePartialTransfer(1);
			}
		}
	}

}
