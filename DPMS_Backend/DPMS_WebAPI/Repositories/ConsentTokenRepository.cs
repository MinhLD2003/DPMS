using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Repositories
{
    public class ConsentTokenRepository : BaseRepository<ConsentToken>, IConsentTokenRepository
    {
        public ConsentTokenRepository(DPMSContext context) : base(context)
        {

        }
    }
}
