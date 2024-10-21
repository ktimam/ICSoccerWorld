using EdjCase.ICP.Candid.Mapping;
using System.Collections.Generic;
using Candid.World.Models;

namespace Candid.World.Models
{
	public class ActionReturn
	{
		[CandidName("callerOutcomes")]
		public List<ActionOutcomeOption> CallerOutcomes { get; set; }

		[CandidName("callerPrincipalId")]
		public string CallerPrincipalId { get; set; }

		[CandidName("targetOutcomes")]
		public List<ActionOutcomeOption> TargetOutcomes { get; set; }

		[CandidName("targetPrincipalId")]
		public string TargetPrincipalId { get; set; }

		[CandidName("worldOutcomes")]
		public List<ActionOutcomeOption> WorldOutcomes { get; set; }

		[CandidName("worldPrincipalId")]
		public string WorldPrincipalId { get; set; }

		public ActionReturn(List<ActionOutcomeOption> callerOutcomes, string callerPrincipalId, List<ActionOutcomeOption> targetOutcomes, string targetPrincipalId, List<ActionOutcomeOption> worldOutcomes, string worldPrincipalId)
		{
			this.CallerOutcomes = callerOutcomes;
			this.CallerPrincipalId = callerPrincipalId;
			this.TargetOutcomes = targetOutcomes;
			this.TargetPrincipalId = targetPrincipalId;
			this.WorldOutcomes = worldOutcomes;
			this.WorldPrincipalId = worldPrincipalId;
		}

		public ActionReturn()
		{
		}
	}
}