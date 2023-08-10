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
		int oilMinesSold;
		PropertyRegister _propertyRegister;

		public OilFieldProspector(PropertyRegister propertyRegister)
		{
			_propertyRegister = propertyRegister;
		}
		public bool TryProspectNewMine(Trader buyer)
		{
			if (oilMinesSold.Count =<)
			{
				oilMinesSold.Add(new OilField(this, _propertyRegister));
			}
			var mineForS
			new TwoPartyTransaction
			return false;
		}
	}
}
