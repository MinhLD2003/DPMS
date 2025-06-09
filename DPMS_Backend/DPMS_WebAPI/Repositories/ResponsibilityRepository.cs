using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Repositories
{
    public class ResponsibilityRepository : BaseRepository<Responsibility> , IResponsibilityRepository
    {
        private readonly DPMSContext _context;

        public ResponsibilityRepository(DPMSContext context) : base(context)
        {

        }
    }
}
