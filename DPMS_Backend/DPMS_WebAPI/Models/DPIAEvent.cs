namespace DPMS_WebAPI.Models
{
    #pragma warning disable CS1591
    public class DPIAEvent : BaseModel
    {
        public Guid DPIAId { get; set; }
        public string Event { get; set; } = string.Empty;
        public DPIAEventType EventType { get; set; }
        public Guid UserId { get; set; }
        public virtual DPIA? DPIA { get; set; }
        public virtual User? User { get; set; }
    }

    public enum DPIAEventType
    {
        Initiated = 1, 
        Updated = 2,
        Requested = 3,
        Approved = 4,
        Rejected = 5
    }
}