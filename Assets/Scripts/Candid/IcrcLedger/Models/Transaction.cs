using EdjCase.ICP.Candid.Mapping;
using EdjCase.ICP.Candid.Models;
using Candid.IcrcLedger.Models;
using Timestamp = System.UInt64;

namespace Candid.IcrcLedger.Models
{
	public class Transaction
	{
		[CandidName("burn")]
		public OptionalValue<Burn> Burn { get; set; }

		[CandidName("kind")]
		public string Kind { get; set; }

		[CandidName("mint")]
		public OptionalValue<Mint> Mint { get; set; }

		[CandidName("approve")]
		public OptionalValue<Approve> Approve { get; set; }

		[CandidName("timestamp")]
		public Timestamp Timestamp { get; set; }

		[CandidName("transfer")]
		public OptionalValue<Transfer> Transfer { get; set; }

		public Transaction(OptionalValue<Burn> burn, string kind, OptionalValue<Mint> mint, OptionalValue<Approve> approve, Timestamp timestamp, OptionalValue<Transfer> transfer)
		{
			this.Burn = burn;
			this.Kind = kind;
			this.Mint = mint;
			this.Approve = approve;
			this.Timestamp = timestamp;
			this.Transfer = transfer;
		}

		public Transaction()
		{
		}
	}
}