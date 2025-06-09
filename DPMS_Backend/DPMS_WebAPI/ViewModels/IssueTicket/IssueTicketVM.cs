using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels.Document;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DPMS_WebAPI.ViewModels.IssueTicket
{
    public class IssueTicketVM
    {
        public Guid Id { get; set; }
        public Guid? ExternalSystemId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string? Title { get; set; }

        public string? ExternalSystemName { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Ticket type is required")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TicketType TicketType { get; set; }

        [Required(ErrorMessage = "Issue ticket status is required")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public IssueTicketStatus IssueTicketStatus { get; set; }

        public List<DocumentVM>? Documents { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModifiedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}
