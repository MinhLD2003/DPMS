using DPMS_WebAPI.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace DPMS_WebAPI.ViewModels.Risk
{
    public class RiskVM
    {
        [BindNever]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Risk name is required.")]
        [StringLength(255, ErrorMessage = "Risk name cannot exceed 200 characters.")]
        public string RiskName { get; set; }

        [Required(ErrorMessage = "Risk Mitigation is required.")]
        public string? Mitigation { get; set; }

        [Required(ErrorMessage = "Risk category is required.")]
        public RiskCategory Category { get; set; }

        [Required(ErrorMessage = "Risk Contingency is required.")]
        public string? RiskContingency { get; set; }

        [Required(ErrorMessage = "Response strategy is required.")]
        public ResponseStrategy Strategy { get; set; }
        [Range(1, 16, ErrorMessage = "Risk impact must be between 1 and 16.")]
        public int RiskImpact { get; set; }

        [Range(1, 5, ErrorMessage = "Risk probability must be between 1 and 5.")]
        public int RiskProbability { get; set; }

        [Range(1, 80, ErrorMessage = "Priority must be between 1 and 80.")]
        public int Priority { get; set; }

        [Range(1, 16, ErrorMessage = "Risk impact after mitigation must be between 1 and 16.")]
        public int? RiskImpactAfterMitigation { get; set; }

        [Range(1, 5, ErrorMessage = "Risk probability after mitigation must be between 1 and 5.")]
        public int? RiskProbabilityAfterMitigation { get; set; }

        public int? PriorityAfterMitigation { get; set; }

        [Required(ErrorMessage = "Risk owner is required.")]
        [StringLength(200, ErrorMessage = "Risk owner name cannot exceed 200 characters.")]
        public string RiskOwner { get; set; }

        [DataType(DataType.Date)]
        public DateTime? RaisedAt { get; set; }
    }
}
