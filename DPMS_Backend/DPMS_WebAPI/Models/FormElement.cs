using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DPMS_WebAPI.Enums;

namespace DPMS_WebAPI.Models
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public class FormElement : BaseModel
    {
        [ForeignKey(nameof(Form))]
        public Guid FormId { get; set; }
        public Guid? ParentId { get; set; } // Nullable for root sections
        public string? Name { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FormElementTypes? DataType { get; set; } // Null for sections
        public int? OrderIndex { get; set; } = 0;
        //[JsonIgnore]
        public Form? Form { get; set; }
        //[JsonIgnore]
        public FormElement? Parent { get; set; } // Self-reference
        public List<FormElement> Children { get; set; } = new List<FormElement>();
    }
}
