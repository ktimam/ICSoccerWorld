using EdjCase.ICP.Candid.Mapping;
using Candid.World.Models;
using System;
using Blockindex1 = System.UInt64;

namespace Candid.World.Models
{
	[Variant]
	public class Transferresult1
	{
		[VariantTagProperty]
		public Transferresult1Tag Tag { get; set; }

		[VariantValueProperty]
		public object? Value { get; set; }

		public Transferresult1(Transferresult1Tag tag, object? value)
		{
			this.Tag = tag;
			this.Value = value;
		}

		protected Transferresult1()
		{
		}

		public static Transferresult1 Err(Transfererror1 info)
		{
			return new Transferresult1(Transferresult1Tag.Err, info);
		}

		public static Transferresult1 Ok(Blockindex1 info)
		{
			return new Transferresult1(Transferresult1Tag.Ok, info);
		}

		public Transfererror1 AsErr()
		{
			this.ValidateTag(Transferresult1Tag.Err);
			return (Transfererror1)this.Value!;
		}

		public Blockindex1 AsOk()
		{
			this.ValidateTag(Transferresult1Tag.Ok);
			return (Blockindex1)this.Value!;
		}

		private void ValidateTag(Transferresult1Tag tag)
		{
			if (!this.Tag.Equals(tag))
			{
				throw new InvalidOperationException($"Cannot cast '{this.Tag}' to type '{tag}'");
			}
		}
	}

	public enum Transferresult1Tag
	{
		Err,
		Ok
	}
}