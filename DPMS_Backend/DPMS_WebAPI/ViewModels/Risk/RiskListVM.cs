using System.Text.Json.Serialization;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels.Risk
{
    public class RiskListVM
    {
        public Guid? Id {  get; set; } 
        public string? RiskName { get; set; }
        public string? Mitigation { get; set; }
        public RiskCategory Category { get; set; }
        public string? RiskContingency { get; set; }
        public ResponseStrategy Strategy { get; set; }
        public int RiskImpact { get; set; }
        public int RiskProbability { get; set; }
        public int Priority { get; set; }
        public int RiskImpactAfterMitigation { get; set; } = 0;
        public int RiskProbabilityAfterMitigation { get; set; } = 0;
        public int PriorityAfterMitigation { get; set; } = 0;
        public string ? RiskOwner { get; set; }
        public DateTime ? RaisedAt { get; set; }
    }
}
