using EdjCase.ICP.Candid.Mapping;
using Candid.World.Models;
using System;

namespace Candid.World.Models
{
	[Variant]
	public class Result8
	{
		[VariantTagProperty]
		public Result8Tag Tag { get; set; }

		[VariantValueProperty]
		public object? Value { get; set; }

		public Result8(Result8Tag tag, object? value)
		{
			this.Tag = tag;
			this.Value = value;
		}

		protected Result8()
		{
		}

		public static Result8 Err(string info)
		{
			return new Result8(Result8Tag.Err, info);
		}

		public static Result8 Ok(ActionStatusReturn info)
		{
			return new Result8(Result8Tag.Ok, info);
		}

		public string AsErr()
		{
			this.ValidateTag(Result8Tag.Err);
			return (string)this.Value!;
		}

		public ActionStatusReturn AsOk()
		{
			this.ValidateTag(Result8Tag.Ok);
			return (ActionStatusReturn)this.Value!;
		}

		private void ValidateTag(Result8Tag tag)
		{
			if (!this.Tag.Equals(tag))
			{
				throw new InvalidOperationException($"Cannot cast '{this.Tag}' to type '{tag}'");
			}
		}
	}

	public enum Result8Tag
	{
		[CandidName("err")]
		Err,
		[CandidName("ok")]
		Ok
	}
}