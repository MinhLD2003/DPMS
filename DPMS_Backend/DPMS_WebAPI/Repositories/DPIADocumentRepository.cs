using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Repositories
{
    public class DPIADocumentRepository : BaseRepository<DPIADocument>, IDPIADocumentRepository
    {
        public DPIADocumentRepository(DPMSContext context) : base(context)
        {

        }
    }
}