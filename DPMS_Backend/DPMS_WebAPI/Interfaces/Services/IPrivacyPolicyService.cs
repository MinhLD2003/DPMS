using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Interfaces.Services
{
    public interface IPrivacyPolicyService : IService<PrivacyPolicy>
    {
        Task ActivePolicy (Guid id);
    }
}
