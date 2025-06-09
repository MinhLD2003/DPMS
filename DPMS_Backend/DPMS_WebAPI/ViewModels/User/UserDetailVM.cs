using DPMS_WebAPI.Enums;

namespace DPMS_WebAPI.ViewModels.User
{
    public class UserDetailVM
    {
        public Guid Id { get; set; }
        public required string UserName { get; set; }
		public required string FullName { get; set; }
		public DateTime Dob { get; set; }
		public string? Email { get; set; }
		public string? Salt { get; set; }
		public string? Password { get; set; }
		public bool IsEmailConfirmed { get; set; }
		public bool IsPasswordConfirmed { get; set; }
		public DateTime? LastTimeLogin { get; set; }
		public UserStatus Status { get; set; }
    }
}