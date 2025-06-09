namespace DPMS_WebAPI.Models
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public class Comment : BaseModel
    {
        public Guid ReferenceId { get; set; }
        public Guid UserId { get; set; }
        public CommentType Type { get; set; }
        public string Content { get; set; } = string.Empty;
        // navigation properties
        public virtual User? User { get; set; }

    }

    public enum CommentType
    {
        DPIA = 1,
        DPIAResponsibility = 2, 
        IssueTicket = 3,
        Document = 4,
        Review = 5
    }
}