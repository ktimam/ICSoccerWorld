using EdjCase.ICP.Candid.Mapping;
using System.Collections.Generic;
using Candid.World.Models;
using EdjCase.ICP.Candid.Models;

namespace Candid.World.Models
{
	public class ActionStatusReturn
	{
		[CandidName("actionHistoryStatus")]
		public List<ConstraintStatus> ActionHistoryStatus { get; set; }

		[CandidName("entitiesStatus")]
		public List<ConstraintStatus> EntitiesStatus { get; set; }

		[CandidName("isValid")]
		public bool IsValid { get; set; }

		[CandidName("timeStatus")]
		public ActionStatusReturn.TimeStatusInfo TimeStatus { get; set; }

		public ActionStatusReturn(List<ConstraintStatus> actionHistoryStatus, List<ConstraintStatus> entitiesStatus, bool isValid, ActionStatusReturn.TimeStatusInfo timeStatus)
		{
			this.ActionHistoryStatus = actionHistoryStatus;
			this.EntitiesStatus = entitiesStatus;
			this.IsValid = isValid;
			this.TimeStatus = timeStatus;
		}

		public ActionStatusReturn()
		{
		}

		public class TimeStatusInfo
		{
			[CandidName("actionsLeft")]
			public OptionalValue<UnboundedUInt> ActionsLeft { get; set; }

			[CandidName("nextAvailableTimestamp")]
			public OptionalValue<UnboundedUInt> NextAvailableTimestamp { get; set; }

			public TimeStatusInfo(OptionalValue<UnboundedUInt> actionsLeft, OptionalValue<UnboundedUInt> nextAvailableTimestamp)
			{
				this.ActionsLeft = actionsLeft;
				this.NextAvailableTimestamp = nextAvailableTimestamp;
			}

			public TimeStatusInfo()
			{
			}
		}
	}
}