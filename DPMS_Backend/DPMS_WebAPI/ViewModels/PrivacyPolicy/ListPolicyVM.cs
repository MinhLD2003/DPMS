
using DPMS_WebAPI.Models;
using System.Text.Json.Serialization;

namespace DPMS_WebAPI.ViewModels.PrivacyPolicy
{
    public class ListPolicyVM
    {
        public Guid Id { get; set; }
        public string? PolicyCode { get; set; }
        public string? Title { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PolicyStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModifiedAt { get; set; }
        public Guid? CreatedById { get; set; }
        public Guid? LastModifiedById { get; set; }
    }
}
