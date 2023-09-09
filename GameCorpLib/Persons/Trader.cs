using GameCorpLib.Stocks;
using GameCorpLib.Transactions;
using System.Collections;
using System.Transactions;
using GameCorpLib.Persons;

namespace GameCorpLib
{
	public class Trader : ITrader
	{

		public Stock Stock { get; private set; }
		public Properties Properties = new Properties();

		public Trader(Stock stock)
		{
			Stock = stock;
		}
		public R<Money> Money { get => Stock.GetResource<Money>(); }
		public R<Oil> Oil { get => Stock.GetResource<Oil>(); }

		public bool TryDoTrade<TResourceTypeFromBuyer, TResourceTypeFromSeller>(R<TResourceTypeFromBuyer> fromBuyer, R<TResourceTypeFromSeller> fromSeller, Trader buyer, Trader seller) where TResourceTypeFromBuyer : IResource where TResourceTypeFromSeller : IResource
		{
			return new TwoPartyTransaction(buyer, seller)
				.AddTransactionItem(fromBuyer, TransactionDirection.FromBuyerToSeller)
				.AddTransactionItem(fromSeller, TransactionDirection.FromSellerToBuyer)
				.TryExecute();
		}
	}

	public class PrivilegedTrader : Trader
	{
		public PrivilegedTrader() : base(StockFactory.CreatePriviligedTraderStock())
		{
		}

	}



	public class Properties
	{
		private IDictionary<int, Property> _value = new Dictionary<int, Property>();


		public IEnumerable<Property> GetEnumerable()
		{
			return _value.Values;
		}
		public void Remove(Property property)
		{
			_value.Remove(property.Id);
		}
		public void Add(Property property)
		{
			_value.Add(property.Id, property);
		}
	}
}
