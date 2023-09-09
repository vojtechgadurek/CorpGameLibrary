using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Transactions
{

	public class ResourceTransfer<TResourceType> : ITransactionItem where TResourceType : IResource
	{
		public Trader _from;
		public Trader _to;
		private LockedResource<R<TResourceType>>? _lockedResource;
		private Blocked<R<Capacity<TResourceType>>, R<TResourceType>>? _blockedCapacity;
		public R<TResourceType> AmountToTransfer { get; private set; }
		private bool _disposed = false;
		public bool Disposed { get => _disposed; }

		public bool TransferCompleted { get; private set; } = false;
		public ResourceTransfer(Trader from, Trader to, R<TResourceType> resource)
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
		public bool TryExecutePartialTransfer(R<TResourceType> resource)
		{
			lock (this)
			{

				if (!CheckSetupIsOk()) return false;

				var ansver = _lockedResource.TryGetPartial(resource); //This cannot be null
				if (!ansver.Item1) return false; //Resource requested was to large

				_blockedCapacity.Use(ansver.Item2); //This cannot be null


				AmountToTransfer -= ansver.Item2;
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
	}

}
