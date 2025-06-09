using System.ComponentModel.DataAnnotations;

namespace DPMS_WebAPI.Models
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public class DPIADocument : BaseDocument
    {
        public Guid DPIAId { get; set; }
        public Guid? ResponsibleId { get; set; }
        public DPIA? DPIA { get; set; }
    }
}
