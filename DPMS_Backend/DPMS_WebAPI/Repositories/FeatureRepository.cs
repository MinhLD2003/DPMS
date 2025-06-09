using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DPMS_WebAPI.Repositories
{
    public class FeatureRepository : BaseRepository<Feature> ,IFeatureRepository
    {
        private readonly DPMSContext _context;

        public FeatureRepository(DPMSContext context) : base(context)
        {
            _context = context;
        }

        public Task<List<Feature>> GetFeaturesByGroupId(Guid groupId)
        {
            var list = _context.GroupFeatures
                .Where(gf => gf.GroupId == groupId)
                .Include(gf => gf.Feature)
                .Select(gf => gf.Feature)
                .ToListAsync();
            return list;
        }

        public async Task<List<Feature>> GetListNestedFeatures()
        {
            return await _context.Features
           .Where(f => f.ParentId == null) // Get only root features
           .Include(f => f.Children) // Include children
           .ToListAsync();
        }
    }
}
