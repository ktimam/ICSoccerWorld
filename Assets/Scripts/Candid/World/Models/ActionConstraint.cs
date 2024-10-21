using EdjCase.ICP.Candid.Mapping;
using System.Collections.Generic;
using Candid.World.Models;
using EdjCase.ICP.Candid.Models;
using System;

namespace Candid.World.Models
{
	public class ActionConstraint
	{
		[CandidName("entityConstraint")]
		public List<EntityConstraint> EntityConstraint { get; set; }

		[CandidName("icrcConstraint")]
		public List<IcrcTx> IcrcConstraint { get; set; }

		[CandidName("nftConstraint")]
		public List<NftTx> NftConstraint { get; set; }

		[CandidName("timeConstraint")]
		public OptionalValue<ActionConstraint.TimeConstraintValue> TimeConstraint { get; set; }

		public ActionConstraint(List<EntityConstraint> entityConstraint, List<IcrcTx> icrcConstraint, List<NftTx> nftConstraint, OptionalValue<ActionConstraint.TimeConstraintValue> timeConstraint)
		{
			this.EntityConstraint = entityConstraint;
			this.IcrcConstraint = icrcConstraint;
			this.NftConstraint = nftConstraint;
			this.TimeConstraint = timeConstraint;
		}

		public ActionConstraint()
		{
		}

		public class TimeConstraintValue
		{
			[CandidName("actionExpirationTimestamp")]
			public OptionalValue<UnboundedUInt> ActionExpirationTimestamp { get; set; }

			[CandidName("actionHistory")]
			public List<ActionConstraint.TimeConstraintValue.ActionHistoryItem> ActionHistory { get; set; }

			[CandidName("actionStartTimestamp")]
			public OptionalValue<UnboundedUInt> ActionStartTimestamp { get; set; }

			[CandidName("actionTimeInterval")]
			public OptionalValue<ActionConstraint.TimeConstraintValue.ActionTimeIntervalValue> ActionTimeInterval { get; set; }

			public TimeConstraintValue(OptionalValue<UnboundedUInt> actionExpirationTimestamp, List<ActionConstraint.TimeConstraintValue.ActionHistoryItem> actionHistory, OptionalValue<UnboundedUInt> actionStartTimestamp, OptionalValue<ActionConstraint.TimeConstraintValue.ActionTimeIntervalValue> actionTimeInterval)
			{
				this.ActionExpirationTimestamp = actionExpirationTimestamp;
				this.ActionHistory = actionHistory;
				this.ActionStartTimestamp = actionStartTimestamp;
				this.ActionTimeInterval = actionTimeInterval;
			}

			public TimeConstraintValue()
			{
			}

			[Variant]
			public class ActionHistoryItem
			{
				[VariantTagProperty]
				public ActionConstraint.TimeConstraintValue.ActionHistoryItemTag Tag { get; set; }

				[VariantValueProperty]
				public object? Value { get; set; }

				public ActionHistoryItem(ActionConstraint.TimeConstraintValue.ActionHistoryItemTag tag, object? value)
				{
					this.Tag = tag;
					this.Value = value;
				}

				protected ActionHistoryItem()
				{
				}

				public static ActionConstraint.TimeConstraintValue.ActionHistoryItem MintNft(MintNft info)
				{
					return new ActionConstraint.TimeConstraintValue.ActionHistoryItem(ActionConstraint.TimeConstraintValue.ActionHistoryItemTag.MintNft, info);
				}

				public static ActionConstraint.TimeConstraintValue.ActionHistoryItem TransferIcrc(TransferIcrc info)
				{
					return new ActionConstraint.TimeConstraintValue.ActionHistoryItem(ActionConstraint.TimeConstraintValue.ActionHistoryItemTag.TransferIcrc, info);
				}

				public static ActionConstraint.TimeConstraintValue.ActionHistoryItem UpdateAction(UpdateAction info)
				{
					return new ActionConstraint.TimeConstraintValue.ActionHistoryItem(ActionConstraint.TimeConstraintValue.ActionHistoryItemTag.UpdateAction, info);
				}

				public static ActionConstraint.TimeConstraintValue.ActionHistoryItem UpdateEntity(UpdateEntity info)
				{
					return new ActionConstraint.TimeConstraintValue.ActionHistoryItem(ActionConstraint.TimeConstraintValue.ActionHistoryItemTag.UpdateEntity, info);
				}

				public MintNft AsMintNft()
				{
					this.ValidateTag(ActionConstraint.TimeConstraintValue.ActionHistoryItemTag.MintNft);
					return (MintNft)this.Value!;
				}

				public TransferIcrc AsTransferIcrc()
				{
					this.ValidateTag(ActionConstraint.TimeConstraintValue.ActionHistoryItemTag.TransferIcrc);
					return (TransferIcrc)this.Value!;
				}

				public UpdateAction AsUpdateAction()
				{
					this.ValidateTag(ActionConstraint.TimeConstraintValue.ActionHistoryItemTag.UpdateAction);
					return (UpdateAction)this.Value!;
				}

				public UpdateEntity AsUpdateEntity()
				{
					this.ValidateTag(ActionConstraint.TimeConstraintValue.ActionHistoryItemTag.UpdateEntity);
					return (UpdateEntity)this.Value!;
				}

				private void ValidateTag(ActionConstraint.TimeConstraintValue.ActionHistoryItemTag tag)
				{
					if (!this.Tag.Equals(tag))
					{
						throw new InvalidOperationException($"Cannot cast '{this.Tag}' to type '{tag}'");
					}
				}
			}

			public enum ActionHistoryItemTag
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

			public class ActionTimeIntervalValue
			{
				[CandidName("actionsPerInterval")]
				public UnboundedUInt ActionsPerInterval { get; set; }

				[CandidName("intervalDuration")]
				public UnboundedUInt IntervalDuration { get; set; }

				public ActionTimeIntervalValue(UnboundedUInt actionsPerInterval, UnboundedUInt intervalDuration)
				{
					this.ActionsPerInterval = actionsPerInterval;
					this.IntervalDuration = intervalDuration;
				}

				public ActionTimeIntervalValue()
				{
				}
			}
		}
	}
}