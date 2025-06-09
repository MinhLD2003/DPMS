using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels.Form
{
	public class FormVM
	{
		public Guid Id { get; set; }
		// public Guid? SystemId { get; set; } // NULL means global template

		[MaxLength(255)]
		public string Name { get; set; }
		public int Version { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public FormStatus Status {get; set; }
		public FormTypes FormType { get; set; }
		public ICollection<FormElementVM>? FormElements { get; set; }
	}
}
