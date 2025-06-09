using System.Text.Json.Serialization;
using DPMS_WebAPI.Enums;

namespace DPMS_WebAPI.ViewModels.ExternalSystem
{
    public class SystemStatusVM
    {
        public Guid SystemId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ExternalSystemStatus Status { get; set; }
    }
}