using GameCorpLib.State;

namespace GameCorpLib
{
	public class Company : Trader
	{
		public string Name;
		readonly int Id;
		Player Controler;
		public Company(string name, Player controler, CompanyRegister companyRegister)
		{
			Name = name;
			Controler = controler;
			companyRegister.RegisterItem(this, out Id);
		}
		public bool TryProspectNewMine(PropertyRegister propertyRegister)
		{
			return Prospectors.OilMineProspector.TryBuyNewMine(this, propertyRegister);
		}
	}
}
