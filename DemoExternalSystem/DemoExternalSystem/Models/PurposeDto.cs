namespace DemoExternalSystem.Models
{
    public class PurposeDto
    {
        public string PurposeId { get; set; }
        public string PurposeName { get; set; }
        public string Status { get; set; }
    }

    public class ConsentStatus
    {
        public List<PurposeDto> Purposes { get; set; }
        public bool Status { get; set; }
    }
}
