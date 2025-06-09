using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels.DPIA
{
    public class DPIACreateVM
    {
        public required string Title { get; set; }
        public required Guid ExternalSystemId { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public DPIAType Type { get; set; }
        public List<IFormFile>? Documents { get; set; }
        public List<DPIAResponsibilityCreateVM> Responsibilities { get; set; }
    }

    public class DPIAResponsibilityCreateVM
    {
        public DateTime? DueDate { get; set; }
        public Guid ResponsibilityId { get; set; }
        public List<Guid> UserIds { get; set; }
        public Guid? Pic { get; set; }
    }

    public class DPIAMemberCreateVM
    {
        public required Guid UserId { get; set; }
        public required List<Guid> Responsibilities { get; set; }
    }

    public class DPIAResponsibilityVM
    {
        public Guid ResponsibilityId { get; set; }
        public DateTime? DueDate { get; set; }
        public List<Guid> UserId { get; set; }
        public Guid Pic { get; set; }
    }
}