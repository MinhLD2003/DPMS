

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DPMS_WebAPI.Enums;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels.Document;

namespace DPMS_WebAPI.ViewModels.DPIA
{
    public class DPIADetailVM
    {
        private Models.DPIA dpia;

        // public DPIADetailVM(Models.DPIA dpia)
        // {
        //     this.dpia = dpia;
        // }

        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DPIAStatus Status { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DPIAType Type { get; set; }
        public DateTime DueDate { get; set; }
        public Guid ExternalSystemId { get; set; }
        public string ExternalSystemName { get; set; }
        public string CreatedBy { get; set; }
        public Guid? CreatedById { get; set; }
        public Guid? UpdatedById { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime LastModifiedAt { get; set; }
        public List<DPIADocumentVM> Documents { get; set; }
        public List<DPIAMemberVM> Members { get; set; }
        public List<DPIAResponsibilityListVM> Responsibilities { get; set; } = new List<DPIAResponsibilityListVM>();
    }

    public class DPIADocumentVM
    {
        public Guid Id { get; set; }
        public required string Title { get; set; } = string.Empty;
        public required string FileUrl { get; set; } = string.Empty;
        public required string? FileFormat { get; set; }
        public Guid? ResponsibilityId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DPIAMemberVM
    {
        public Guid Id { get; set; }
        public Guid UserId { get;set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        // public List<MemberResponsibilityVM> Responsibilities { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class MemberResponsibilityVM
    {
        public Guid Id { get; set; }
        public Guid MemberId { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsPic { get; set; } = false;
        public DateTime JoinedAt { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CompletionStatus CompletionStatus { get; set; } = CompletionStatus.NotStarted;
    }

    public class DPIAResponsibilityListVM
    {
        public Guid Id { get; set; }
        public Guid ResponsibilityId { get; set; }
        public string Title { get; set; }
        public DateTime DueDate { get; set; }
        public string Description { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ResponsibilityStatus Status { get; set; } = ResponsibilityStatus.NotStarted;
        public List<MemberResponsibilityVM> Members { get; set; } = new List<MemberResponsibilityVM>();
    }

    public class EventDetailVM
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DPIAEventType Type { get; set; }
        public UserVM CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
}