using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Interfaces.Repositories
{
    public interface IDsarRepository : IRepository<DSAR>
    {
        Task<IEnumerable<DSAR>> GetOverDue();
    }
}
