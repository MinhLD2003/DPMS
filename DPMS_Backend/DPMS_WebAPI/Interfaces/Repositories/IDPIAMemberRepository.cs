using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Interfaces.Repositories
{
    public interface IDPIAMemberRepository : IRepository<DPIAMember>
    {
        Task UpdateAsync(List<DPIAMember> updatedMembers);
        /// <summary>
        /// TODO: Validate ID
        /// </summary>
        /// <param name="dpiaId"></param>
        /// <returns></returns>
        Task<List<string>> GetDpiaMemberEmail(Guid dpiaId);
    }
}
