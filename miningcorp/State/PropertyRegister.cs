using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.State
{
	public class PropertyRegister : Register<Property>
	{ 
		public enum TradePropertyState
		{
			Success,
			PropertyNotOwned,
			NotEnoughMoney
		}
		public TradePropertyState TryTradeProperty(Trader seller, Trader buyer, Property property, double price)
		{
			if (seller != property.owner)
			{
				return TradePropertyState.PropertyNotOwned;
			}
			if ((price > 0 && buyer.Money - price < 0) || price < 0 && (seller.Money + price) < 0)
			{
				return TradePropertyState.NotEnoughMoney;
			}


			//Do trade
			if (buyer.TryChangePropertyOwner(property, seller, buyer))
			{
				return TradePropertyState.Success;
			};
			return TradePropertyState.PropertyNotOwned;



		}
		public void Update()
		{
			foreach (var property in items)
			{
				property.Update();
			}
		}
	}
}
