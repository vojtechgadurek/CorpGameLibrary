using System.Data;

namespace GameCorpLib.Tradables
{
	public interface ITransactionItem
	{
		void ExecuteTransfer();
		void ReleaseResources();
	}

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
				if (!(resource.Amount > _resource.Amount)) return false;

				if (!(_resourceLocked && _capacityBlocked && !_transferCompleted)) return false;

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
	public record class PropertyTransfer : ITransactionItem
	{
		private Trader _from;
		private Trader _to;
		private Property _itemTraded;
		private bool _transferSetupFailed = false;
		private bool _transferCompleted = false;
		public bool TransferCompleted { get { return _transferCompleted; } }
		private bool _propertyLocked = false;
		public bool TransferSetupFailed { get { return _transferSetupFailed; } }

		public PropertyTransfer(Trader from, Trader to, Property itemTraded)
		{
			_from = from;
			_to = to;
			_itemTraded = itemTraded;
			if (itemTraded.Owner != from)
			{
				_transferSetupFailed = true;
				return;
			}
			if (!itemTraded.TryLockForTrade()) { _transferSetupFailed = true; return; };
			_propertyLocked = true;
		}

		public void ExecuteTransfer()
		{
			lock (this)
			{
				if (!_transferSetupFailed && !_transferCompleted)
				{
					_itemTraded.ChangeOwner(_to);
					_transferCompleted = true;
				}
			}
		}
		public void ReleaseResources()
		{
			lock (this)
			{
				if (_propertyLocked)
				{
					_itemTraded.UnlockFromTrade();
				}
			}
		}
		public void Dispose()
		{
			if (!_transferCompleted)
			{
				ReleaseResources();
			}
		}


	}


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

	public class TransactionUtils
	{



	}

	public enum TransactionDirection
	{
		FromBuyerToSeller,
		FromSellerToBuyer
	}

	public class TwoPartyTransaction
	{
		bool _isOk = true;
		bool _completed = false;
		bool _resourcesReleased = false;
		public bool IsOk { get { return _isOk; } }
		Trader _buyer;
		Trader _seller;
		protected IList<ITransactionItem> Items;

		public TwoPartyTransaction(Trader buyer, Trader seller)
		{
			_buyer = buyer;
			_seller = seller;
			Items = new List<ITransactionItem>();
		}

		TwoPartyTransaction AddTransactionItem(Resource resource, Trader from, Trader to)
		{
			var resourceTransfer = new ResourceTransfer(from, to, resource);
			if (resourceTransfer.TransferSetupFailed)
			{
				_isOk = false;
			}
			Items.Add(resourceTransfer);
			return this;
		}
		void SetFromAndTo(out Trader from, out Trader to, TransactionDirection transactionDirection)
		{
			if (transactionDirection == TransactionDirection.FromBuyerToSeller)
			{
				from = _buyer;
				to = _seller;
			}
			else
			{
				from = _seller;
				to = _buyer;
			}
		}
		public TwoPartyTransaction AddTransactionItem(Resource resource, TransactionDirection transactionDirection)
		{
			Trader from;
			Trader to;
			SetFromAndTo(out from, out to, transactionDirection);
			return AddTransactionItem(resource, from, to);
		}
		TwoPartyTransaction AddTransactionItem(Trader from, Trader to, Property property)
		{
			var propertyTransaction = new PropertyTransfer(from, to, property);
			if (propertyTransaction.TransferSetupFailed)
			{
				_isOk = false;
			}
			Items.Add(propertyTransaction);
			return this;
		}

		public TwoPartyTransaction AddTransactionItem(Property property, TransactionDirection transactionDirection)
		{
			Trader from;
			Trader to;
			SetFromAndTo(out from, out to, transactionDirection);
			return AddTransactionItem(from, to, property);
		}

		public bool TryExecute()
		{
			lock (this)
			{
				if (_completed) return false;
				if (_isOk)
				{
					foreach (var item in Items)
					{
						item.ExecuteTransfer();
					}
					return true;
				}
				else
				{
					ReleaseResources();
					return false;
				}
			}

		}

		public void ReleaseResources()
		{
			lock (this)
			{
				if (_resourcesReleased) return;

				foreach (var item in Items)
				{
					item.ReleaseResources();
				}
				_resourcesReleased = true;
			}
		}
	}
}
