using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GameCorpLib;
using GameCorpLib.Transactions;

namespace GameCorpLib.Markets
{
	public enum SpotMarketOfferType
	{
		Sell,
		Buy
	}


	public class SpotMarket
	{
		IDictionary<Type, object> _spotmarkets = new Dictionary<Type, object>();
		public SpotMarket(R<Money> GovermentBuyout)
		{
			foreach (var resource in Resources.ResourcesList)
			{
				object spotMarket = Activator.CreateInstance(typeof(SpotMarketInResource<>).MakeGenericType(resource.Value.TheType), GovermentBuyout);
				_spotmarkets.Add(resource.Value.TheType, spotMarket);
			}
		}

		public object GetSpotMarket(Type resourceType)
		{
			return _spotmarkets[resourceType];
		}
		public SpotMarketInResource<TResourceType> GetSpotMarket<TResourceType>() where TResourceType : IResource
		{
			return (SpotMarketInResource<TResourceType>)_spotmarkets[typeof(TResourceType)];
		}
	}
}

public class SpotMarketOffer<TResourceTradedType> where TResourceTradedType : IResource
{
	SpotMarketOfferType _offerType;
	TwoPartyProportionalTransaction<TResourceTradedType, Money> _proportionalTransaction;
	R<Money> _pricePerUnit;
	public R<TResourceTradedType> ResourceTraded { get => _proportionalTransaction.FromSeller; }
	public R<Money> PricePerUnit { get => _pricePerUnit; }

	SpotMarketOffer(SpotMarketOfferType offerType, R<Money> price, TwoPartyProportionalTransaction<TResourceTradedType, Money> proportionalTransaction)
	{
		_offerType = offerType;
		_pricePerUnit = price;
		_proportionalTransaction = proportionalTransaction;
	}

	public void ReleaseResources()
	{
		_proportionalTransaction.ReleaseResource();
	}

	public bool TryFill(R<TResourceTradedType> resource)
	{
		return _proportionalTransaction.TryExecuteProportional<TResourceTradedType>(resource);
	}

	public static SpotMarketOffer<TResourceTradedType>? TryCreateTradeOffer(R<TResourceTradedType> resourceTraded, R<Money> pricePerUnit, Trader market, Trader counterParty, SpotMarketOfferType offerType)
	{

		Trader trader1;
		Trader trader2;
		if (offerType == SpotMarketOfferType.Buy)
		{
			trader1 = market;
			trader2 = counterParty;
		}
		else
		{
			trader1 = counterParty;
			trader2 = market;
		}
		var proportionalTransaction = new TwoPartyProportionalTransaction<TResourceTradedType, Money>(resourceTraded, resourceTraded.Amount * pricePerUnit, trader1, trader2);
		if (proportionalTransaction.SetupFailed) return null;

		return new SpotMarketOffer<TResourceTradedType>(offerType, pricePerUnit, proportionalTransaction);
	}
}


public class SpotMarketInResource<TResourceTradedType> : PrivilegedTrader where TResourceTradedType : IResource
{
	R<Money> _govermentBuyout;
	public R<Money> GovermentBuyout { get => _govermentBuyout; }
	public SortedSet<SpotMarketOffer<TResourceTradedType>> SellOffers = new SortedSet<SpotMarketOffer<TResourceTradedType>>(new SpotMarketComparer<TResourceTradedType>());
	public SortedSet<SpotMarketOffer<TResourceTradedType>> BuyOffers = new SortedSet<SpotMarketOffer<TResourceTradedType>>(new SpotMarketComparer<TResourceTradedType>());

	class SpotMarketComparer<TResourceType> : IComparer<SpotMarketOffer<TResourceType>> where TResourceType : IResource
	{
		public int Compare(SpotMarketOffer<TResourceType> x, SpotMarketOffer<TResourceType> y)
		{
			int ansver = x.PricePerUnit.Amount.CompareTo(y.PricePerUnit.Amount);
			if (ansver == 0) return x.GetHashCode().CompareTo(y.GetHashCode());
			return ansver;
		}
	}

	public SpotMarketInResource(R<Money> govermentBuyout)
	{
		_govermentBuyout = govermentBuyout;
	}
	public void RemoveTradeOffer(SortedSet<SpotMarketOffer<TResourceTradedType>> list, SpotMarketOffer<TResourceTradedType> spotMarketOffer)
	{
		lock (this)
		{
			list.Remove(spotMarketOffer);
			spotMarketOffer.ReleaseResources();
		}
	}
	bool TryCompleteTrade()
	{
		lock (this)
		{
			if (SellOffers.Count == 0 || BuyOffers.Count == 0) return false;
			var lowestSellPriceOffer = SellOffers.First();
			var highestBuyPriceOffer = BuyOffers.Last();
			if (lowestSellPriceOffer.PricePerUnit > highestBuyPriceOffer.PricePerUnit)
			{
				return false;
			}
			R<TResourceTradedType> resource =
				lowestSellPriceOffer.ResourceTraded < highestBuyPriceOffer.ResourceTraded ?
				lowestSellPriceOffer.ResourceTraded : highestBuyPriceOffer.ResourceTraded;
			lowestSellPriceOffer.TryFill(resource);
			highestBuyPriceOffer.TryFill(resource);
			if (lowestSellPriceOffer.ResourceTraded.Amount == 0)
			{
				RemoveTradeOffer(SellOffers, lowestSellPriceOffer);
			}
			if (highestBuyPriceOffer.ResourceTraded.Amount == 0)
			{
				RemoveTradeOffer(BuyOffers, highestBuyPriceOffer);
			}
			return true;
		}
	}

	public void OnMarketPriceLiqudation(R<TResourceTradedType> toLiquidate, Player player)
	{
		lock (this)
		{
			while (toLiquidate > 0.Create<TResourceTradedType>() && BuyOffers.Count > 0)
			{
				var higgestPriceOffer = BuyOffers.Last();
				//If there is no noone to buy then there is goverment to buy :D at very miserable price
				R<TResourceTradedType> amountSold = higgestPriceOffer.ResourceTraded < toLiquidate ? higgestPriceOffer.ResourceTraded : toLiquidate;
				higgestPriceOffer.TryFill(amountSold);

				player.Stock.ForceIncreaseResources<Money>((amountSold.Amount * higgestPriceOffer.PricePerUnit));
				toLiquidate -= amountSold;
			}
			player.Stock.ForceIncreaseResources<Money>(toLiquidate.Amount * _govermentBuyout);
		}
	}

	public void TryCompleteAsManyAsPossibleTrades()
	{
		while (TryCompleteTrade()) ;
	}

	public bool TryCreateNewTradeOffer(R<TResourceTradedType> resource, R<Money> price, Trader trader, SpotMarketOfferType spotMarketOfferType)
	{
		lock (this)
		{
			if (resource <= 0.Create<TResourceTradedType>()) return false;
			SpotMarketOffer<TResourceTradedType>? marketOffer = SpotMarketOffer<TResourceTradedType>.TryCreateTradeOffer(
				resource, price, this, trader, spotMarketOfferType);
			if (marketOffer is null) return false;

			if (spotMarketOfferType == SpotMarketOfferType.Buy)
			{
				BuyOffers.Add(marketOffer);
			}
			else
			{
				SellOffers.Add(marketOffer);
			}

			TryCompleteAsManyAsPossibleTrades();
			return true;
		}
	}

}
