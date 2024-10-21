using EdjCase.ICP.Candid.Mapping;

namespace Candid.IcrcLedger.Models
{
	public class StandardRecord
	{
		[CandidName("url")]
		public string Url { get; set; }

		[CandidName("name")]
		public string Name { get; set; }

		public StandardRecord(string url, string name)
		{
			this.Url = url;
			this.Name = name;
		}

		public StandardRecord()
		{
		}
	}
}