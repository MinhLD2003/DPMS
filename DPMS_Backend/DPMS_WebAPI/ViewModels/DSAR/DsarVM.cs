using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels.DSAR
{
    public class DsarVM
    {
        public Guid Id { get; set; }
        public string RequesterName { get; set; }
        public string RequesterEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string Description { get; set; }
        public DSARType Type { get; set; }
        public DSARStatus Status { get; set; }
        public DateTime? RequiredResponse { get; set; }
        public DateTime? CompletedDate { get; set; }
        public Guid ExternalSystemId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModifiedAt { get; set; }
        public Guid? CreatedById { get; set; }
        public Guid? LastModifiedById { get; set; }
    }
}
