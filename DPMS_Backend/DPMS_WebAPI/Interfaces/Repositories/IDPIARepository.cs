using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Interfaces.Repositories
{
    public interface IDPIARepository : IRepository<DPIA>
    {
        //Task<DPIA?> GetDPIAByIdAsync(Guid id);
        //Task<DPIA?> CreateDPIAAsync(DPIA dpia);
        //Task<DPIA?> UpdateDPIAAsync(DPIA dpia);
        //Task<bool> DeleteDPIAAsync(Guid id);
        //Task<IEnumerable<DPIA>> GetAllDPIAsAsync();
        Task<IEnumerable<DPIAMember>>GetDPIAMembersAsync(Guid id);
        Task<IEnumerable<DPIAResponsibility>> GetDPIAResponsibilitiesAsync(Guid id);
        Task<IEnumerable<Comment>> GetCommentsAsync(Guid id);
        Task SaveCommentAsync(Comment comment);
        Task SaveEventsAsync(DPIAEvent dpiaEvent);
        Task<List<DPIAEvent>> GetEventsAsync(Guid id);
        Task AddMemberAsync(DPIAMember member);
        Task BulkAddMembersAsync(List<DPIAMember> newMembers);
        Task BulkAddResponsibilitiesAsync(List<DPIAResponsibility> newResponsibilities);
        Task UploadDocumentAsync(Guid dpiaId, DPIADocument document);
        Task<DPIA?> GetDPIADetailAsync(Guid dpiaId);
    }
}