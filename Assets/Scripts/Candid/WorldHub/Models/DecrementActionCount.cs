using EdjCase.ICP.Candid.Mapping;
using Candid.WorldHub.Models;
using System;

namespace Candid.WorldHub.Models
{
	public class DecrementActionCount
	{
		[CandidName("value")]
		public DecrementActionCount.ValueInfo Value { get; set; }

		public DecrementActionCount(DecrementActionCount.ValueInfo value)
		{
			this.Value = value;
		}

		public DecrementActionCount()
		{
		}

		[Variant]
		public class ValueInfo
		{
			[VariantTagProperty]
			public DecrementActionCount.ValueInfoTag Tag { get; set; }

			[VariantValueProperty]
			public object? Value { get; set; }

			public ValueInfo(DecrementActionCount.ValueInfoTag tag, object? value)
			{
				this.Tag = tag;
				this.Value = value;
			}

			protected ValueInfo()
			{
			}

			public static DecrementActionCount.ValueInfo Formula(string info)
			{
				return new DecrementActionCount.ValueInfo(DecrementActionCount.ValueInfoTag.Formula, info);
			}

			public static DecrementActionCount.ValueInfo Number(double info)
			{
				return new DecrementActionCount.ValueInfo(DecrementActionCount.ValueInfoTag.Number, info);
			}

			public string AsFormula()
			{
				this.ValidateTag(DecrementActionCount.ValueInfoTag.Formula);
				return (string)this.Value!;
			}

			public double AsNumber()
			{
				this.ValidateTag(DecrementActionCount.ValueInfoTag.Number);
				return (double)this.Value!;
			}

			private void ValidateTag(DecrementActionCount.ValueInfoTag tag)
			{
				if (!this.Tag.Equals(tag))
				{
					throw new InvalidOperationException($"Cannot cast '{this.Tag}' to type '{tag}'");
				}
			}
		}

		public enum ValueInfoTag
		{
			[CandidName("formula")]
			Formula,
			[CandidName("number")]
			Number
		}
	}
}