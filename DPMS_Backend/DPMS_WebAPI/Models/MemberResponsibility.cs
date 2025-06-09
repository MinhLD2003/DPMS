namespace DPMS_WebAPI.Models
{
    #pragma warning disable CS1591
    
    public class MemberResponsibility : BaseModel
    {
        public Guid MemberId { get; set; }
        public Guid DPIAResponsibilityId { get; set; }
        // public bool IsCompleted { get; set; } = false;
        public CompletionStatus CompletionStatus { get; set; } = CompletionStatus.NotStarted;
        public bool IsPic { get; set; } = false;
        // navigation properties
        public DPIAMember? Member { get; set; }
        public virtual DPIAResponsibility? DPIAResponsibility  { get; set; }
    }

    public enum CompletionStatus
    {
        NotStarted,
        InProgress,
        Completed
    }
}