using EdjCase.ICP.Candid.Mapping;
using EdjCase.ICP.Candid.Models;
using Candid.World.Models;
using System;
using WorldId = System.String;

namespace Candid.World.Models
{
	public class ActionOutcomeHistory
	{
		[CandidName("appliedAt")]
		public UnboundedUInt AppliedAt { get; set; }

		[CandidName("option")]
		public ActionOutcomeHistory.OptionInfo Option { get; set; }

		[CandidName("wid")]
		public WorldId Wid { get; set; }

		public ActionOutcomeHistory(UnboundedUInt appliedAt, ActionOutcomeHistory.OptionInfo option, WorldId wid)
		{
			this.AppliedAt = appliedAt;
			this.Option = option;
			this.Wid = wid;
		}

		public ActionOutcomeHistory()
		{
		}

		[Variant]
		public class OptionInfo
		{
			[VariantTagProperty]
			public ActionOutcomeHistory.OptionInfoTag Tag { get; set; }

			[VariantValueProperty]
			public object? Value { get; set; }

			public OptionInfo(ActionOutcomeHistory.OptionInfoTag tag, object? value)
			{
				this.Tag = tag;
				this.Value = value;
			}

			protected OptionInfo()
			{
			}

			public static ActionOutcomeHistory.OptionInfo MintNft(MintNft info)
			{
				return new ActionOutcomeHistory.OptionInfo(ActionOutcomeHistory.OptionInfoTag.MintNft, info);
			}

			public static ActionOutcomeHistory.OptionInfo TransferIcrc(TransferIcrc info)
			{
				return new ActionOutcomeHistory.OptionInfo(ActionOutcomeHistory.OptionInfoTag.TransferIcrc, info);
			}

			public static ActionOutcomeHistory.OptionInfo UpdateAction(UpdateAction info)
			{
				return new ActionOutcomeHistory.OptionInfo(ActionOutcomeHistory.OptionInfoTag.UpdateAction, info);
			}

			public static ActionOutcomeHistory.OptionInfo UpdateEntity(UpdateEntity info)
			{
				return new ActionOutcomeHistory.OptionInfo(ActionOutcomeHistory.OptionInfoTag.UpdateEntity, info);
			}

			public MintNft AsMintNft()
			{
				this.ValidateTag(ActionOutcomeHistory.OptionInfoTag.MintNft);
				return (MintNft)this.Value!;
			}

			public TransferIcrc AsTransferIcrc()
			{
				this.ValidateTag(ActionOutcomeHistory.OptionInfoTag.TransferIcrc);
				return (TransferIcrc)this.Value!;
			}

			public UpdateAction AsUpdateAction()
			{
				this.ValidateTag(ActionOutcomeHistory.OptionInfoTag.UpdateAction);
				return (UpdateAction)this.Value!;
			}

			public UpdateEntity AsUpdateEntity()
			{
				this.ValidateTag(ActionOutcomeHistory.OptionInfoTag.UpdateEntity);
				return (UpdateEntity)this.Value!;
			}

			private void ValidateTag(ActionOutcomeHistory.OptionInfoTag tag)
			{
				if (!this.Tag.Equals(tag))
				{
					throw new InvalidOperationException($"Cannot cast '{this.Tag}' to type '{tag}'");
				}
			}
		}

		public enum OptionInfoTag
		{
			[CandidName("mintNft")]
			MintNft,
			[CandidName("transferIcrc")]
			TransferIcrc,
			[CandidName("updateAction")]
			UpdateAction,
			[CandidName("updateEntity")]
			UpdateEntity
		}
	}
}