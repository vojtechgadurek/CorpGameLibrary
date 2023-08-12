using GameCorpLib.State;
using GameCorpLib.Tradables;
using System.Reflection.Metadata.Ecma335;

namespace GameCorpLib
{
	public abstract class Property
	{
		private readonly int _id;
		public int Id { get => _id; }
		public Trader Owner;
		bool _lockedForTrade = false;
		private PropertyRegister _propertyRegister;
		public Property(Trader owner, PropertyRegister propertyRegister)
		{
			//Adds id
			propertyRegister.RegisterItem(this, out _id);
			this.Owner = owner;
			_propertyRegister = propertyRegister;
		}

		public void ChangeOwner(Trader newOwner)
		{

			Owner.Properties.Remove(this);
			newOwner.Properties.Add(this);
			Owner = newOwner;
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
