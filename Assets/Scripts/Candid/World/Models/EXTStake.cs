using EdjCase.ICP.Candid.Mapping;
using EdjCase.ICP.Candid.Models;

namespace Candid.World.Models
{
	public class EXTStake
	{
		[CandidName("dissolvedAt")]
		public UnboundedInt DissolvedAt { get; set; }

		[CandidName("stakedAt")]
		public UnboundedInt StakedAt { get; set; }

		[CandidName("staker")]
		public string Staker { get; set; }

		[CandidName("tokenIndex")]
		public uint TokenIndex { get; set; }

		public EXTStake(UnboundedInt dissolvedAt, UnboundedInt stakedAt, string staker, uint tokenIndex)
		{
			this.DissolvedAt = dissolvedAt;
			this.StakedAt = stakedAt;
			this.Staker = staker;
			this.TokenIndex = tokenIndex;
		}

		public EXTStake()
		{
		}
	}
}