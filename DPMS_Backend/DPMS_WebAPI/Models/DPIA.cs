using DPMS_WebAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace DPMS_WebAPI.Models
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

	public class DPIA : BaseModel
	{
		public Guid? ExternalSystemId { get; set; }

		[MaxLength(100)]
		public string? Title { get; set; }
		public string? Description { get; set; }
		public DPIAStatus Status { get; set; }
		public DateTime? DueDate { get; set; }
		public DPIAType Type { get; set; }
		public ExternalSystem? ExternalSystem { get; set; }
		public List<DPIAMember> DPIAMembers { get; set; } = new List<DPIAMember>();
		public List<DPIADocument> Documents { get; set; } = new List<DPIADocument>();
		public List<DPIAEvent> Events { get; set; } = new List<DPIAEvent>();
		public List<DPIAResponsibility> Responsibilities { get; set; } = new List<DPIAResponsibility>();
	}

    public enum DPIAType
    {
        NewOrUpdatedSystem,   // DPIA for new or updated external systems
        PeriodicReview,       // DPIA performed periodically
        DataBreach            // DPIA due to a data breach
    }

}
