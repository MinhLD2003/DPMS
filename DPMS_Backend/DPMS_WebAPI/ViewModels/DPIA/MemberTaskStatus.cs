using System.Text.Json.Serialization;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels.DPIA
{
    public class MemberTaskStatus
    {
        public Guid MemberResponsibilityId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CompletionStatus CompletionStatus { get; set; }
    }
}