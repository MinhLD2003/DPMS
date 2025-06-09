using DPMS_WebAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace DPMS_WebAPI.Models
{
	#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

	public class ExternalSystem : BaseModel
	{
		[MaxLength(255)]
		public required string Name { get; set; }
        [MaxLength(255)]
        public string Domain { get; set; } = string.Empty;
        public string? Description { get; set; }
		public ExternalSystemStatus Status { get; set; }
		public string? ApiKeyHash { get; set; }

        // Navigation Property
        public ICollection<ExternalSystemPurpose> Purposes { get; set; } = new List<ExternalSystemPurpose>();
        public ICollection<DSAR> DSARs { get; set; } = new List<DSAR>();
        public ICollection<Submission> FormSubmission { get; set; } = new List<Submission>(); // An external system may have many FormSubmissions
        public ICollection<Consent>? Consents { get; set; }
		public ICollection<Group> Groups { get; set; } = new List<Group>(); // An external system may have many Groups belong to it
	}
}
