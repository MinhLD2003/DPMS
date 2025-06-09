namespace DPMS_WebAPI.Models
{
    // DPIAMember model, is auditor of DPIA, will be used in the future 
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class DPIAMember : BaseModel
    {
        public Guid DPIAId { get; set; }
        public Guid UserId { get; set; }

        // navigation properties
        public DPIA? DPIA { get; set; }
        public virtual User? User { get; set; } // virtual for lazy loading
        public virtual List<MemberResponsibility> Responsibilities { get; set; } = new List<MemberResponsibility>();
    }
}