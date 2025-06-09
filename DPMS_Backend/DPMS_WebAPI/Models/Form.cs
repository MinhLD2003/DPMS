using System.ComponentModel.DataAnnotations;

namespace DPMS_WebAPI.Models
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public class Form : BaseModel
    {
        // public Guid? SystemId { get; set; } 
        [MaxLength(255)]
        public string? Name { get; set; }
        public int Version { get; set; }
        public FormTypes FormType { get; set; }
        public FormStatus Status { get; set; }
        // public ExternalSystem? ExternalSystem { get; set; }
        public ICollection<FormElement>? FormElements { get; set; } = new List<FormElement>();
    }

    public enum FormStatus
    {
        Draft = 1,
        Activated = 2,
        Deactivated = 3
    }

    public enum FormTypes
    {
        FIC = 1,
    }
}
