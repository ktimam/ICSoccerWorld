using EdjCase.ICP.Candid.Mapping;
using System.Collections.Generic;
using Candid.World.Models;
using ActionId = System.String;

namespace Candid.World.Models
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