using EdjCase.ICP.Candid.Mapping;
using Candid.IcrcLedger.Models;
using EdjCase.ICP.Candid.Models;
using System.Collections.Generic;
using Timestamp = System.UInt64;

namespace Candid.IcrcLedger.Models
{
	public class Burn
	{
		[CandidName("from")]
		public Account From { get; set; }

		[CandidName("memo")]
		public OptionalValue<List<byte>> Memo { get; set; }

		[CandidName("created_at_time")]
		public Burn.CreatedAtTimeInfo CreatedAtTime { get; set; }

		[CandidName("amount")]
		public UnboundedUInt Amount { get; set; }

		[CandidName("spender")]
		public OptionalValue<Account> Spender { get; set; }

		public Burn(Account from, OptionalValue<List<byte>> memo, Burn.CreatedAtTimeInfo createdAtTime, UnboundedUInt amount, OptionalValue<Account> spender)
		{
			this.From = from;
			this.Memo = memo;
			this.CreatedAtTime = createdAtTime;
			this.Amount = amount;
			this.Spender = spender;
		}

		public Burn()
		{
		}

		public class CreatedAtTimeInfo : OptionalValue<Timestamp>
		{
			public CreatedAtTimeInfo()
			{
			}

			public CreatedAtTimeInfo(Timestamp value) : base(value)
			{
			}
		}
	}
}