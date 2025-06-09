using DPMS_WebAPI.Enums;

namespace DPMS_WebAPI.Models
{
    public class DSAR : BaseModel
    {
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
        public ExternalSystem ExternalSystem { get; set; }
    }

    public enum DSARType
    {
        Access,
        Delete
    }

    public enum DSARStatus
    {
        Submitted,
        RequiredReponse,
        Completed,
        Rejected
    }
}
