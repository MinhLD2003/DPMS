namespace DPMS_WebAPI.Models
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public class ExternalSystemPurpose : BaseModel
    {
        public Guid ExternalSystemId { get; set; }
        public Guid PurposeId { get; set; }

        // navigation properties
        public ExternalSystem? ExternalSystem { get; set; }
        public Purpose? Purpose { get; set; }
    }
}
