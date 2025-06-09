namespace DPMS_WebAPI.ViewModels.Consent
{
    public class DpmsConsentVM
    {
        public Guid PurposeId { get; set; }
        public string PurposeName { get; set; }
        public string Status { get; set; } // "True", "False", or "null"
    }
}
