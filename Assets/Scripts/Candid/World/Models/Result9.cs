using EdjCase.ICP.Candid.Mapping;
using Candid.World.Models;
using System.Collections.Generic;
using System;

namespace Candid.World.Models
{
	[Variant]
	public class Result9
	{
		[VariantTagProperty]
		public Result9Tag Tag { get; set; }

		[VariantValueProperty]
		public object? Value { get; set; }

		public Result9(Result9Tag tag, object? value)
		{
			this.Tag = tag;
			this.Value = value;
		}

		protected Result9()
		{
		}

		public static Result9 Err(string info)
		{
			return new Result9(Result9Tag.Err, info);
		}

		public static Result9 Ok(List<ActionOutcomeHistory> info)
		{
			return new Result9(Result9Tag.Ok, info);
		}

		public string AsErr()
		{
			this.ValidateTag(Result9Tag.Err);
			return (string)this.Value!;
		}

		public List<ActionOutcomeHistory> AsOk()
		{
			this.ValidateTag(Result9Tag.Ok);
			return (List<ActionOutcomeHistory>)this.Value!;
		}

		private void ValidateTag(Result9Tag tag)
		{
			if (!this.Tag.Equals(tag))
			{
				throw new InvalidOperationException($"Cannot cast '{this.Tag}' to type '{tag}'");
			}
		}
	}

	public enum Result9Tag
	{
		[CandidName("err")]
		Err,
		[CandidName("ok")]
		Ok
	}
}