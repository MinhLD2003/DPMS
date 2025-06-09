namespace DPMS_WebAPI.Models
{
	#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

	public class UserGroup : BaseModel
	{
		public Guid UserId { get; set; }
		public Guid GroupId { get; set; }
		public bool IsPic { get; set; } = false;

		// navigation properties
		public User? User { get; set; }
		public Group? Group { get; set; }
	}
}
