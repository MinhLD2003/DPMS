using System.Text.Json.Serialization;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.ViewModels.Risk
{
    public class ExportRiskVM
    {
        public Guid Id { get; set; }
        public string? RiskName { get; set; }
        public string? Mitigation { get; set; }
        public string Category { get; set; }
        public string? RiskContingency { get; set; }
        public string Strategy { get; set; }
        public int RiskImpact { get; set; }
        public int RiskProbability { get; set; }
        public int Priority { get; set; }
        public int RiskImpactAfterMitigation { get; set; }
        public int RiskProbabilityAfterMitigation { get; set; }
        public int PriorityAfterMitigation { get; set; }
        public string ? RiskOwner { get; set; }
        public DateTime ? RaisedAt { get; set; }
        public string CreatedBy { get; set; }
        
    }
}
