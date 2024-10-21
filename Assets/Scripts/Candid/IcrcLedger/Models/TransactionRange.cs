using EdjCase.ICP.Candid.Mapping;
using System.Collections.Generic;
using Candid.IcrcLedger.Models;

namespace Candid.IcrcLedger.Models
{
	public class TransactionRange
	{
		[CandidName("transactions")]
		public List<Transaction> Transactions { get; set; }

		public TransactionRange(List<Transaction> transactions)
		{
			this.Transactions = transactions;
		}

		public TransactionRange()
		{
		}
	}
}