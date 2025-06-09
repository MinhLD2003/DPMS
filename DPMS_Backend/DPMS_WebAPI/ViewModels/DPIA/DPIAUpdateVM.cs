using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels.DPIA
{
    public class DPIAUpdateVM
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
    }


    public class DPIAResponsibilityUpdateVM
    {
        public required Guid ResponsibilityId { get; set; } // Maps to DPIAResponsibility
        public List<Guid> UserId { get; set; }
        public Guid Pic { get; set; } // Maps to UserId in DPIAResponsibility
    }

    public class DPIAResponsibilityUpdateStatusVM
    {
        public Guid DPIAResponsibilityId { get; set; }
        public ResponsibilityStatus Status { get; set; }
    }

    public class DPIAResponsibilityMemberUpdateVM
    {
        public List<Guid> UserIds { get; set; } // Maps to UserId in DPIAResponsibility
        public Guid? Pic { get; set; } // Maps to UserId in DPIAResponsibility
    }
}