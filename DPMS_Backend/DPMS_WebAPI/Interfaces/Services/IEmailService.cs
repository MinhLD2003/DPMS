using DPMS_WebAPI.ViewModels.Email;

namespace DPMS_WebAPI.Interfaces.Services
{
    public interface IEmailService
    {
        Task<bool> SendResetPasswordEmailAsync(string email, string token, string baseUrl);
        Task SendAccountCredentials(AccountCredentials credentials);
        Task SendAsync(string from, string to, string senderName, string receiverName, string subject, string body);
    }
}
