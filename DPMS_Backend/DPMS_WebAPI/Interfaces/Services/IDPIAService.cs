using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.Comment;
using DPMS_WebAPI.ViewModels.DPIA;
using System.Security.Claims;

namespace DPMS_WebAPI.Interfaces.Services
{
    
    public interface IDPIAService : IService<DPIA>
    {
        Task<DPIADetailVM> GetDPIAByIdAsync(Guid id);
        Task<List<Comment>> GetCommentsAsync(Guid id, ClaimsPrincipal principal);
        Task<List<EventDetailVM>> GetHistoryAsync(Guid id, ClaimsPrincipal principal);
        Task SaveEventsAsync(Guid id, DPIAEvent dpiaEvent);
        Task<List<DPIAMemberVM>> GetMembersAsync(Guid id, ClaimsPrincipal principal);
        Task AddCommentAsync(AddCommentVM comment, ClaimsPrincipal principal);
        Task AddMembersAsync(Guid id, List<DPIAMemberCreateVM> members);
        Task UpdateMembersAsync(Guid id, List<DPIAMemberCreateVM> members);
        /// <summary>
        /// Overload of AddAsync
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="principal"></param>
        /// <returns></returns>
        Task<DPIA> AddAsync(DPIACreateVM entity, ClaimsPrincipal principal); // Tested
        Task DeleteAsync(Guid id, ClaimsPrincipal principal);
        Task UpdateMemberResponsibilitiesAsync(Guid id, List<DPIAResponsibilityVM> responsibilityVMs);
        Task<DPIAResponsibilityDetailVM> GetResponsibilityAsync(Guid dpiaId, Guid responsibilityId, ClaimsPrincipal principal); // Tested
        Task StartDPIAAsync(Guid id, ClaimsPrincipal principal); // Tested
        Task RequestApprovalAsync(Guid id, ClaimsPrincipal principal);
        Task ApproveAsync(Guid id, ClaimsPrincipal principal);
        Task RejectAsync(Guid id, ClaimsPrincipal principal);
        Task RestartAsync(Guid id, ClaimsPrincipal principal);
        Task UpdateResponsibilityMembersAsync(Guid id, Guid responsibilityId, List<Guid> userIds, Guid? pic, ClaimsPrincipal principal);
        Task UploadDocumentAsync(Guid dpiaId, Guid responsibilityId, IFormFile file, ClaimsPrincipal principal);
        Task UpdateResponsibilityStatusAsync(Guid id, DPIAResponsibilityUpdateStatusVM status, ClaimsPrincipal principal);
        Task UpdateMemberResponsibilityStatusAsync(Guid id, MemberTaskStatus status, ClaimsPrincipal principal); // Tested
        Task UpdateCommentAsync(Guid id, AddCommentVM comment, ClaimsPrincipal principal); 
        Task UploadDocumentAsync(Guid id, IFormFile file, ClaimsPrincipal user);
        Task<DPIADetailVM> GetDPIADetailByMemberId(Guid id, Guid DPIAMemberId);
        Task<IEnumerable<UserVM>> GetUsersForDPIA();
        Task DeleteResponsibilityAsync(Guid dpiaId, Guid responsibilityId, ClaimsPrincipal principal);
        Task DeleteDocumentAsync(Guid id, Guid documentId, ClaimsPrincipal principal);
        Task RestartResponsibilityAsync(Guid id, Guid responsibilityId, ClaimsPrincipal principal); // Tested
        Task<PagedResponse<DPIAListVM>> GetDPIAs(QueryParams queryParams, ClaimsPrincipal principal);
    }
}