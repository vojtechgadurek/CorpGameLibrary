using GameCorpLib.Persons;
using GameCorpLib.State;

namespace GameCorpLib
{
	public class Company : ITrader
	{
		public string Name;
		readonly int Id;
		Player Controler;
		Trader trader;
		public Stock Stock { get => trader.Stock; }
		public Company(string name, Player controler, CompanyRegister companyRegister)
		{
			Name = name;
			Controler = controler;
			companyRegister.RegisterItem(this, out Id);
		}
	}
}
