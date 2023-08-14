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
			PricePerUnitMined = new R<Money>(100 * Random.Shared.NextDouble());
			AllOil = new R<Oil>(10000 * Random.Shared.NextDouble());
			AmountLeft = AllOil;
		}
		public R<Money> PricePerUnitMined;
		R<Oil> AllOil;
		R<Oil> AmountLeft;
		public int NumberOfMiningRings = 0;
		public R<Money> BasePricePerRigBought = new R<Money>(100);
		double HalfMiningTime = 100;

		public R<Oil> GetExpectedMinedOilUnits()
		{
			R<Oil> ansver = (AmountLeft) * NumberOfMiningRings / HalfMiningTime;
			if (ansver > AmountLeft) ansver = AmountLeft;
			return ansver;
		}

		public R<Money> GetExpectedCostForMining()
		{
			return new R<Money>(-GetExpectedMinedOilUnits().Amount * PricePerUnitMined.Amount);
		}
		public bool TryBuyMiningRing()
		{
			lock (this)
			{
				if (!Owner.Stock.TryAddResource(-BasePricePerRigBought)) return false;
				NumberOfMiningRings++;
				return true;
			}
		}

		public R<Oil> Mine()
		{
			R<Oil> AmountMinedThisRound = GetExpectedMinedOilUnits();
			AmountLeft -= AmountMinedThisRound;
			return AmountMinedThisRound;
		}

		public override void Update()
		{
			lock (this)
			{
				//Pay for mining
				Owner.Stock.ForceIncreaseResources(GetExpectedCostForMining());
				//Mine
				Owner.Stock.ForceIncreaseResources(Mine());
				//Update amount left
			}
		}
	}

}
