using DPMS_WebAPI.Enums;

namespace DPMS_WebAPI.ViewModels.User
{
    public class UserListVM
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public DateTime Dob { get; set; }
        public required string Email { get; set; }
        public DateTime? LastTimeLogin { get; set; }
        public UserStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<GroupVM> Groups { get; set; } // groups of this user
    }
}