using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DPMS_WebAPI.Enums;

namespace DPMS_WebAPI.ViewModels.ExternalSystem
{
	public class ExternalSystemVM
	{
		public Guid Id { get; set; }
		public required string Name { get; set; }
        [MaxLength(255)]
        public required string Domain { get; set; }
        public required string Description { get; set; }
		public DateTime CreatedAt { get; set; }
		public string? CreateBy  { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public ExternalSystemStatus Status { get; set; }
	}
}
