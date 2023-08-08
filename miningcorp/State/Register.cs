using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.State
{
	/// <summary>
	/// Serves as register for Item, it provides id to them
	/// </summary>
	/// <typeparam name="TItem"></typeparam>
	public class Register<TItem>
	{
		public static int _id = 0;
		public static IList<TItem> items = new List<TItem>();
		/// <summary>
		/// Does basic registering and it is not thread safe
		/// </summary>
		/// <param name="item"></param>
		/// <param name="id"></param>
		void BasicRegister(TItem item, out int id)
		{
			items.Add(item);
			id = IssueId();
		}
		/// <summary>
		/// Allows to add actions to be done after registering
		/// </summary>
		/// <param name="item"></param>
		internal virtual void AfterRegisterItem(TItem item)
		{

		}
		/// <summary>
		/// Register item and fills its id field and it is thread safe
		/// </summary>
		/// <param name="item"></param>
		/// <param name="Id"></param>
		public void RegisterItem(TItem item, out int Id)
		{
			lock (this)
			{
				lock (items)
				{
					BasicRegister(item, out Id);
					AfterRegisterItem(item);
				}
			}
		}
		/// <summary>
		/// Issues id for item and it is not thread safe
		/// </summary>
		/// <returns></returns>
		int IssueId()
		{
			return _id++;
		}

	}
}
