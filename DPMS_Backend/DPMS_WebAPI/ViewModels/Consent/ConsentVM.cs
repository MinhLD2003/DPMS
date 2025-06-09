using DPMS_WebAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace DPMS_WebAPI.ViewModels.Consent
{
    public class ConsentVM
    {
        public Guid Id { get; set; }
        public string? DataSubjectId { get; set; }

        [StringLength(255)]
        public string? Email { get; set; }

        public string ConsentMethod { get; set; }

        [StringLength(45)]
        public string? ConsentIp { get; set; }

        [StringLength(512)]
        public string? ConsentUserAgent { get; set; }

        public DateTime ConsentDate { get; set; }

        public Guid ExternalSystemId { get; set; }  // Collect at which system
        public string ExternalSystemName { get; set; }

        public bool IsWithdrawn { get; set; } // Withdrawn all consent 

        public DateTime? WithdrawnDate { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
