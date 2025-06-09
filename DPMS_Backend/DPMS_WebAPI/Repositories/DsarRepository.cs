
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DPMS_WebAPI.Repositories
{
    public class DsarRepository :BaseRepository<DSAR>, IDsarRepository
    {
        private readonly DPMSContext _context;

        public DsarRepository(DPMSContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DSAR>> GetOverDue()
        {
            return await _context.DSARs
                .Include(d => d.ExternalSystem)
                .Where(d => d.Status == DSARStatus.Submitted &&
                                d.RequiredResponse >= DateTime.UtcNow.AddDays(-1)).ToListAsync();
        }
    }
}
