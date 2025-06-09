using DPMS_WebAPI.Enums;

namespace DPMS_WebAPI.ViewModels.User
{
    public class ProfileVM
    {
        public Guid Id { get; set; } // UserId
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Status { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime Dob { get; set; }
        public string Address { get; set; }
        public DateTime LastTimeLogin { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime LastModifiedAt { get; set; }

        public List<string> Groups { get; set; } // global groups
        public List<string> Systems { get; set; } // Systems this user manage
    }
}