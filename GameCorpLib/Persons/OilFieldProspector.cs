using GameCorpLib.State;
using GameCorpLib.Tradables;
using GameCorpLib.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Persons
{
	public class OilFieldProspector : PrivilegedTrader
	{
		R<Money> _minePrice;
		IList<OilField> oilMines = new List<OilField>();
		int oilMinesSold;
		PropertyRegister _propertyRegister;

		public OilFieldProspector(PropertyRegister propertyRegister, R<Money> minePrice)
		{
			_propertyRegister = propertyRegister;
			_minePrice = minePrice;
		}
		public bool TryProspectNewMine(Trader buyer)
		{
			if (oilMines.Count <= oilMinesSold)
			{
				oilMines.Add(new OilField(this, _propertyRegister));
			}
			var mineForSale = oilMines[oilMinesSold];
			TwoPartyTransaction buyingMineTransaction = new TwoPartyTransaction(buyer, this)
				.AddTransactionItem(mineForSale, TransactionDirection.FromSellerToBuyer)
				.AddTransactionItem(_minePrice, TransactionDirection.FromBuyerToSeller);
			bool success = buyingMineTransaction.TryExecute();
			if (success)
			{
				oilMinesSold++;
			}
			return success;
		}
	}
}
