namespace DPMS_WebAPI.Events.EventModels
{
    public class DPIACreatedNotification : BaseNotification<DPIACreatedEvent>
    {
        public DPIACreatedNotification(DPIACreatedEvent data) : base(data)
        {
        }
    }

    public class DPIACreatedEvent
    {
        public Guid? ExternalSystemId { get; set; }
        // add more data if needed
    }
}
