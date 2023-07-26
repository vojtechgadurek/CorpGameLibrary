class Round
{
    readonly int RoundNumber;
    public Round(int roundNumber)
    {
        this.RoundNumber = roundNumber;
    }
    Round Next(Round currentRound)
    {
        Round newRound = new Round(currentRound.RoundNumber + 1);
        return newRound;
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

class MarketInResource
{

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
    /// <param name="resourceToBeBought"></param>
    /// What resource at what quantity is going to be bought
    /// <returns></returns>
    double DeterminePriceForTrade(Resource resourceToBeBought)
    {
        //Integral of the price 
        double fulfilmentOfStock = GetRatioOfStockToEconomy(resourceToBeBought.Type);
        double newFulfilmentOfStock = GetFulfilmentOfStockAfterChange(resourceToBeBought);
        
        double Integral(double fulfilmentOfStock)
        {
            return -BaseOilPrice * (1 / fulfilmentOfStock);
        }
        double price =  (Integral(newFulfilmentOfStock) - Integral(fulfilmentOfStock)) / ExpectedSizeOfStockAcordingToEconomy();
        return price;
    }
    bool TryTrade(Trader counterparty, Resource resource)
    {
        double price = DeterminePriceForTrade(resource);
        //This expect non-negative cash on company accounts
        return TryDoTrade(new Resource(ResourceType.Money, price), resource, this, counterparty);
    }
}
record struct Resource(ResourceType Type, double Amount);
class Trader
{
    public Stock Stock = new Stock();
    public double Money { get => Stock.resources[ResourceType.Money]; set => Stock.resources[ResourceType.Money] = value; }

    public IDictionary<int, Property> Properties = new Dictionary<int, Property>();
    public void ExchangeResource(Resource resource, Trader from, Trader to)
    {
        from.Stock.resources[resource.Type] -= resource.Amount;
        to.Stock.resources[resource.Type] += resource.Amount;
    }
    public bool TryDoTrade(Resource toBeSold, Resource toBeBought, Trader buyer, Trader seller)
    {
        //Negative values of resources are not allowed
       if(toBeSold.Amount < 0 || toBeBought.Amount < 0)
        {
                return false;
            }
       if(buyer.HasEnough(toBeSold.Type, toBeSold.Amount) && seller.HasEnough(toBeBought.Type, toBeBought.Amount))
       {
              ExchangeResource(toBeSold, seller, buyer);
              ExchangeResource(toBeBought, buyer, seller);
              return true;     
       }
        return false;
    }
    public bool HasEnough(ResourceType resource, double amnount)
    {
        return Stock.resources[resource] >= amnount;
    }

    public bool TryChangePropertyOwner(Property property, Trader oldOwner, Trader newOwner)
    {
        if(property.owner != oldOwner)
        {
            return false;
        }
        property.owner = newOwner;
        oldOwner.Properties.Remove(property.id);
        newOwner.Properties.Add(property.id, property);
        return true;
    }


}

class Company : Trader
{
    IDictionary<int, OilMine> OilMines = new Dictionary<int, OilMine>();
    Market market;

    public void TradeWithMarket(Resource resource)
    {
        
    }
    public bool TryProspectNewMine()
    {
        OilMine? mine = OilMineFactory.TryProspect(this);
        if(mine != null)
        {
            OilMines.Add(mine.Id, mine);
            return true;
        }
        return false;
    }     
}



enum ResourceType { Oil, Money }  
class Stock
{
    public IDictionary<ResourceType, double> resources = new Dictionary<ResourceType, double>();
    public Stock()
    {
        foreach (var resource in Enum.GetValues<ResourceType>()) { resources.Add(resource, 0); };
    }
}

static class PropertyFactory
{
    static int id = 0;
    static IList<Property> Properties = new List<Property>();
    public static Property CreateProperty()
    {
        Property newProperty = new Property(id, null);
        id++;
        Properties.Add(newProperty);
        return newProperty;
    }
    public enum TradePropertyState
    {
        Success,
        PropertyNotOwned,
        NotEnoughMoney
    }
    static public TradePropertyState TryTradeProperty(Trader seller, Trader buyer, Property property, double price)
    {
        if(seller != property.owner)
        {
            return TradePropertyState.PropertyNotOwned;
        }
        if((price > 0 && buyer.Money - price < 0) || price < 0 && (seller.Money + price) < 0)
        {
            return TradePropertyState.NotEnoughMoney;
        }
        
        
        //Do trade
        if(buyer.TryChangePropertyOwner(property, seller, buyer))
        {
            return TradePropertyState.Success;
        };
        return TradePropertyState.PropertyNotOwned;



    }
}

class Property
{
    public readonly int id;
    public Trader owner;
    public Property(int id, Trader owner)
    {
        this.id = id;
        this.owner = owner;
    }
}

static class OilMineFactory
{
    static int id = 0;
    static IList<OilMine> OilMines = new List<OilMine>();
    static readonly public double OilMinePrice = 1000;
    public static OilMine? TryProspect(Company owner)
    {  
        if(owner.TryPay(OilMinePrice))
        {
            return Prospect(owner);
        }
        return null;
    }
    public static OilMine Prospect (Company owner)
    {
        OilMine newMine = new OilMine(id, owner);
        id++;
        OilMines.Add(newMine);
        return newMine;
    }
}

class OilMine
{
    public readonly int Id;
    double allResources = 10000;
    double minedResources = 0;
    double miningRate = 0.01;
    Company owner;
    public OilMine(int id, Company owner)
    {
        this.Id = id;
        this.owner = owner;
    }
    void Mine() { 
        double newlyMinedResources = miningRate * allResources;
        minedResources += newlyMinedResources;
        owner.Stock.resources[ResourceType.Oil] += newlyMinedResources;
    }  
}



class Resource<Type>
{
    
}



