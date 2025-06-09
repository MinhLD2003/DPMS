using MediatR;

namespace DPMS_WebAPI.Events.EventModels
{
    public class DPIAApprovalNotification : INotification
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string DPIAName { get; set; }
        public bool IsApproved { get; set; }
        public string DPOName { get; set; }
        public string SystemName { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}