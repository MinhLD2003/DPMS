using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels.Comment;

namespace DPMS_WebAPI.ViewModels.DPIA
{
    public class DPIAResponsibilityDetailVM
    {
        public Guid Id { get; set; }
        public Guid ResponsibilityId { get; set; }
        public string? ResponsibilityName { get; set; }
        public string? ResponsibilityDescription { get; set; }
        public DateTime? DueDate { get; set; }
        public List<CommentVM> Comments { get; set; } = new List<CommentVM>();
        public ResponsibilityStatus Status { get; set; }
        public List<MemberResponsibilityVM> Members { get; set; } = new List<MemberResponsibilityVM>();
        public List<DPIADocument> Documents { get; set; } = new List<DPIADocument>();
    }
    

}