namespace DPMS_WebAPI.Events.EventModels
{
    public class DPIAStartedNotification : BaseNotification<DPIAStartedEvent>
    {
        public DPIAStartedNotification(DPIAStartedEvent data) : base(data) { }
    }

    public class DPIAStartedEvent
    {
        /// <summary>
        /// DPIA members email
        /// </summary>
        public List<string> Emails { get; set; }
        public string QAManagerEmail { get; set; }
        public string QAManagerName { get; set; }
        public string DPIAName { get; set; }
        public DateTime DPIAStartTime { get; set; }
    }
}
