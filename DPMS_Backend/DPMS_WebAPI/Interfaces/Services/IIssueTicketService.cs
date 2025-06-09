using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Interfaces.Services
{
    public interface IIssueTicketService : IService<IssueTicket>
    {
        public Task<IssueTicket> CreateIssueTicket(IssueTicket issueTicket, List<IFormFile> files);
        public Task<List<IssueTicketDocument>> UpdateIssueTicketFilesOnS3(Guid id, List<IFormFile> files, List<string> removedFiles);
    }
}
