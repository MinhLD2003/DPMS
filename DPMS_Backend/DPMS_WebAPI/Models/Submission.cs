namespace DPMS_WebAPI.Models
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public class Submission : BaseModel
    {

        public Guid? FormId { get; set; }
        public Form? Form { get; set; }

        public Guid? SystemId { get; set; } 
        public ExternalSystem? ExternalSystem { get; set; }
        public ICollection<FormResponse> SubmissionElements { get; set; } = new List<FormResponse>();
    }
}