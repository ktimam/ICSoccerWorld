using EdjCase.ICP.Candid.Mapping;
using Candid.World.Models;
using System.Collections.Generic;
using System;

namespace Candid.World.Models
{
	[Variant]
	public class Result7
	{
		[VariantTagProperty]
		public Result7Tag Tag { get; set; }

		[VariantValueProperty]
		public object? Value { get; set; }

		public Result7(Result7Tag tag, object? value)
		{
			this.Tag = tag;
			this.Value = value;
		}

		protected Result7()
		{
		}

		public static Result7 Err(string info)
		{
			return new Result7(Result7Tag.Err, info);
		}

		public static Result7 Ok(List<ActionState> info)
		{
			return new Result7(Result7Tag.Ok, info);
		}

		public string AsErr()
		{
			this.ValidateTag(Result7Tag.Err);
			return (string)this.Value!;
		}

		public List<ActionState> AsOk()
		{
			this.ValidateTag(Result7Tag.Ok);
			return (List<ActionState>)this.Value!;
		}

		private void ValidateTag(Result7Tag tag)
		{
			if (!this.Tag.Equals(tag))
			{
				throw new InvalidOperationException($"Cannot cast '{this.Tag}' to type '{tag}'");
			}
		}
	}

	public enum Result7Tag
	{
		[CandidName("err")]
		Err,
		[CandidName("ok")]
		Ok
	}
}