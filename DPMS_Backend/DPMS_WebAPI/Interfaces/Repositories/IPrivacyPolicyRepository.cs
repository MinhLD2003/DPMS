using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Interfaces.Repositories
{
    public interface IPrivacyPolicyRepository : IRepository<PrivacyPolicy>
    {
        Task ActivePolicy (Guid id);
    }
}
