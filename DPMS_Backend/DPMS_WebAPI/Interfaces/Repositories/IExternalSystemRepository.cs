
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Interfaces.Repositories
{
    public interface IExternalSystemRepository : IRepository<ExternalSystem>
    {
        public Task<IEnumerable<User>> GetUsersAsync(Guid systemId);
        public Task<Guid?> GetIdByName(string systemName);
    }
}