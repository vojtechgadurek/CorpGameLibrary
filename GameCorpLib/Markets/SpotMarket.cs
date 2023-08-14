using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GameCorpLib.Transactions;

namespace GameCorpLib.Markets
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
        Resource _pricePerUnit;
        public Resource ResourceTraded { get => _offerType == SpotMarketOfferType.Buy ? _proportionalTransaction.FromBuyer : _proportionalTransaction.FromSeller; }
        public Resource PricePerUnit { get => _pricePerUnit; }

        SpotMarketOffer(SpotMarketOfferType offerType, Resource price, ProportionalTransaction proportionalTransaction)
        {
            _offerType = offerType;
            _pricePerUnit = price;
            _proportionalTransaction = proportionalTransaction;
        }

        public void ReleaseResources()
        {
            _proportionalTransaction.ReleaseResource();
        }

        public bool TryFill(Resource resource)
        {
            return _proportionalTransaction.TryExecuteProportional(resource);
        }

        public static SpotMarketOffer? TryCreateSellOffer(Resource forSale, Resource pricePerUnit, Trader market, Trader seller)
        {
            var proportionalTransaction = new ProportionalTransaction(forSale, pricePerUnit * forSale.Amount, seller, market);

            if (proportionalTransaction.SetupFailed) return null;

            return new SpotMarketOffer(SpotMarketOfferType.Sell, pricePerUnit, proportionalTransaction);
        }
        public static SpotMarketOffer? TryCreateBuyOffer(Resource toBuy, Resource pricePerUnit, Trader market, Trader buyer)
        {
            var proportionalTransaction = new ProportionalTransaction(pricePerUnit * toBuy.Amount, toBuy, buyer, market);

            if (proportionalTransaction.SetupFailed) return null;

            return new SpotMarketOffer(SpotMarketOfferType.Buy, pricePerUnit, proportionalTransaction);
        }

    }

    public class SpotMarket : PrivilegedTrader
    {
        Resource _govermentBuyout;
        ResourceType _resourceTraded = ResourceType.Oil;
        public SortedSet<SpotMarketOffer> SellOffers = new SortedSet<SpotMarketOffer>(new SpotMarketComparer());
        public SortedSet<SpotMarketOffer> BuyOffers = new SortedSet<SpotMarketOffer>(new SpotMarketComparer());

        class SpotMarketComparer : IComparer<SpotMarketOffer>
        {
            public int Compare(SpotMarketOffer x, SpotMarketOffer y)
            {
                int ansver = x.PricePerUnit.Amount.CompareTo(y.PricePerUnit.Amount);
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
                if (SellOffers.Count == 0 || BuyOffers.Count == 0) return false;
                var lowestSellPriceOffer = SellOffers.First();
                var highestBuyPriceOffer = BuyOffers.Last();
                if (lowestSellPriceOffer.PricePerUnit > highestBuyPriceOffer.PricePerUnit)
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
                    RemoveTradeOffer(SellOffers, lowestSellPriceOffer);
                }
                if (highestBuyPriceOffer.ResourceTraded.Amount == 0)
                {
                    RemoveTradeOffer(BuyOffers, highestBuyPriceOffer);
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
                    var higgestPriceOffer = BuyOffers.Last();
                    //If there is no noone to buy then there is goverment to buy :D at very miserable price
                    if (higgestPriceOffer is null)
                    {
                        player.Stock.ForceIncreaseResources(Resource.CreateMoney(toLiquidate.Amount * _govermentBuyout.Amount));
                        return;
                    }

                    Resource amountSold = higgestPriceOffer.ResourceTraded < toLiquidate ? higgestPriceOffer.ResourceTraded : toLiquidate;
                    higgestPriceOffer.TryFill(amountSold);
                    player.Stock.ForceIncreaseResources(Resource.CreateMoney(amountSold.Amount * higgestPriceOffer.PricePerUnit.Amount));
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

            if (resource.Type != _resourceTraded) throw new InvalidOperationException("Wrong resource type");
            lock (this)
            {
                SpotMarketOffer? marketOffer;
                switch (spotMarketOfferType)
                {
                    case SpotMarketOfferType.Buy:
                        marketOffer = SpotMarketOffer.TryCreateBuyOffer(resource, price, this, trader);
                        if (marketOffer is null) return false;
                        BuyOffers.Add(marketOffer);
                        break;
                    case SpotMarketOfferType.Sell:
                        marketOffer = SpotMarketOffer.TryCreateSellOffer(resource, price, this, trader);
                        if (marketOffer is null) return false;
                        SellOffers.Add(marketOffer);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown SpotMarketOfferType");
                }

                TryCompleteAsManyAsPossibleTrades();
                return true;
            }
        }

    }
}
