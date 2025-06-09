using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels.Document;
using System.Text.Json.Serialization;

namespace DPMS_WebAPI.ViewModels.IssueTicket
{
    public class IssueTicketCreateVM
    {
        public Guid Id { get; set; }
        public Guid? ExternalSystemId { get; set; }
        public string? Title { get; set; }
        public string? ExternalSystemName { get; set; }
        public string? Description { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TicketType TicketType { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public IssueTicketStatus IssueTicketStatus { get; set; }
        public List<DocumentVM>? Documents { get; set; }
    }
}
