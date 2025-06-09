namespace DPMS_WebAPI.Events.EventModels
{
    public class FICSubmittedNotification : BaseNotification<FICSubmittedEvent>
    {
        public FICSubmittedNotification(FICSubmittedEvent data) : base(data)
        {
        }
    }

    public class FICSubmittedEvent
    {
        public Guid SystemId { get; set; }
        public Guid SubmissionId { get; set; }
    }
}
