using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Utils
{
	public class Maybe<T> where T : class
	{
		T? Value;
		public static Maybe<T> Empty = GetEmpty();
		public Maybe(T value)
		{
			Value = value;
		}
		public Maybe()
		{
			Value = null;
		}
		static public Maybe<T> GetEmpty() => new Maybe<T>();
		public Maybe<T> Bind(T input, Func<T, T, T?> action)
		{
			if (Value is not null)
			{
				Value = action.Invoke(Value, input);
			}
			return this;
		}
	}
}
