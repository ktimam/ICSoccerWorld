using EdjCase.ICP.Candid.Mapping;
using SoccerSim.SoccerSimClient.Models;
using System;
using StatusCode = System.UInt16;

namespace SoccerSim.SoccerSimClient.Models
{
	[Variant(typeof(ResultTag))]
	public class Result
	{
		[VariantTagProperty]
		public ResultTag Tag { get; set; }

		[VariantValueProperty]
		public object? Value { get; set; }

		public Result(ResultTag tag, object? value)
		{
			this.Tag = tag;
			this.Value = value;
		}

		protected Result()
		{
		}

		public static Result Err(StatusCode info)
		{
			return new Result(ResultTag.Err, info);
		}

		public static Result Ok(string info)
		{
			return new Result(ResultTag.Ok, info);
		}

		public StatusCode AsErr()
		{
			this.ValidateTag(ResultTag.Err);
			return (StatusCode)this.Value!;
		}

		public string AsOk()
		{
			this.ValidateTag(ResultTag.Ok);
			return (string)this.Value!;
		}

		private void ValidateTag(ResultTag tag)
		{
			if (!this.Tag.Equals(tag))
			{
				throw new InvalidOperationException($"Cannot cast '{this.Tag}' to type '{tag}'");
			}
		}
	}

	public enum ResultTag
	{
		[CandidName("err")]
		Err,
		[CandidName("ok")]
		Ok
	}
}