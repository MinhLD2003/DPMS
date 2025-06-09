using System.ComponentModel.DataAnnotations;

namespace DPMS_WebAPI.ViewModels.ExternalSystem
{
    public class SystemPurposeVM
    {
        public Guid ExternalSystemId { get; set; }

        public List<Guid> PurposeIds { get; set; } = new List<Guid>();
    }
}
