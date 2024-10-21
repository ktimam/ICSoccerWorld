using EdjCase.ICP.Candid.Mapping;

namespace Candid.IcrcLedger.Models
{
	public class FeatureFlags
	{
		[CandidName("icrc2")]
		public bool Icrc2 { get; set; }

		public FeatureFlags(bool icrc2)
		{
			this.Icrc2 = icrc2;
		}

		public FeatureFlags()
		{
		}
	}
}