using EdjCase.ICP.Candid.Mapping;
using Candid.World.Models;
using System;
using BlockIndex = EdjCase.ICP.Candid.Models.UnboundedUInt;

namespace Candid.World.Models
{
	[Variant]
	public class TransferResult
	{
		[VariantTagProperty]
		public TransferResultTag Tag { get; set; }

		[VariantValueProperty]
		public object? Value { get; set; }

		public TransferResult(TransferResultTag tag, object? value)
		{
			this.Tag = tag;
			this.Value = value;
		}

		protected TransferResult()
		{
		}

		public static TransferResult Err(TransferError info)
		{
			return new TransferResult(TransferResultTag.Err, info);
		}

		public static TransferResult Ok(BlockIndex info)
		{
			return new TransferResult(TransferResultTag.Ok, info);
		}

		public TransferError AsErr()
		{
			this.ValidateTag(TransferResultTag.Err);
			return (TransferError)this.Value!;
		}

		public BlockIndex AsOk()
		{
			this.ValidateTag(TransferResultTag.Ok);
			return (BlockIndex)this.Value!;
		}

		private void ValidateTag(TransferResultTag tag)
		{
			if (!this.Tag.Equals(tag))
			{
				throw new InvalidOperationException($"Cannot cast '{this.Tag}' to type '{tag}'");
			}
		}
	}

	public enum TransferResultTag
	{
		Err,
		Ok
	}
}