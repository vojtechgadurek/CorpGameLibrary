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

		public static SpotMarketOffer? TryCreateSellOffer(Resource forSale, Resource price, Trader market, Trader seller)
		{
			var proportionalTransaction = new ProportionalTransaction(forSale, price * forSale.Amount, seller, market);

			if (proportionalTransaction.SetupFailed) return null;

			return new SpotMarketOffer(SpotMarketOfferType.Sell, price, forSale, proportionalTransaction);
		}
		public static SpotMarketOffer? TryCreateBuyOffer(Resource toBuy, Resource price, Trader market, Trader buyer)
		{
			var proportionalTransaction = new ProportionalTransaction(price * toBuy.Amount, toBuy, market, buyer);

			if (proportionalTransaction.SetupFailed) return null;

			return new SpotMarketOffer(SpotMarketOfferType.Buy, price, toBuy, proportionalTransaction);
		}

	}

	public class SpotMarket : PriviligedTrader
	{
		Resource _govermentBuyout;
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

		public SpotMarket(Resource govermentBuyout)
		{
			_govermentBuyout = govermentBuyout;
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

		public void OnMarketPriceLiqudation(Resource toLiquidate, Player player)
		{
			lock (this)
			{
				while (toLiquidate.Amount > 0)
				{
					var higgestPriceOffer = buyOffers.Last();
					//If there is no noone to buy then there is goverment to buy :D at very miserable price
					if (higgestPriceOffer is null)
					{
						player.Stock.ForceIncreaseResources(Resource.CreateMoney(toLiquidate.Amount * _govermentBuyout.Amount));
						return;
					}

					Resource amountSold = higgestPriceOffer.ResourceTraded < toLiquidate ? higgestPriceOffer.ResourceTraded : toLiquidate;
					higgestPriceOffer.TryFill(amountSold);
					player.Stock.ForceIncreaseResources(Resource.CreateMoney(amountSold.Amount * higgestPriceOffer.Price.Amount));
					toLiquidate -= amountSold;
				}
			}
		}

		public void TryCompleteAsManyAsPossibleTrades()
		{
			while (TryCompleteTrade()) ;
		}

		public bool TryCreateNewTradeOffer(Resource resource, Resource price, Trader trader, SpotMarketOfferType spotMarketOfferType)
		{
			lock (this)
			{
				SpotMarketOffer? marketOffer;
				switch (spotMarketOfferType)
				{
					case SpotMarketOfferType.Buy:
						marketOffer = SpotMarketOffer.TryCreateBuyOffer(resource, price, this, trader);
						break;
					case SpotMarketOfferType.Sell:
						marketOffer = SpotMarketOffer.TryCreateSellOffer(resource, price, this, trader);
						break;
					default:
						throw new InvalidOperationException("Unknown SpotMarketOfferType");
				}
				if (marketOffer is null) return false;
				sellOffers.Add(marketOffer);
				TryCompleteAsManyAsPossibleTrades();
				return true;
			}
		}

	}
}
