using System.ComponentModel.DataAnnotations;

namespace DPMS_WebAPI.Models
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public abstract class BaseDocument : BaseModel
    {
        [Required]
        [MaxLength(200)]
        public required string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public required string FileUrl { get; set; } = string.Empty;

        [MaxLength(300)]
        public required string? FileFormat { get; set; }
    }
}