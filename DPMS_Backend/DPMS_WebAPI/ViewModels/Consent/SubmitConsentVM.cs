using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels.Consent
{
    /// <summary>
    /// VM used in SubmitConsent API
    /// </summary>
    public class SubmitConsentVM
    {
        public string? DataSubjectId { get; set; }
        public string? Email { get; set; }
        public ConsentMethod ConsentMethod { get; set; }
        public string ConsentIp { get; set; }
        public string ConsentUserAgent { get; set; }
        public Guid PrivacyPolicyId { get; set; }
        public Guid ExternalSystemId { get; set; }
        public bool? IsWithdrawn { get; set; }
        public DateTime WithdrawnDate { get; set; } // ???

        public List<ConsentPurposeVM> ConsentPurposes { get; set; }
    }
}
