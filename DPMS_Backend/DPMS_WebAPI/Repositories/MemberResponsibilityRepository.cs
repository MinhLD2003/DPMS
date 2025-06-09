using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Repositories
{
    public class MemberResponsibilityRepository : BaseRepository<MemberResponsibility>, IMemberResponsibilityRepository
    {
        public MemberResponsibilityRepository(DPMSContext context) : base(context)
        {
        }
    }
}
