using GameCorpLib.Markets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Stocks
{
	public class HardStuffSilo<TResourceType> : Silo<TResourceType>
	{
		SpotMarket<TResourceType> _spotMarket;
		Player _player;
		public HardStuffSilo(R<Capacity<TResourceType>> capacity, SpotMarket<TResourceType> spotMarket, Player player) : base(capacity)
		{
			_spotMarket = spotMarket;
			_player = player;
		}
		public override void HandleSpill(R<TResourceType> spill)
		{
			_spotMarket.OnMarketPriceLiqudation(spill, _player);
		}
	}
}
