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
	public record class ResourceTransfer
	{
		public Trader From;
		public Trader To;
		public Resource Resource;
		public bool IsCompleted = false;
		ResourceTransfer(Trader from, Trader to, Resource resource)
		{
			From = from;
			To = to;
			Resource = resource;
		}
		public ResourceTransfer? TryCreateResourceTransfer(Trader from, Trader to, Resource resource)
		{
			if (from.Stock.TryLock(resource)) return this; else return null;
		}
		public bool TryComplte()
		{
			lock (To.Stock)
			{
				if (To.Stock.TryChangeResources(Resource))
				{
					IsCompleted = true;
					return true;
				}
				return false;
			}
		}
	}
	public interface ITradable
	{
		void ChangeOwner(Trader newOwner);
		bool TryLockForTrade();
		void UnlockFromTrade();
	}
	public record class TransactionItem : IDisposable
	{
		private Trader _from;
		private Trader _to;
		private ITradable _itemTraded;

		public static TransactionItem? TryCreateTransactionItem(Trader from, Trader to, Resource itemTraded)
		{

		}
		public static TransactionItem? TryCreateTransactionItem(Trader from, Trader to, ITradable itemTraded)
		{
			if (itemTraded.TryLockForTrade())
			{
				return new TransactionItem(from, to, itemTraded);
			}
			return null;
		}
		private TransactionItem(Trader from, Trader to, ITradable itemTraded)
		{
			_from = from;
			_to = to;
			_itemTraded = itemTraded;
		}
		public void Dispose()
		{
			_itemTraded.UnlockFromTrade();
		}
	}

	public class Maybe<T> where T : class
	{
		T? Value;
		public static Maybe<T> Empty = GetEmpty();
		public Maybe(T value)
		{
			Value = value;
		}
		public Maybe()
		{
			Value = null;
		}
		static public Maybe<T> GetEmpty() => new Maybe<T>();
		public Maybe<T> Bind(T input, Func<T, T, T?> action)
		{
			if (Value is not null)
			{
				Value = action.Invoke(Value, input);
			}
			return this;
		}
	}


	public class Transaction
	{
		static public Transaction Create => new Transaction();
		bool IsOk = true;
		IList<TransactionItem> Items;

		Transaction TryAddResourceTransaction(Trader from, Trader to, Resource resource)
		{
			return TryAddItem(TransactionItem.TryCreateTransactionItem(from, to, resource));
		}
		Transaction TryAddPropertyTransaction(Trader from, Trader to, Property property)
		{
			return TryAddItem(TransactionItem.TryCreateTransactionItem(from, to, property));
		}
		Transaction TryAddItem(TransactionItem? item)
		{
			if (item == null) IsOk = false; else Items.Add(item);
			return this;
		}
		bool TryExecute()
		{
			foreach (var item in Items)
			{
				if (!item.ItemTraded.TryLockForTrade()) return false;
			}
			foreach (var item in Items)
			{
				item.ItemTraded.ChangeOwner(item.Buyer);
			}
			foreach (var item in Items)
			{
				item.ItemTraded.UnlockFromTrade();
			}
			return true;
		}
	}
}
