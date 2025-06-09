using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DPMS_WebAPI.Models
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public class IssueTicket : BaseModel
    {
        public Guid? ExternalSystemId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public TicketType TicketType { get; set; }
        public IssueTicketStatus IssueTicketStatus { get; set; }
        public ExternalSystem? ExternalSystem { get; set; }
        public List<IssueTicketDocument> Documents { get; set; } = new List<IssueTicketDocument>();
    }

    public enum TicketType
    {
        DPIA,
        Risk,
        Violation,
        System
    }
    public enum IssueTicketStatus
    {
        Pending,
        Accept,
        Reject,
        Done
    }
}
