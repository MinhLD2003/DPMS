using DPMS_WebAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DPMS_WebAPI.ViewModels.PrivacyPolicy
{
    public class PolicyVM
    {
        [Required(ErrorMessage = "Policy code is required.")]
        [StringLength(50, ErrorMessage = "Policy code cannot exceed 50 characters.")]
        public string? PolicyCode { get; set; }
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
        public string? Title { get; set; }
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }
        [Required(ErrorMessage = "Content is required.")]
        public string? Content { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PolicyStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModifiedAt { get; set; }
        public Guid? CreatedById { get; set; }
        public Guid? LastModifiedById { get; set; }
    }
}
