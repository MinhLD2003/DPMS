using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Interfaces.Services
{
    public interface IIssueTicketDocumentService : IService<IssueTicketDocument>
    {
        Task<bool> DeleteIssueTicketFilesOnS3(List<IssueTicketDocument> documents);
    }
}
