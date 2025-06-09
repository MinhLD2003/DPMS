namespace DPMS_WebAPI.Models
{
    public class ConsentPurpose : BaseModel
    {
        public Guid ConsentId { get; set; }
        public Guid PurposeId { get; set; }
        public bool Status { get; set; }

        public virtual Consent Consent { get; set; }
        public virtual Purpose Purpose { get; set; }
    }
}
