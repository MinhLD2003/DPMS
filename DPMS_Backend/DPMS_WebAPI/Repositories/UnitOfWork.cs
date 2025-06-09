using DPMS_WebAPI.FileStorage;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DPMS_WebAPI.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DPMSContext _context;
        private IIssueTicketRepository _issueTicketRepository;
        private IUserRepository _userRepository;
        private IFeatureRepository _featureRepository;
        private IGroupRepository _groupRepository;
        private IGroupFeatureRepository _groupFeatureRepository;
        private IIssueTicketDocumentRepository _issueTicketDocumentRepository;
        private IDPIADocumentRepository _dpiaDocumentRepository;
        private IExternalSystemRepository _externalSystemRepository;
        private IPurposeRepository _purposeRepository;
        private IFormRepository _formRepository;
        private IExternalSystemPurposeRepository _externalSystemPurposeRepository;
        private IDPIAMemberRepository _DPIAMemberRepository;
        private IMemberResponsibilityRepository _memberResponsibilityRepository;
        private IDPIARepository _dpiaRepository;
        private IDPIAResponsibilityRepository _dpiaResponsibilityRepository;
        private IResponsibilityRepository _responsibilityRepository;
        private IConsentRepository _consentRepository;
        private IConsentPurposeRepository _consentPurposeRepository;
        private IConsentTokenRepository _consentTokenRepository;
        private IPrivacyPolicyRepository _privacyPolicyRepository;
        private IDsarRepository _dsarRepository;
        private IRiskRepository _riskRepository;
        private IFileRepository _fileRepository;
        private ICommentRepository _commentRepository;

        // Add other repositories here
        private bool _disposed = false;

        public UnitOfWork(DPMSContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IIssueTicketRepository IssueTickets => _issueTicketRepository ??= new IssueTicketRepository(_context);
        public IIssueTicketDocumentRepository IssueTicketDocuments => _issueTicketDocumentRepository ??= new IssueTicketDocumentRepository(_context);
        public IGroupRepository Groups => _groupRepository ??= new GroupRepository(_context);
        public IFeatureRepository Features => _featureRepository ??= new FeatureRepository(_context);
        public IGroupFeatureRepository GroupFeatures => _groupFeatureRepository ??= new GroupFeatureRepository(_context);
        public IUserRepository Users => _userRepository ??= new UserRepository(_context);
        public IExternalSystemRepository ExternalSystems => _externalSystemRepository ??= new ExternalSystemRepository(_context);
        public IPurposeRepository Purposes => _purposeRepository ??= new PurposeRepository(_context);
        public IFormRepository Forms => _formRepository ??= new FormRepository(_context);
        public IExternalSystemPurposeRepository ExternalSystemPurposes => _externalSystemPurposeRepository ??= new ExternalSystemPurposeRepository(_context);
        public IDPIAMemberRepository DPIAMembers => _DPIAMemberRepository ??= new DPIAMemberRepository(_context);
        public IMemberResponsibilityRepository MemberResponsibilities => _memberResponsibilityRepository ??= new MemberResponsibilityRepository(_context);
        public IDPIARepository DPIAs => _dpiaRepository ??= new DPIARepository(_context);
        public IConsentRepository Consents => _consentRepository ??= new ConsentRepository(_context);
        public IConsentPurposeRepository ConsentPurposes => _consentPurposeRepository ??= new ConsentPurposeRepository(_context);
        public IResponsibilityRepository Responsibilities => _responsibilityRepository ??= new ResponsibilityRepository(_context);
        public IPrivacyPolicyRepository PrivacyPolicies => _privacyPolicyRepository ??= new PrivacyPolicyRepository(_context);
        public IConsentTokenRepository ConsentTokens => _consentTokenRepository ??= new ConsentTokenRepository(_context);
        public IDsarRepository Dsars => _dsarRepository ??= new DsarRepository(_context);
        public IDPIAResponsibilityRepository DPIAResponsibilities => _dpiaResponsibilityRepository ??= new DPIAResponsibilityRepository(_context);
        public IRiskRepository Risks => _riskRepository ??= new RiskRepository(_context);
        public ICommentRepository Comments => _commentRepository ??= new CommentRepository(_context);
        public IDPIADocumentRepository DPIADocuments => _dpiaDocumentRepository ??= new DPIADocumentRepository(_context);
        // Implement other repository properties similarly

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
