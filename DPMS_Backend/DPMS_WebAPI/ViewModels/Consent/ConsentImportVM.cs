using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels.Consent
{
    public class ConsentImportVM
    {
        public string Email { get; set; }
        public ConsentMethod ConsentMethod { get; set; }
        public DateTime ConsentDate { get; set; }

        public Guid ExternalSystemId { get; set; }  // Collect at which system
        public Guid PrivacyPolicyId { get; set; }
        public List<ConsentPurposeImportVM> Purposes { get; set; } = new List<ConsentPurposeImportVM>();
    }

    public class ConsentPurposeImportVM
    {
        public Guid PurposeId { get; set; }
        public bool Status { get; set; }
    }
}
