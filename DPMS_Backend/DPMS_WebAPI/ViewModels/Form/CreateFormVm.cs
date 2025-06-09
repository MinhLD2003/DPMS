using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DPMS_WebAPI.Enums;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels.Form
{
    public class CreateFormVm
    {
        // public Guid? SystemId { get; set; } // NULL means global template

        [MaxLength(255)]
        public string Name { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FormTypes FormType { get; set; }

        public ICollection<CreateFormElementsVm>? FormElements { get; set; }
    }

    public class CreateFormElementsVm
    {

        [MaxLength(500)]
        public string Name { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FormElementTypes? DataType { get; set; }
        public string? Value { get; set; }
		public int? OrderIndex { get; set; }

        public ICollection<CreateFormElementsVm>? Children { get; set; }
    }
}