using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels.Consent
{
    #pragma warning disable CS1591
    public class SubmitConsentRequestVM
    {
        public string? UniqueIdentifier { get; set; }
        public string? TokenString { get; set; }

        public List<ConsentPurposeVM> ConsentPurposes { get; set; } = new List<ConsentPurposeVM>();
    }
}
