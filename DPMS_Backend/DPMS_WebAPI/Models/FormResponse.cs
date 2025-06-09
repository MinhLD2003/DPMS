namespace DPMS_WebAPI.Models
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public class FormResponse : BaseModel
    {
        public Guid SubmissionId { get; set; }
        
        public Guid FormElementId { get; set; }
        public string? Value { get; set; }

        // navigation properties
        public Submission? Submission { get; set; }
        public FormElement? FormElement { get; set; }
    }
}