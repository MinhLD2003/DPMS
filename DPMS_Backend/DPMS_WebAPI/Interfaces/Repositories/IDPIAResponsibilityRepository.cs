using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Interfaces.Repositories
{
    public interface IDPIAResponsibilityRepository: IRepository<DPIAResponsibility>
    {
        public Task UploadDocumentAsync(Guid id, Guid responsibilityId, DPIADocument document);
        public Task<IEnumerable<MemberResponsibility>> GetMembersAsync(Guid id);
    }
}
