using DPMS.EmailEngine.EmailTemplates;
using DPMS_WebAPI.Events.EventModels;
using DPMS_WebAPI.ViewModels.Email;

namespace DPMS_WebAPI.Interfaces.Services
{
    public interface IEmailTemplateService
    {
        Task<bool> SendResetPasswordEmailAsync(string email, string token);
        Task<bool> SendAccountCredentialsAsync(AccountCredentials credentials);
        Task<bool> SendUserAddedToSystemNotificationAsync(UserAddedToSystemVM model);
        Task<bool> SendDPIAStartNotificationAsync(DPIAStartedNotification notification);
        Task<bool> SendUserAddedToDPIANotificationAsync(UserAddedToDPIANotification notification);
        Task<bool> SendDPIAReviewRequestNotificationAsync(DPIAReviewRequestNotification notification);
        Task<bool> SendDPIAApprovalNotification(DPIAApprovalNotification notification);
        Task<bool> SendResponsibilityCompletedNotificationAsync(ResponsibilityCompletedNotification notification);
    }
}
