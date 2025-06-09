namespace DPMS_WebAPI.Models
{
    public class Risk : BaseModel
    {
        public string? RiskName { get; set; }
        public string? Mitigation { get; set; }
        public RiskCategory Category { get; set; }
        public string? RiskContingency { get; set; }
        public ResponseStrategy Strategy { get; set; }
        public int RiskImpact { get; set; }
        public int RiskProbability { get; set; }
        public int Priority { get; set; }
        public int RiskImpactAfterMitigation { get; set; }
        public int RiskProbabilityAfterMitigation { get; set; }
        public int PriorityAfterMitigation { get; set; }
        public string? RiskOwner { get; set; }
        public DateTime RaisedAt { get; set; }
    }

    public enum RiskCategory
    {
        Technical = 0,
        Organizational = 1,
        Scope = 2,
        Schedule = 3,
        Usability = 4,
        Communication = 5,
        Quality = 6,
    }

    public enum ResponseStrategy
    {
        Mitigate = 0,
        Prevent = 1,
        Transfer = 2,
        Acceptance = 3,
        Exploitation = 4,
    }
}
