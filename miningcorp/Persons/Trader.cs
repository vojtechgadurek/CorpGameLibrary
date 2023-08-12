using GameCorpLib.Stocks;
using GameCorpLib.Transactions;
using System.Collections;
using System.Transactions;

namespace GameCorpLib
{
	public class Trader
	{

		public Stock Stock;
		public Properties Properties = new Properties();

		public Trader(Stock stock)
		{
			Stock = stock;
		}
		public double Money { get => Stock.GetResource(ResourceType.Money).Amount; }
		public double Oil { get => Stock.GetResource(ResourceType.Oil).Amount; }

		public void ExchangeResource(Resource resource, Trader from, Trader to)
		{
			new ResourceTransfer(from, to, resource).ExecuteTransfer();
		}
		public bool TryDoTrade(Resource fromBuyer, Resource fromSeller, Trader buyer, Trader seller)
		{
			return new TwoPartyTransaction(buyer, seller)
				.AddTransactionItem(fromBuyer, TransactionDirection.FromBuyerToSeller)
				.AddTransactionItem(fromSeller, TransactionDirection.FromSellerToBuyer)
				.TryExecute();
		}
	}

	public class PrivilegedTrader : Trader
	{
		/// <summary>
		/// It used for mocking normal user behaviour, but every operation succed,
		/// </summary>
		public PrivilegedTrader() : base(new MagicalStock())
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
