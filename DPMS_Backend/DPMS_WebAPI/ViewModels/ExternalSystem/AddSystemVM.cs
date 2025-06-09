using System.ComponentModel.DataAnnotations;

namespace DPMS_WebAPI.ViewModels.ExternalSystem
{
	public class AddSystemVM
	{
		public required string Name { get; set; }
		public required string Description { get; set; }
        [MaxLength(255)]
        public required string Domain { get; set; }

        public required List<string> ProductDevEmails { get; set; }
		public required List<string> BusinessOwnerEmails { get; set; }
	}
}
