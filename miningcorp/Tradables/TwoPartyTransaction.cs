using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Tradables
{
	public interface ITransactionItem : IDisposable
	{
		void ExecuteTransfer();
		void ReleaseResources();
	}
	public record class ResourceTransfer : ITransactionItem
	{
		public Trader _trom;
		public Trader _to;
		public Resource _resource;
		bool _resourceLocked = false;
		bool _capacityBlocked = false;
		bool _transferCompleted = false;
		bool _transferSetUpFailed = false;
		public bool TransferSetupFailed { get { return _transferSetUpFailed; } }

		public ResourceTransfer(Trader from, Trader to, Resource resource)
		{
			_trom = from;
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

		public void ExecuteTransfer()
		{
			lock (this)
			{
				if (_resourceLocked && _capacityBlocked && !_transferCompleted)
				{
					_trom.Stock.RemoveLockedResource(_resource);
					_to.Stock.FillBlockedResourceCapacity(_resource);
					_transferCompleted = true;
				}
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
						_trom.Stock.UnlockResource(_resource);
					}
					if (_capacityBlocked)
					{
						_to.Stock.UnblockResourceCapacity(_resource);
					}
					_transferCompleted = true;
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
	public record class PropertyTransfer : ITransactionItem
	{
		private Trader _from;
		private Trader _to;
		private Property _itemTraded;
		private bool _transferSetupFailed = false;
		private bool _transferCompleted = false;
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


	public enum TransactionDirection
	{
		FromBuyerToSeller,
		FromSellerToBuyer
	}
	public class TwoPartyTransaction
	{
		bool IsOk = true;
		Trader _buyer;
		Trader _seller;
		IList<ITransactionItem> Items;

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
				IsOk = false;
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
				IsOk = false;
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
				if (IsOk)
				{
					foreach (var item in Items)
					{
						item.ExecuteTransfer();
					}
					return true;
				}
				else
				{
					foreach (var item in Items)
					{
						item.ReleaseResources();
					}
					return false;
				}
			}

		}
	}
}
