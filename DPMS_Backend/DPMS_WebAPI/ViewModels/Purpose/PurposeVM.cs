using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels.Document;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DPMS_WebAPI.ViewModels.Purpose
{
    public class PurposeVM
    {
        [BindNever]
        public Guid Id { get; set; }
        [MaxLength(500, ErrorMessage = "Purpose name cannot exceed 500 characters ")]
        public string Name { get; set; }
        public string Description { get; set; }

        [EnumDataType(typeof(PurposeStatus))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PurposeStatus Status { get; set; }
      
    }
}
