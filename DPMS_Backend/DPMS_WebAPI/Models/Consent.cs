using System.ComponentModel.DataAnnotations;

namespace DPMS_WebAPI.Models
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public class Consent : BaseModel
    {
        public string? DataSubjectId { get; set; }
        [StringLength(255)]
        public string? Email { get; set; }
        public ConsentMethod ConsentMethod { get; set; }
        [StringLength(45)]
        public string? ConsentIp { get; set; }
        [StringLength(512)]
        public string? ConsentUserAgent { get; set; }
        public DateTime ConsentDate { get; set; }
        public Guid PrivacyPolicyId {  get; set; }
        public virtual PrivacyPolicy PrivacyPolicy { get; set; }
        public Guid ExternalSystemId { get; set; }  // Collect at which system
        public virtual ExternalSystem ExternalSystem { get; set; }

        public bool IsWithdrawn { get; set; }

        public DateTime? WithdrawnDate { get; set; } 
        public virtual ICollection<ConsentPurpose> Purposes { get; set; }
        
    }
    public enum ConsentMethod
    {
        WebForm,
        Email,
        Form // Dien bang giay
    }
}
