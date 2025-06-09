using System.Text.Json.Serialization;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels
{
    public class FeatureDetailVM
    {
        public Guid Id { get; set; }
        public required string FeatureName { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HttpMethodType? HttpMethod { get; set; }
        public List<FeatureVM> Children { get; set; } = new List<FeatureVM>();
    }
}