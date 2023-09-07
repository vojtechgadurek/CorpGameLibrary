using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Utils
{
	public interface IVector<T>
	{
		public IVector<T> Add(T value);
		public IVector<T> Subtract(T value);
		public IVector<T> ScalarMultiply(double value);
		public bool IsZero();
	}

	public interface IOrderedVector<T> : IComparable<T>, IVector<T>
	{

	}
}
