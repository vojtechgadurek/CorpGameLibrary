using System.Data;

namespace GameCorpLib.Transactions
{

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

		TwoPartyTransaction AddTransactionItem<TResourceType>(R<TResourceType> resource, Trader from, Trader to)
		{
			var resourceTransfer = new ResourceTransfer<TResourceType>(from, to, resource);
			if (resourceTransfer.Disposed)
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
		public TwoPartyTransaction AddTransactionItem<TResourceType>(R<TResourceType> resource, TransactionDirection transactionDirection)
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
