using GameCorpLib.State;

namespace GameCorpLib
{
	public abstract class Property
	{
		public readonly int Id;
		public Trader owner;
		public Property(Trader owner, PropertyRegister propertyRegister)
		{
			//Adds id
			propertyRegister.RegisterItem(this, out int Id);
			this.owner = owner;
		}
		abstract public void Update();
	}
}
