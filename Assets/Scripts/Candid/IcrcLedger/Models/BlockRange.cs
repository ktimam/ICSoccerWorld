using EdjCase.ICP.Candid.Mapping;
using Candid.IcrcLedger.Models;
using System.Collections.Generic;
using Block = Candid.IcrcLedger.Models.Value;

namespace Candid.IcrcLedger.Models
{
	public class BlockRange
	{
		[CandidName("blocks")]
		public BlockRange.BlocksInfo Blocks { get; set; }

		public BlockRange(BlockRange.BlocksInfo blocks)
		{
			this.Blocks = blocks;
		}

		public BlockRange()
		{
		}

		public class BlocksInfo : List<Block>
		{
			public BlocksInfo()
			{
			}
		}
	}
}