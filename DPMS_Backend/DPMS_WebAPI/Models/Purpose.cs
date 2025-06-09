using System.ComponentModel.DataAnnotations;

namespace DPMS_WebAPI.Models
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public class Purpose : BaseModel
    {
        [MaxLength(255)]
        public string? Name { get; set; }
        public string? Description { get; set; }

        [EnumDataType(typeof(PurposeStatus))]
        public PurposeStatus Status {  get; set; }

        // public string? DataTypes { get; set; }
        // public string? ProcessingActivities { get; set; }

        public ICollection<ExternalSystemPurpose>? ExternalSystems { get; set; }
        public ICollection<ConsentPurpose>? Purposes { get; set; }
    }
    public enum PurposeStatus
    {
        Draft, // Cho phe duyet thi moi duoc sang active 
        Active, // Active -> Dang hoat dong
        Inactive, // Ngung hoat dong nhung => cac consent giu nguyen, cac external system se khong 
    }
}
