using System.ComponentModel.DataAnnotations;

namespace DPMS_WebAPI.ViewModels.Risk
{
    public class RiskResolveVM
    {
        [Range(1, 16, ErrorMessage = "Risk impact after mitigation must be between 1 and 16.")]
        public int? RiskImpactAfterMitigation { get; set; }
        [Range(1, 5, ErrorMessage = "Risk probability after mitigation must be between 1 and 5.")]
        public int? RiskProbabilityAfterMitigation { get; set; }
        [Range(1, 80, ErrorMessage = "Priority after mitigation must be between 1 and 80.")]
        public int? PriorityAfterMitigation { get; set; }
    }
}
