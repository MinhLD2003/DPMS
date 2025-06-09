using System.Text.Json.Serialization;
using DPMS_WebAPI.Enums;

namespace DPMS_WebAPI.ViewModels.User
{
    public class UpdateUserStatusVM
    {
        public Guid Id { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserStatus Status { get; set; }
    }
}