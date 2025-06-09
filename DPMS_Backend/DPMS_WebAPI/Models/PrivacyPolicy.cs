namespace DPMS_WebAPI.Models
{
    public class PrivacyPolicy : BaseModel
    {
        public string? PolicyCode { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Content { get; set; }
        public PolicyStatus Status { get; set; }
        public virtual ICollection<Consent> Consents { get; set; }
    }
    public enum PolicyStatus
    {
        Draft = 0,
        Active = 1,
        Inactive = 2

    }   
}
