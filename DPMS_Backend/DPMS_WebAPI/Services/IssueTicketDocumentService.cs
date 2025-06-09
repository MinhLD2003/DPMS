using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Services
{
    public class IssueTicketDocumentService : BaseService<IssueTicketDocument>, IIssueTicketDocumentService
    {
        // private readonly IUnitOfWork _unitOfWork;
        private readonly IFileRepository _fileRepository;
        public IssueTicketDocumentService(IUnitOfWork unitOfWork, IFileRepository fileRepository) : base(unitOfWork)
        {
            _fileRepository = fileRepository;
            // _unitOfWork = unitOfWork ??  throw new ArgumentNullException(nameof(unitOfWork));   
        }
        protected override IRepository<IssueTicketDocument> Repository => _unitOfWork.IssueTicketDocuments;

        public async Task<bool> DeleteIssueTicketFilesOnS3(List<IssueTicketDocument> documents)
        {
            try
            {
                foreach (var document in documents)
                {
                    await _fileRepository.DeleteFileAsync(document.FileUrl);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
