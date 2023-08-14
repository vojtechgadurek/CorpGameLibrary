using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCorpLib.Transactions
{
	public interface ITransactionItem
	{
		void ExecuteTransfer();
		void ReleaseResources();
	}





	public class TransactionUtils
	{



	}

	public enum TransactionDirection
	{
		FromBuyerToSeller,
		FromSellerToBuyer
	}
}
