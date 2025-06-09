using MediatR;

namespace DPMS_WebAPI.Events.EventModels
{
    public class UserAddedToSystemNotification : INotification
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; } // Bussiness Owner or Product Developer
        public string SystemName { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
