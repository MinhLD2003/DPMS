namespace DPMS_WebAPI.ViewModels.Consent
{
    public class ConsentPVM
    {
        public Guid ConsentId { get; set; }
        public Guid PurposeId { get; set; }
        public bool Status { get; set; }
    }
}
