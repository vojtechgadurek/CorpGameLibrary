using GameCorpLib;
using GameCorpLib.State;
using System.Data;
using System.Security.Cryptography.X509Certificates;

public class Program
{
    public static void Main(string[] args)
    {
	}
}




class Contract
{
    private static int idCount = 0;
    int id;
    int startRound;
    int endRound;
    double price;
    double amountOfOil;
    Company writer;
    Company holder;
}

class Economy
{
    public double Size = 1;
    double Growth;
    double BaseOilDemand;
    void Update()
    {

    }
}

static class MarketWrapper
{
    public static Market market = new Market();
}

class MarketInResource
{
    Market market;
    void Test()
    {
       
    }
}
class Market : Trader
{
    double BaseStockToEconomyMultiplicator = 10;
    Economy Economy;
    double BaseOilPrice = 100;

    public Market()
    {
        //Money is infinite
        Stock.resources[ResourceType.Money] = Double.PositiveInfinity;
        Stock.resources[ResourceType.Oil] = 1000;
    }
    /// <summary>
    /// Calculates how large the stock should be according to the economy
    /// </summary>
    /// <returns></returns>
    double ExpectedSizeOfStockAcordingToEconomy()
    {
        return BaseStockToEconomyMultiplicator * Economy.Size;
    }

    /// <summary>
    /// Calculates ratio of stock held by market and to the number expeted by the size of the economy
    /// </summary>
    /// <param name="resourceType"></param>
    ///  Resource type of which the ratio is calculated
    /// <returns></returns>
    double GetRatioOfStockToEconomy(ResourceType resourceType)
    {
        return Stock.resources[resourceType] / ExpectedSizeOfStockAcordingToEconomy();
    }

    /// <summary>
    /// Returns how much a change in stock (units) would change the ratio of stock to economy
    /// </summary>
    /// <param name="change"></param>
    /// How much of the stock is going to be change (units)
    /// <returns></returns>
    double GetProportionOfChangeToStock(double change)
    {
        return change / ExpectedSizeOfStockAcordingToEconomy();
    }
    /// <summary>
    /// If the sale or purchase of a resource is made, this function calculates the new ratio of stock to economy
    /// </summary>
    /// <param name="expectedChangeOfStock"></param>
    /// How much of the stock is going to be change (units)
    /// <returns></returns>
    double GetFulfilmentOfStockAfterChange(Resource expectedChangeOfStock)
    {
        return GetRatioOfStockToEconomy(expectedChangeOfStock.Type) + GetProportionOfChangeToStock(expectedChangeOfStock.Amount);
    }
    /// <summary>
    /// Is used by market to determine the price of a resource, a ratio of stock to economy is used to determine the price
    /// </summary>
    /// <param name="resourceType"></param>
    /// <returns></returns>
    double DeterminePrice(ResourceType resourceType)
    {
        //Uses formula price = 1/x^2
        double fulfilmentOfStock = GetFulfilmentOfStockAfterChange(new Resource(resourceType, 0));
        return BaseOilPrice * (1 / Math.Pow(fulfilmentOfStock, 2));
    }
	/// <summary>
	///  Is used by market to determine the price of a resource, a ratio of stock to economy is used to determine the price
	///  It is integral of the price function
	/// </summary>
	/// What resource at what quantity is going to be bought
	/// <param name="resourceToBeBought"></param>
	/// <returns></returns>
	public double DeterminePriceForTrade(Resource resourceToBeBought)
    {
        //Integral of the price 
        double fulfilmentOfStock = GetRatioOfStockToEconomy(resourceToBeBought.Type);
        double newFulfilmentOfStock = GetFulfilmentOfStockAfterChange(resourceToBeBought);
        double Integral(double fulfilmentOfStock)
        {
            return -BaseOilPrice * (1 / fulfilmentOfStock);
        }
        double price = (Integral(newFulfilmentOfStock) - Integral(fulfilmentOfStock)) / ExpectedSizeOfStockAcordingToEconomy();
        return price;
    }
    bool TryTrade(Trader counterparty, Resource resource)
    {
        double price = DeterminePriceForTrade(resource);
        //This expect non-negative cash on company accounts
        return TryDoTrade(new Resource(ResourceType.Money, price), resource, this, counterparty);
    }
}
public record struct Resource(ResourceType Type, double Amount);






public enum ResourceType { Oil, Money }
public class Stock
{
    public IDictionary<ResourceType, double> resources = new Dictionary<ResourceType, double>();
    public Stock()
    {
        foreach (var resource in Enum.GetValues<ResourceType>()) { resources.Add(resource, 0); };
    }
}



class OilField : Property
{
    public OilField(Trader owner, PropertyRegister propertyRegister) : base(owner, propertyRegister)
    {

    }
    double PricePerUnitMined;

    public override void Update()
    {
        //Mine

    }
}

static class Prospectors
{
    public static OilMineProspector OilMineProspector = new OilMineProspector();
}
class OilMineProspector : Trader
{
    double MinePrice = 1000;
    IList<OilMine> oilMines = new List<OilMine>();
    public bool TryBuyNewMine(Trader buyer, PropertyRegister propertyRegister)
    {
        if (oilMines.Count == 0)
        {
            oilMines.Add(new OilMine(this, propertyRegister));
        }
        if (propertyRegister.TryTradeProperty(this, buyer, oilMines.Last(), MinePrice) == PropertyRegister.TradePropertyState.Success)
        {
            oilMines.RemoveAt(oilMines.Count - 1);
            return true;
        };
        return false;


    }

}

class OilMine : Property
{
    double allResources = 10000;
    double minedResources = 0;
    double miningRate = 0.01;
    bool Active = false;
    public OilMine(Trader owner, PropertyRegister propertyRegister) : base(owner, propertyRegister)
    {
    }
    public override void Update()
    {
        if (Active)
        {
            Mine();
        }
    }
    void Mine()
    {
        double newlyMinedResources = miningRate * allResources;
        minedResources += newlyMinedResources;
        owner.Stock.resources[ResourceType.Oil] += newlyMinedResources;
    }
}

class Resource<Type>
{

}

