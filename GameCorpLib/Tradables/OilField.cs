using GameCorpLib.State;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Tradables
{
	public class OilField : Property
	{
		public OilField(Trader owner, PropertyRegister propertyRegister) : base(owner, propertyRegister)
		{
			PricePerUnitMined = 100 * Random.Shared.NextDouble();
			AllOil = 10000 * Random.Shared.NextDouble();
			AmountLeft = AllOil;
		}
		public double PricePerUnitMined;
		double AllOil;
		double AmountLeft;
		public int NumberOfMiningRings = 0;
		public double BasePricePerRigBought = 100;
		double HalfMiningTime = 100;

		public double GetExpectedMinedOilUnits()
		{
			double ansver = (AmountLeft) / HalfMiningTime * NumberOfMiningRings;
			if (ansver > AmountLeft) ansver = AmountLeft;
			return ansver;
		}

		public Resource GetExpectedCostForMining()
		{
			return Resource.CreateMoney(-GetExpectedMinedOilUnits() * PricePerUnitMined);
		}
		public bool TryBuyMiningRing()
		{
			lock (this)
			{
				if (!Owner.Stock.TryAddResource(Resource.CreateMoney(-BasePricePerRigBought))) return false;
				NumberOfMiningRings++;
				return true;
			}
		}
		public override void Update()
		{
			lock (this)
			{
				double AmountMinedThisRound = GetExpectedMinedOilUnits();
				//Pay for mining
				Owner.Stock.ForceIncreaseResources(GetExpectedCostForMining());
				//Mine
				Owner.Stock.ForceIncreaseResources(new Resource(ResourceType.Oil, AmountMinedThisRound));
				//Update amount left
				AmountLeft -= AmountMinedThisRound;
			}

		}
	}

}
