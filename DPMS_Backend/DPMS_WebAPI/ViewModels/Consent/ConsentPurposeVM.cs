namespace DPMS_WebAPI.ViewModels.Consent
{
    /// <summary>
    /// Used in SubmitConsent API
    /// </summary>
    public class ConsentPurposeVM
    {
        public Guid PurposeId { get; set; }
        public bool Status { get; set; }
    }
}
