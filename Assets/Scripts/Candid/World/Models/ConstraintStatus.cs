using EdjCase.ICP.Candid.Mapping;

namespace Candid.World.Models
{
	public class ConstraintStatus
	{
		[CandidName("currentValue")]
		public string CurrentValue { get; set; }

		[CandidName("eid")]
		public string Eid { get; set; }

		[CandidName("expectedValue")]
		public string ExpectedValue { get; set; }

		[CandidName("fieldName")]
		public string FieldName { get; set; }

		public ConstraintStatus(string currentValue, string eid, string expectedValue, string fieldName)
		{
			this.CurrentValue = currentValue;
			this.Eid = eid;
			this.ExpectedValue = expectedValue;
			this.FieldName = fieldName;
		}

		public ConstraintStatus()
		{
		}
	}
}