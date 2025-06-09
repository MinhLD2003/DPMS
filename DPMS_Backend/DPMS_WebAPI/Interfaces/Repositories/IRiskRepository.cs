using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Interfaces.Repositories
{
    public interface IRiskRepository : IRepository<Risk>
    {
        Task<List<Risk>> GetExportData();
    }
}
