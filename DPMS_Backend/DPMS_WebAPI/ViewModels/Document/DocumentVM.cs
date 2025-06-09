using DPMS_WebAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace DPMS_WebAPI.ViewModels.Document
{
    public class DocumentVM
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DocumentType DocumentType { get; set; }
        public string FileUrl { get; set; }
        public Guid? RelatedId { get; set; }
        public string FileFormat { get; set; }

    }
}
