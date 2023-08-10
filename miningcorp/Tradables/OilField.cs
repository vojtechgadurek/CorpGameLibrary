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
			AllOil = 1000 * Random.Shared.NextDouble();
			AmountLeft = AllOil;
		}
		public double PricePerUnitMined;
		double AllOil;
		double AmountLeft;
		public int NumberOfMiningRings = 0;
		public double BasePricePerRigBought = 100;
		double HalfMiningTime = 100;
		public bool TryBuyMiningRing()
		{
			lock (owner.Stock.resources)
			{
				lock (this)
				{
					if (owner.Money < BasePricePerRigBought) return false;
					owner.Money -= BasePricePerRigBought;
					NumberOfMiningRings++;
					return true;
				}

			}
		}
		public override void Update()
		{
			double AmountMinedThisRound = (AmountLeft / AllOil) / HalfMiningTime * NumberOfMiningRings;
			if (AmountMinedThisRound > AmountLeft) AmountMinedThisRound = AmountLeft;
			owner.Oil += AmountMinedThisRound;
			AmountLeft -= AmountMinedThisRound;
			owner.Money -= AmountMinedThisRound * PricePerUnitMined;
		}
	}

}
