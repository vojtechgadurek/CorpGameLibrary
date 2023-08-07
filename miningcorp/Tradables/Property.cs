using GameCorpLib.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
