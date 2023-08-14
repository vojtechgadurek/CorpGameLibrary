global using GameCorpLib.Utils;
global using GameCorpLib.State;
global using GameCorpLib.Tradables;
global using GameCorpLib.Transactions;
global using GameCorpLib.Stocks;
global using GameCorpLib.Markets;
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
/*
public record struct R(ResourceType Type, double Amount)
{
	static public R CreateMoney(double amount)
	{
		return new R(ResourceType.Money, amount);
	}
	static public R CreateOil(double amount)
	{
		return new R(ResourceType.Oil, amount);
	}

	public static R operator +(R resource, R amount)
	{
		if (resource.Type != amount.Type)
		{
			throw new System.Exception("Resources are not of the same type");
		}
		return new R(resource.Type, resource.Amount + amount.Amount);
	}
	public static R operator -(R resource, R amount)
	{
		if (resource.Type != amount.Type)
		{
			throw new System.Exception("Resources are not of the same type");
		}
		return new R(resource.Type, resource.Amount - amount.Amount);
	}
	public static R operator *(R resource, double amount)
	{
		return new R(resource.Type, resource.Amount * amount);
	}
	public static R operator /(R resource, double amount)
	{
		return new R(resource.Type, resource.Amount / amount);
	}
	public static bool operator >(R resource, R amount)
	{
		if (resource.Type != amount.Type)
		{
			throw new System.Exception("Resources are not of the same type");
		}
		return resource.Amount > amount.Amount;
	}
	public static bool operator <(R resource, R amount)
	{
		if (resource.Type != amount.Type)
		{
			throw new System.Exception("Resources are not of the same type");
		}
		return resource.Amount < amount.Amount;
	}
	public static bool operator >=(R resource, R amount)
	{
		if (resource.Type != amount.Type)
		{
			throw new System.Exception("Resources are not of the same type");
		}
		return resource.Amount >= amount.Amount;
	}
	public static bool operator <=(R resource, R amount)
	{
		if (resource.Type != amount.Type)
		{
			throw new System.Exception("Resources are not of the same type");
		}
		return resource.Amount <= amount.Amount;
	}
	public static R operator -(R resource)
	{
		return new R(resource.Type, -resource.Amount);
	}
};

*/





public enum ResourceType { Oil, Money, Capacity }


