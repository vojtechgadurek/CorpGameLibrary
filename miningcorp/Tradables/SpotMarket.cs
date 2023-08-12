using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Tradables
{
	public enum SpotMarketOfferType
	{
		Sell,
		Buy
	}
	public class SpotMarketOffer
	{
		SpotMarketOfferType offerType;
		ProportionalTransaction proportionalTransaction;
		Resource _price;
		public Resource Price { get => _price; }

		SpotMarketOffer(SpotMarketOfferType offerType, Resource price, ProportionalTransaction proportionalTransaction)
		{
			this.offerType = offerType;
			_price = price;
			this.proportionalTransaction = proportionalTransaction;
		}

		public static SpotMarketOffer CreateSellOffer(Resource forSale, Resource price, Trader market, Trader seller)
		{
			return new SpotMarketOffer(SpotMarketOfferType.Sell, price, new ProportionalTransaction(forSale, price * forSale.Amount, seller, market));
		}
		public static SpotMarketOffer CreateBuyOffer(Resource toBuy, Resource price, Trader market, Trader buyer)
		{
			return new SpotMarketOffer(SpotMarketOfferType.Buy, price, new ProportionalTransaction(price * toBuy.Amount, toBuy, market, buyer));
		}

	}

	public class SpotMarket
	{
		SortedDictionary<Resource, SpotMarketOffer> sellOffers = new SortedDictionary<Resource, SpotMarketOffer>();
		SortedDictionary<Resource, SpotMarketOffer> buyOffers = new SortedDictionary<Resource, SpotMarketOffer>();
	}
}
