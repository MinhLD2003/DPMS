using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DPMS_WebAPI.Repositories
{
    public class RiskRepository : BaseRepository<Risk>, IRiskRepository
    {
        public RiskRepository(DPMSContext context) : base(context)
        {
        }

        public async Task<List<Risk>> GetExportData()
        {
            return await _context.Risks.Include(r => r.CreatedBy).ToListAsync();
        }
    }
}
