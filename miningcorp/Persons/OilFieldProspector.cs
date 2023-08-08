using GameCorpLib.State;
using GameCorpLib.Tradables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Persons
{
	class OilFieldProspector : Trader
	{
		double MinePrice = 1000;
		IList<OilField> oilMines = new List<OilField>();
		PropertyRegister _propertyRegister;

		public OilFieldProspector(PropertyRegister propertyRegister)
		{
			_propertyRegister = propertyRegister;
		}
		public bool TryBuyNewMine(Trader buyer)
		{
			if (oilMines.Count == 0)
			{
				oilMines.Add(new OilField(this, _propertyRegister));
			}
			if (_propertyRegister.TryTradeProperty(this, buyer, oilMines.Last(), MinePrice) == PropertyRegister.TradePropertyState.Success)
			{
				oilMines.RemoveAt(oilMines.Count - 1);
				return true;
			};
			return false;
		}
	}
}
