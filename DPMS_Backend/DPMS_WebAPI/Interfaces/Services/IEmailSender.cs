using DPMS_WebAPI.Models.NonDbModels;

namespace DPMS_WebAPI.Interfaces.Services
{
    public interface IEmailSender
    {
        Task<bool> SendAsync(EmailMessage message);
    }
}
