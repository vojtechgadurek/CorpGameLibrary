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

		public void Update()
		{
			foreach (var property in items)
			{
				property.Update();
			}
		}
	}
}
