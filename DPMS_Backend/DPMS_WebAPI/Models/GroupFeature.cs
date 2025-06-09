namespace DPMS_WebAPI.Models
{
	#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

	public class GroupFeature : BaseModel
	{
		public Guid GroupId { get; set; }
		public Guid FeatureId { get; set; }

		// navigation properties
		public Group? Group { get; set; }
		public Feature? Feature { get; set; }
	}
}
