using DPMS_WebAPI.Models;
using System.Text.Json.Serialization;

namespace DPMS_WebAPI.ViewModels
{
    public class DPIAListVM
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public string? SystemName { get; set; }
        public Guid? SystemId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DPIAType Type { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}