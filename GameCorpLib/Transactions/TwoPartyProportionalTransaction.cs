using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Transactions
{
	public class TwoPartyProportionalTransaction<TFromSellerResourceType, TFromBuyerResourceType> where TFromSellerResourceType : IResource where TFromBuyerResourceType : IResource
	{
		bool _setupFailed = false;
		public bool SetupFailed { get => _setupFailed; }
		bool _transactionCompleted = false;
		public bool TransactionCompleted { get => _transactionCompleted; }
		bool _resourcesReleased = false;
		public R<TFromSellerResourceType> FromSeller { get => _fromSeller.AmountToTransfer; }
		public R<TFromBuyerResourceType> FromBuyer { get => _fromBuyer.AmountToTransfer; }
		ResourceTransfer<TFromSellerResourceType> _fromSeller;
		ResourceTransfer<TFromBuyerResourceType> _fromBuyer;

		public TwoPartyProportionalTransaction(R<TFromSellerResourceType> fromSeller, R<TFromBuyerResourceType> fromBuyer, Trader seller, Trader buyer)
		{
			if (fromSeller.GetType() == fromBuyer.GetType()) throw new InvalidOperationException("Resource types must be different");
			_fromSeller = new ResourceTransfer<TFromSellerResourceType>(seller, buyer, fromSeller);
			_fromBuyer = new ResourceTransfer<TFromBuyerResourceType>(buyer, seller, fromBuyer);
			_setupFailed |= _fromSeller.Disposed;
			_setupFailed |= _fromBuyer.Disposed;

			if (_setupFailed) CompleteTransaction();
		}

		public bool TryExecuteProportional(double proportion)
		{
			if (_transactionCompleted || _resourcesReleased) return false;
			if (proportion > 1 || proportion < 0) return false;
			lock (this)
			{
				bool ansver = true;
				ansver |= _fromSeller.TryExecutePartialTransfer(_fromSeller.AmountToTransfer * proportion);
				ansver |= _fromBuyer.TryExecutePartialTransfer(_fromBuyer.AmountToTransfer * proportion);
				if (_fromSeller.TransferCompleted || _fromSeller.TransferCompleted) CompleteTransaction();
				return ansver;
			}
		}

		public bool TryExecuteProportional<TResourceType>(R<TResourceType> resource) where TResourceType : IResource
		{
			lock (this)
			{

				double toCompareTo = FromSeller.GetType() == resource.GetType() ? FromSeller.Amount : FromBuyer.Amount;

				if (resource.Amount > toCompareTo) return false;

				bool ansver = TryExecuteProportional(resource.Amount / toCompareTo);

				//If one of the transfers is completed, the other one should be completed too
				//then transaction is completed

				return ansver;
			}
		}

		void CompleteTransaction()
		{
			_transactionCompleted = true;
			ReleaseResource();
		}

		public void ReleaseResource()
		{
			if (!_resourcesReleased)
			{
				_fromSeller.ReleaseResources();
				_fromBuyer.ReleaseResources();
				_resourcesReleased = true;
			}
		}

	}

}
