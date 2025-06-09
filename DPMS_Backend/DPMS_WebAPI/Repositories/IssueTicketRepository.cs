using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Repositories;

public class IssueTicketRepository : BaseRepository<IssueTicket>, IIssueTicketRepository
{
    private readonly DPMSContext _context;

    public IssueTicketRepository(DPMSContext context) : base(context)
    {
    }
}
