using DPMS_WebAPI.Interfaces.Repositories;

namespace DPMS_WebAPI.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IIssueTicketRepository IssueTickets { get; }
        IFeatureRepository Features { get; }
        IGroupRepository Groups { get; }
        IGroupFeatureRepository GroupFeatures { get; }
        IIssueTicketDocumentRepository IssueTicketDocuments { get; }
        IDPIADocumentRepository DPIADocuments { get; }
        IUserRepository Users { get; }
        IExternalSystemRepository ExternalSystems { get; }
        IPurposeRepository Purposes { get; }
        IExternalSystemPurposeRepository ExternalSystemPurposes { get; }
        IFormRepository Forms { get; }
        IDPIARepository DPIAs {  get; }
        IDPIAMemberRepository DPIAMembers { get; }
        IMemberResponsibilityRepository MemberResponsibilities { get; }
        IDPIAResponsibilityRepository DPIAResponsibilities { get; }
        IResponsibilityRepository Responsibilities { get; }
        IConsentRepository Consents { get; }
        IConsentPurposeRepository ConsentPurposes { get; }
        IConsentTokenRepository ConsentTokens { get; }
        IPrivacyPolicyRepository PrivacyPolicies { get; }
        IDsarRepository Dsars { get; }
        IRiskRepository Risks { get; }
        ICommentRepository Comments { get; }
        Task<int> SaveChangesAsync();
    }
}
