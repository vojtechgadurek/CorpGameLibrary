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
		SpotMarketOfferType _offerType;
		ProportionalTransaction _proportionalTransaction;
		Resource _price;
		Resource _resourceTraded;
		public Resource ResourceTraded { get => _resourceTraded; }
		public Resource Price { get => _price; }

		SpotMarketOffer(SpotMarketOfferType offerType, Resource price, Resource resourceTraded, ProportionalTransaction proportionalTransaction)
		{
			this._offerType = offerType;
			_price = price;
			_resourceTraded = resourceTraded;
			this._proportionalTransaction = proportionalTransaction;
		}

		public void ReleaseResources()
		{
			_proportionalTransaction.ReleaseResource();
		}

		public bool TryFill(Resource resource)
		{
			return _proportionalTransaction.TryExecuteProportional(resource);
		}

		public static SpotMarketOffer CreateSellOffer(Resource forSale, Resource price, Trader market, Trader seller)
		{
			return new SpotMarketOffer(SpotMarketOfferType.Sell, price, forSale, new ProportionalTransaction(forSale, price * forSale.Amount, seller, market));
		}
		public static SpotMarketOffer CreateBuyOffer(Resource toBuy, Resource price, Trader market, Trader buyer)
		{
			return new SpotMarketOffer(SpotMarketOfferType.Buy, price, toBuy, new ProportionalTransaction(price * toBuy.Amount, toBuy, market, buyer));
		}

	}

	public class SpotMarket : PriviligedTrader
	{
		public SortedSet<SpotMarketOffer> sellOffers = new SortedSet<SpotMarketOffer>();
		public SortedSet<SpotMarketOffer> buyOffers = new SortedSet<SpotMarketOffer>();

		class SpotMarketComparer : IComparer<SpotMarketOffer>
		{
			public int Compare(SpotMarketOffer x, SpotMarketOffer y)
			{
				int ansver = x.Price.Amount.CompareTo(y.Price.Amount);
				if (ansver == 0) return x.GetHashCode().CompareTo(y.GetHashCode());
				return ansver;
			}
		}
		public void RemoveTradeOffer(SortedSet<SpotMarketOffer> list, SpotMarketOffer spotMarketOffer)
		{
			lock (this)
			{
				list.Remove(spotMarketOffer);
				spotMarketOffer.ReleaseResources();
			}
		}
		public bool TryCompleteTrade()
		{
			lock (this)
			{
				var lowestSellPriceOffer = sellOffers.First();
				var highestBuyPriceOffer = buyOffers.Last();
				if (lowestSellPriceOffer.Price > highestBuyPriceOffer.Price)
				{
					return false;
				}
				Resource resource =
					lowestSellPriceOffer.ResourceTraded < highestBuyPriceOffer.ResourceTraded ?
					lowestSellPriceOffer.ResourceTraded : highestBuyPriceOffer.ResourceTraded;
				lowestSellPriceOffer.TryFill(resource);
				highestBuyPriceOffer.TryFill(resource);
				if (lowestSellPriceOffer.ResourceTraded.Amount == 0)
				{
					RemoveTradeOffer(sellOffers, lowestSellPriceOffer);
				}
				if (highestBuyPriceOffer.ResourceTraded.Amount == 0)
				{
					RemoveTradeOffer(buyOffers, highestBuyPriceOffer);
				}
				return true;
			}
		}

		public void TryCompleteAsManyAsPossibleTrades()
		{
			while (TryCompleteTrade()) ;
		}

		public void CreateNewSellOffer(Resource forSale, Resource price, Trader seller)
		{
			lock (this)
			{
				sellOffers.Add(SpotMarketOffer.CreateSellOffer(forSale, price, this, seller));
				TryCompleteAsManyAsPossibleTrades();
			}
		}
		public void CreateNewBuyOffer(Resource toBuy, Resource price, Trader buyer)
		{
			lock (this)
			{
				buyOffers.Add(SpotMarketOffer.CreateBuyOffer(toBuy, price, this, buyer));
				TryCompleteAsManyAsPossibleTrades();
			}
		}

	}
}
