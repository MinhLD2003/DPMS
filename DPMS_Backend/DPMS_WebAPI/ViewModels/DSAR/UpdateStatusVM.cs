using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels.DSAR
{
    public class UpdateStatusVM
    {
        public Guid Id { get; set; }
        public DSARStatus Status { get; set; }
    }
}
