using EdjCase.ICP.Candid.Mapping;
using Candid.World.Models;
using System;

namespace Candid.World.Models
{
	[Variant]
	public class Result10
	{
		[VariantTagProperty]
		public Result10Tag Tag { get; set; }

		[VariantValueProperty]
		public object? Value { get; set; }

		public Result10(Result10Tag tag, object? value)
		{
			this.Tag = tag;
			this.Value = value;
		}

		protected Result10()
		{
		}

		public static Result10 Err(string info)
		{
			return new Result10(Result10Tag.Err, info);
		}

		public static Result10 Ok()
		{
			return new Result10(Result10Tag.Ok, null);
		}

		public string AsErr()
		{
			this.ValidateTag(Result10Tag.Err);
			return (string)this.Value!;
		}

		private void ValidateTag(Result10Tag tag)
		{
			if (!this.Tag.Equals(tag))
			{
				throw new InvalidOperationException($"Cannot cast '{this.Tag}' to type '{tag}'");
			}
		}
	}

	public enum Result10Tag
	{
		[CandidName("err")]
		Err,
		[CandidName("ok")]
		Ok
	}
}