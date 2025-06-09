using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels.DSAR
{
    public class DsarImportVM
    {
        public string RequesterName { get; set; }
        public string RequesterEmail { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string TypeStr { get; set; }
        public string StatusStr { get; set; }
        public DSARType? Type { get; set; }
        public DSARStatus? Status { get; set; }
        public string ExternalSystemName { get; set; }
        public string RequiredResponseStr { get; set; }
        public string CompletedDateStr { get; set; }
        public DateTime? RequiredResponse { get; set; }
        public DateTime? CompletedDate { get; set; }

        public Guid? ExternalSystemId { get; set; }

        //public int Index { get; set; }
        public string Error { get; set; } = string.Empty;
    }
}
