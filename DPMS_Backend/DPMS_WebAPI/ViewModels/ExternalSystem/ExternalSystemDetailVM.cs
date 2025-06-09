
using DPMS_WebAPI.Enums;
using DPMS_WebAPI.ViewModels.Purpose;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DPMS_WebAPI.ViewModels.ExternalSystem
{
    public class ExternalSystemDetailVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Domain { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string LastModifiedBy { get; set; }
        public bool? HasApiKey { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ExternalSystemStatus Status { get; set; }

        public List<SystemUserVM> Users { get; set; }
        public List<SystemGroupVM> Groups { get; set; }
        public List<PurposeVM> Purposes { get; set; } 
    }
}