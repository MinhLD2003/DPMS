using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Repositories
{
    public class PurposeRepository : BaseRepository<Purpose> ,IPurposeRepository
    {
        private readonly DPMSContext _context;

        public PurposeRepository(DPMSContext context) : base(context)
        {

        }
    }
}
