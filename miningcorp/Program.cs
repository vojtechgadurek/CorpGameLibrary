global using GameCorpLib.Utils;
global using GameCorpLib.State;
global using GameCorpLib.Tradables;
global using GameCorpLib.Transactions;
global using System;
global using System.Collections.Generic;
global using System.Linq;



using GameCorpLib;
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
	public static bool operator >(Resource resource, Resource amount)
	{
		if (resource.Type != amount.Type)
		{
			throw new System.Exception("Resources are not of the same type");
		}
		return resource.Amount > amount.Amount;
	}
	public static bool operator <(Resource resource, Resource amount)
	{
		if (resource.Type != amount.Type)
		{
			throw new System.Exception("Resources are not of the same type");
		}
		return resource.Amount < amount.Amount;
	}
	public static bool operator >=(Resource resource, Resource amount)
	{
		if (resource.Type != amount.Type)
		{
			throw new System.Exception("Resources are not of the same type");
		}
		return resource.Amount >= amount.Amount;
	}
	public static bool operator <=(Resource resource, Resource amount)
	{
		if (resource.Type != amount.Type)
		{
			throw new System.Exception("Resources are not of the same type");
		}
		return resource.Amount <= amount.Amount;
	}
};







public enum ResourceType { Oil, Money }


