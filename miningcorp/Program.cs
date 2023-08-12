using GameCorpLib;
using GameCorpLib.State;
using GameCorpLib.Tradables;
using System.Data;
using System.Data.SqlTypes;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
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
	Company? writer;
	Company? holder;
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
class Market : Trader
{
	double BaseStockToEconomyMultiplicator = 10;
	Economy Economy;
	double BaseOilPrice = 100;

	public Market()
	{
		//Money is infinite
		Stock.TrySetResource(new Resource(ResourceType.Money, Double.PositiveInfinity));
		Stock.TrySetResource(new Resource(ResourceType.Oil, 1000));
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
		return Stock.GetResource(resourceType).Amount / ExpectedSizeOfStockAcordingToEconomy();
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
public record struct Resource(ResourceType Type, double Amount)
{
	static public Resource CreateMoney(double amount)
	{
		return new Resource(ResourceType.Money, amount);
	}
	static public Resource CreateOil(double amount)
	{
		return new Resource(ResourceType.Oil, amount);
	}

	public static Resource operator +(Resource resource, Resource amount)
	{
		if (resource.Type != amount.Type)
		{
			throw new System.Exception("Resources are not of the same type");
		}
		return new Resource(resource.Type, resource.Amount + amount.Amount);
	}
	public static Resource operator -(Resource resource, Resource amount)
	{
		if (resource.Type != amount.Type)
		{
			throw new System.Exception("Resources are not of the same type");
		}
		return new Resource(resource.Type, resource.Amount - amount.Amount);
	}
	public static Resource operator *(Resource resource, double amount)
	{
		return new Resource(resource.Type, resource.Amount * amount);
	}
	public static Resource operator /(Resource resource, double amount)
	{
		return new Resource(resource.Type, resource.Amount / amount);
	}
};







public enum ResourceType { Oil, Money }


