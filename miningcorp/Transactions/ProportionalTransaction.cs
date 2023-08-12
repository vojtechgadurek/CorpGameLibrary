using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Transactions
{
	public class ProportionalTransaction
	{
		bool _setupFailed = false;
		public bool SetupFailed { get => _setupFailed; }
		bool _transactionCompleted = false;
		public bool TransactionCompleted { get => _transactionCompleted; }
		bool _resourcesReleased = false;
		public Resource FromSeller { get => _fromSeller.Resource; }
		public Resource FromBuyer { get => _fromBuyer.Resource; }
		ResourceTransfer _fromSeller;
		ResourceTransfer _fromBuyer;

		public ProportionalTransaction(Resource fromSeller, Resource fromBuyer, Trader seller, Trader buyer)
		{
			if (fromSeller.Type == fromBuyer.Type) _setupFailed = true;
			_fromSeller = new ResourceTransfer(seller, buyer, fromSeller);
			_fromBuyer = new ResourceTransfer(buyer, seller, fromBuyer);
			_setupFailed |= _fromSeller.TransferSetupFailed;
			_setupFailed |= _fromBuyer.TransferSetupFailed;

			if (_setupFailed) ReleaseResource();
		}

		public bool TryExecuteProportional(double proportion)
		{
			lock (this)
			{
				bool ansver = true;
				ansver |= _fromSeller.TryExecutePartialTransfer(_fromSeller.Resource * proportion);
				ansver |= _fromBuyer.TryExecutePartialTransfer(_fromBuyer.Resource * proportion);
				return ansver;
			}
		}

		public bool TryExecuteProportional(Resource resource)
		{
			lock (this)
			{

				Resource toCompareTo = (FromSeller.Type == resource.Type) ? FromSeller : FromBuyer;

				if (resource.Amount > toCompareTo.Amount) return false;

				bool ansver = TryExecuteProportional(resource / toCompareTo.Amount);

				//If one of the transfers is completed, the other one should be completed too
				//then transaction is completed
				if (_fromSeller.TransferCompleted || _fromSeller.TransferCompleted) CompleteTransaction();

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
