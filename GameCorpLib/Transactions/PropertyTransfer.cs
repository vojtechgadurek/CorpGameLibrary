using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Transactions
{
	public record class PropertyTransfer : ITransactionItem
	{
		private Trader _from;
		private Trader _to;
		private Property _itemTraded;
		public string Error { get; private set; }
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
}
