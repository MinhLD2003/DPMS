using DPMS_WebAPI.Enums;
using DPMS_WebAPI.ViewModels;

namespace DPMS_WebAPI.ViewModels.ExternalSystem
{
    public class SystemUserVM
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public List<SystemGroupVM> Groups { get; set; }
        public string Email { get; set; }
        public UserStatus Status { get; set; }
        public DateTime? LastTimeLogin { get; set; }
    }

    public class SystemGroupVM
    {
        public Guid Id  { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

}
