using System.Text.Json.Serialization;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels
{
    public class CreateFeatureVM
    {
        public required string FeatureName { get; set; }
        public string? Description { get; set; }
        public Guid? ParentId { get; set; }
        public string? Url { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public HttpMethodType? HttpMethod { get; set; }
    }
}