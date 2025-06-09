using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Interfaces.Services
{
    public interface IRiskService : IService<Risk>
    {
        Task<Stream> ExportAsync();
    }
}
