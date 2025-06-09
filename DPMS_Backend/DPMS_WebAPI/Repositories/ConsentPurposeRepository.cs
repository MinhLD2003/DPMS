using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Repositories
{
    public class ConsentPurposeRepository : BaseRepository<ConsentPurpose>, IConsentPurposeRepository
    {
        public ConsentPurposeRepository(DPMSContext context) : base(context)
        {
        }
    }
}
