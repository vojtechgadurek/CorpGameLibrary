using GameCorpLib.State;
using GameCorpLib.Tradables;

namespace GameCorpLib
{
	public abstract class Property : ITradable
	{
		public readonly int Id;
		public Trader owner;
		private bool _lockedForTrade = false;
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
			lock (this)
			{
				if (_lockedForTrade)
				{
					return false;
				}
				_lockedForTrade = true;
				return true;
			}
		}

		public void UnlockFromTrade()
		{
			_lockedForTrade = false;
		}

		abstract public void Update();
	}
}
