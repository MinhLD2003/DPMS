using System.ComponentModel.DataAnnotations;

namespace DPMS_WebAPI.Models
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public class IssueTicketDocument : BaseDocument
    {
        public Guid IssueTicketId { get; set; }
        public IssueTicket? IssueTicket { get; set; }
    }
}
