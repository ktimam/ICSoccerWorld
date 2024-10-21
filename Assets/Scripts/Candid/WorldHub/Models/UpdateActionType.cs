using EdjCase.ICP.Candid.Mapping;
using Candid.WorldHub.Models;
using System;

namespace Candid.WorldHub.Models
{
	[Variant]
	public class UpdateActionType
	{
		[VariantTagProperty]
		public UpdateActionTypeTag Tag { get; set; }

		[VariantValueProperty]
		public object? Value { get; set; }

		public UpdateActionType(UpdateActionTypeTag tag, object? value)
		{
			this.Tag = tag;
			this.Value = value;
		}

		protected UpdateActionType()
		{
		}

		public static UpdateActionType DecrementActionCount(DecrementActionCount info)
		{
			return new UpdateActionType(UpdateActionTypeTag.DecrementActionCount, info);
		}

		public DecrementActionCount AsDecrementActionCount()
		{
			this.ValidateTag(UpdateActionTypeTag.DecrementActionCount);
			return (DecrementActionCount)this.Value!;
		}

		private void ValidateTag(UpdateActionTypeTag tag)
		{
			if (!this.Tag.Equals(tag))
			{
				throw new InvalidOperationException($"Cannot cast '{this.Tag}' to type '{tag}'");
			}
		}
	}

	public enum UpdateActionTypeTag
	{
		[CandidName("decrementActionCount")]
		DecrementActionCount
	}
}