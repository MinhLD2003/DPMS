using MediatR;

namespace DPMS_WebAPI.Events.EventModels
{
    public class ResponsibilityCompletedNotification : INotification
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string DPIAName { get; set; }
        public string SystemName { get; set; }
        public string ResponsibilityName { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}