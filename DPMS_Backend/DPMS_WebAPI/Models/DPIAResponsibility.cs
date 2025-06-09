namespace DPMS_WebAPI.Models
{
    #pragma warning disable CS1591
    
    public class DPIAResponsibility : BaseModel
    {
        public Guid DPIAId { get; set; }
        public Guid ResponsibilityId { get; set; }
        public DateTime? DueDate { get; set; }
        public ResponsibilityStatus Status { get; set; }
        public string? Comment { get; set; }
        // navigation properties
        public DPIA? DPIA { get; set; }
        public virtual Responsibility? Responsibility { get; set; }
        public virtual ICollection<MemberResponsibility>? MemberResponsibilities { get; set; }
    }

    public enum ResponsibilityStatus
    {
        NotStarted,
        Ready,
        InProgress,
        Completed
    }
}