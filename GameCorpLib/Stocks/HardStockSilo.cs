using GameCorpLib.Markets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Stocks
{
	public class HardStuffSilo : Silo
	{
		SpotMarket _spotMarket;
		public HardStuffSilo(Resource capacity, SpotMarket spotMarket) : base(capacity)
		{
			_spotMarket = spotMarket;
		}
		public override void HandleSpill(double spill)
		{
			throw new NotImplementedException();
		}
	}
}
