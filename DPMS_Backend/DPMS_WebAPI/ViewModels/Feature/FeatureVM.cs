using System.Text.Json.Serialization;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels
{
    public class FeatureVM
    {
        public Guid Id { get; set; }
        public required string FeatureName { get; set; }
        public string? Url { get; set; }
        public Guid? ParentId { get; set; } 
        public string? Description { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HttpMethodType? HttpMethod { get; set; }
        public List<FeatureVM>? Children { get; set; } = new List<FeatureVM>();
        public bool? isChecked { get; set; }
    }
}