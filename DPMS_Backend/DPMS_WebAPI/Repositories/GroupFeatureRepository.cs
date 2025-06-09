using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DPMS_WebAPI.Repositories
{
    public class GroupFeatureRepository : BaseRepository<GroupFeature>, IGroupFeatureRepository
    {
        public GroupFeatureRepository(DPMSContext context) : base(context)
        {
        }

        public async Task<int> BulkAddAsync(IEnumerable<GroupFeature> entities)
        {
            await _context.Set<GroupFeature>().AddRangeAsync(entities);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> BulkDeleteAsync(IEnumerable<GroupFeature> entities)
        {
            _context.Set<GroupFeature>().RemoveRange(entities);
            return await _context.SaveChangesAsync();
        }
    }
} 