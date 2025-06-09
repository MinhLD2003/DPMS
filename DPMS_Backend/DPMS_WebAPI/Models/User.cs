using DPMS_WebAPI.Enums;

namespace DPMS_WebAPI.Models
{
	#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
	public class User : BaseModel
	{
		public required string UserName { get; set; }
		public required string FullName { get; set; }
		public DateTime Dob { get; set; }
		public required string Email { get; set; }
		public string? Salt { get; set; }
        public string? Password { get; set; }
        public bool IsEmailConfirmed { get; set; }
		public bool IsPasswordConfirmed { get; set; }
		public DateTime? LastTimeLogin { get; set; }
		public UserStatus Status { get; set; }

		// navigational properties
		public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>(); // Một user có thể thuộc nhiều group
		public ICollection<Group> Groups { get; set; } = new List<Group>(); // Một user có thể thuộc nhiều group
		public ICollection<DPIAMember> DPIAs { get; set; } = new List<DPIAMember>(); // Một user có thể thuộc nhiều DPIA
	}
}
