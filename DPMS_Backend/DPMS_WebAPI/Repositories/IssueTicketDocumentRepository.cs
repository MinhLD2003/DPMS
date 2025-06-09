using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Repositories
{
    public class IssueTicketDocumentRepository : BaseRepository<IssueTicketDocument>, IIssueTicketDocumentRepository
    {
        private readonly DPMSContext _context;
        public IssueTicketDocumentRepository(DPMSContext context) : base(context)
        {
        }
    }
}
