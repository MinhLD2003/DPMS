using System.Text.Json.Serialization;
using DPMS_WebAPI.Enums;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels.Form
{
	public class FormElementVM
	{
		public Guid Id { get; set; }
		public Guid FormId { get; set; }
		public Guid? ParentId { get; set; } // Nullable for root sections
		public string Name { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public FormElementTypes? DataType { get; set; } // Null for sections
		public string? Value { get; set; }
		public int? OrderIndex { get; set; }
		public List<FormElementVM>? Children { get; set; } = new();
	}
}
