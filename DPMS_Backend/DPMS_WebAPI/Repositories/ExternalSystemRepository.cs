using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DPMS_WebAPI.Repositories
{
    public class ExternalSystemRepository : BaseRepository<ExternalSystem>, IExternalSystemRepository
    {
        //private readonly DPMSContext _context;

        public ExternalSystemRepository(DPMSContext context) : base(context)
        {
            //_context = context;
        }

        public async Task<Guid?> GetIdByName(string systemName)
        {
            return (await _context.ExternalSystems.FirstOrDefaultAsync(s => s.Name == systemName)).Id;
        }

        public async Task<IEnumerable<User>> GetUsersAsync(Guid systemId)
        {
            return await _context.Users
                .Include(u => u.Groups)
                .Where(u => u.Groups.Any(g => g.SystemId == systemId))
                .ToListAsync();
        }
    }
}
