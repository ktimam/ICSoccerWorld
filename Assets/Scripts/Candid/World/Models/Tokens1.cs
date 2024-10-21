using EdjCase.ICP.Candid.Mapping;

namespace Candid.World.Models
{
	public class Tokens1
	{
		[CandidName("e8s")]
		public ulong E8s { get; set; }

		public Tokens1(ulong e8s)
		{
			this.E8s = e8s;
		}

		public Tokens1()
		{
		}
	}
}