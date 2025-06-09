using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels.Consent
{
    public class ConsentLogVM
    {
        public string? DataSubjectId { get; set; }
        public string? Email { get; set; }
        public ConsentMethod ConsentMethod { get; set; }
        public string ConsentIp { get; set; }
        public string ConsentUserAgent { get; set; }
        public string ExternalSystemName { get; set; }
        public bool? IsWithdrawn { get; set; }
        public DateTime WithdrawnDate { get; set; } 
        public List<ConsentPurposeLogVM> ConsentPurpose { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ConsentDate {  get; set; }
    }
    public class ConsentPurposeLogVM
    {
        public string Name { get; set; }
        public bool Status { get; set; }
    }
}
