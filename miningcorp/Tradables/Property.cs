using GameCorpLib.State;
using GameCorpLib.Tradables;

namespace GameCorpLib
{
	public abstract class Property : ITradable
	{
		public readonly int Id;
		public Trader owner;
		public Property(Trader owner, PropertyRegister propertyRegister)
		{
			//Adds id
			propertyRegister.RegisterItem(this, out int Id);
			this.owner = owner;
		}

		public void ChangeOwner(Trader newOwner)
		{
			throw new NotImplementedException();
		}

		public bool TryLockForTrade()
		{
			throw new NotImplementedException();
		}

		public void UnlockFromTrade()
		{
			throw new NotImplementedException();
		}

		abstract public void Update();
	}
}
