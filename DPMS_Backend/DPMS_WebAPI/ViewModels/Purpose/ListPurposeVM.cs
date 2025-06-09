using DPMS_WebAPI.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DPMS_WebAPI.ViewModels.Purpose
{
    public class ListPurposeVM
    {
        [BindNever]
        public Guid Id { get; set; }
        [MaxLength(255)]
        public string? Name { get; set; }
        public string Description { get; set; }

        [EnumDataType(typeof(PurposeStatus))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PurposeStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}
