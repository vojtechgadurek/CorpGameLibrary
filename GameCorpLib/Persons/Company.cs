using GameCorpLib.State;

namespace GameCorpLib
{
	public class Company
	{
		public string Name;
		readonly int Id;
		Player Controler;
		Trader trader;
		Stock Stock => trader.Stock;
		public Company(string name, Player controler, CompanyRegister companyRegister)
		{
			Name = name;
			Controler = controler;
			companyRegister.RegisterItem(this, out Id);
		}
	}
}
