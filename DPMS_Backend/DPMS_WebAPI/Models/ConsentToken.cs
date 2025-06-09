using System.ComponentModel.DataAnnotations;

namespace DPMS_WebAPI.Models
{
    public class ConsentToken
    {
        [Key]
        public string? TokenString { get; set; }
        public bool IsValid { get; set; }
        public DateTime ExpireTime { get; set; }
        public Guid ExternalSystemId { get; set; }
    }
}
