using EdjCase.ICP.Candid.Mapping;

namespace Candid.WorldDeployer.Models
{
	public class World
	{
		[CandidName("canister")]
		public string Canister { get; set; }

		[CandidName("cover")]
		public string Cover { get; set; }

		[CandidName("name")]
		public string Name { get; set; }

		public World(string canister, string cover, string name)
		{
			this.Canister = canister;
			this.Cover = cover;
			this.Name = name;
		}

		public World()
		{
		}
	}
}