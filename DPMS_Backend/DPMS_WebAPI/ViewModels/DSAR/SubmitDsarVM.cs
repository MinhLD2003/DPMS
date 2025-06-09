using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels.DSAR
{
    public class SubmitDsarVM
    {
        public string RequesterName { get; set; }
        public string RequesterEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public DSARType Type { get; set; }
        public DSARStatus? Status { get; set; }
        public DateTime? RequiredResponse { get; set; }
        public Guid? ExternalSystemId { get; set; }
    }
}
