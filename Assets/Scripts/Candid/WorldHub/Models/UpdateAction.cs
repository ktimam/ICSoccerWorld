using EdjCase.ICP.Candid.Mapping;
using System.Collections.Generic;
using Candid.WorldHub.Models;
using ActionId = System.String;

namespace Candid.WorldHub.Models
{
	public class UpdateAction
	{
		[CandidName("aid")]
		public ActionId Aid { get; set; }

		[CandidName("updates")]
		public List<UpdateActionType> Updates { get; set; }

		public UpdateAction(ActionId aid, List<UpdateActionType> updates)
		{
			this.Aid = aid;
			this.Updates = updates;
		}

		public UpdateAction()
		{
		}
	}
}