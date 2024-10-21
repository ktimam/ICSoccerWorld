using EdjCase.ICP.Candid.Mapping;
using EdjCase.ICP.Candid.Models;
using Candid.World.Models;

namespace Candid.World.Models
{
	public class ICRCStake
	{
		[CandidName("amount")]
		public UnboundedUInt Amount { get; set; }

		[CandidName("dissolvedAt")]
		public UnboundedInt DissolvedAt { get; set; }

		[CandidName("kind")]
		public ICRCStakeKind Kind { get; set; }

		[CandidName("stakedAt")]
		public UnboundedInt StakedAt { get; set; }

		[CandidName("staker")]
		public string Staker { get; set; }

		[CandidName("tokenCanisterId")]
		public string TokenCanisterId { get; set; }

		public ICRCStake(UnboundedUInt amount, UnboundedInt dissolvedAt, ICRCStakeKind kind, UnboundedInt stakedAt, string staker, string tokenCanisterId)
		{
			this.Amount = amount;
			this.DissolvedAt = dissolvedAt;
			this.Kind = kind;
			this.StakedAt = stakedAt;
			this.Staker = staker;
			this.TokenCanisterId = tokenCanisterId;
		}

		public ICRCStake()
		{
		}
	}
}