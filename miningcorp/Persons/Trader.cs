namespace GameCorpLib
{
    public class Trader
    {
        public Stock Stock = new Stock();
        public double Money { get => Stock.resources[ResourceType.Money]; set => Stock.resources[ResourceType.Money] = value; }
        public double Oil { get => Stock.resources[ResourceType.Oil]; set => Stock.resources[ResourceType.Oil] = value; }

        public IDictionary<int, Property> Properties = new Dictionary<int, Property>();
        public void ExchangeResource(Resource resource, Trader from, Trader to)
        {
            from.Stock.resources[resource.Type] -= resource.Amount;
            to.Stock.resources[resource.Type] += resource.Amount;
        }
        public bool TryDoTrade(Resource toBeSold, Resource toBeBought, Trader buyer, Trader seller)
        {
            //Negative values of resources are not allowed
            if (toBeSold.Amount < 0 || toBeBought.Amount < 0)
            {
                return false;
            }
            if (buyer.HasEnough(toBeSold.Type, toBeSold.Amount) && seller.HasEnough(toBeBought.Type, toBeBought.Amount))
            {
                ExchangeResource(toBeSold, seller, buyer);
                ExchangeResource(toBeBought, buyer, seller);
                return true;
            }
            return false;
        }
        public bool HasEnough(ResourceType resource, double amnount)
        {
            return Stock.resources[resource] >= amnount;
        }

        public bool TryChangePropertyOwner(Property property, Trader oldOwner, Trader newOwner)
        {
            if (property.owner != oldOwner)
            {
                return false;
            }
            property.owner = newOwner;
            oldOwner.Properties.Remove(property.Id);
            newOwner.Properties.Add(property.Id, property);
            return true;
        }
    }
}
