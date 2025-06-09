using System.ComponentModel.DataAnnotations;
namespace DPMS_WebAPI.Models
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public class Document : BaseModel
    {
        [StringLength(255)]
        public required string Title { get; set; }
        [EnumDataType(typeof(DocumentType))]
        public DocumentType DocumentType { get; set; }
        public required string FileUrl { get; set; }
        public Guid? RelatedId { get; set; }
        public Guid? IssueTicketId { get; set; }
        public Guid? DPIAId { get; set; }   
        [StringLength(50)]
        public required string FileFormat { get; set; }
    }

    public enum DocumentType
    {
        DPIA,
        IssueTicket,
        General
    }

}
