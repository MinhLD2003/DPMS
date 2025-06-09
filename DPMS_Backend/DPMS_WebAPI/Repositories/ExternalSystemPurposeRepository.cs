using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Repositories
{
    public class ExternalSystemPurposeRepository : BaseRepository<ExternalSystemPurpose>, IExternalSystemPurposeRepository
    {
        private readonly DPMSContext _context;
        public ExternalSystemPurposeRepository(DPMSContext context) : base(context)
        {

        }
    }
}
