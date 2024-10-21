using EdjCase.ICP.Candid.Mapping;

namespace Candid.World.Models
{
	public enum Result3
	{
		[CandidName("err")]
		Err,
		[CandidName("ok")]
		Ok
	}
}