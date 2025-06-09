using AutoMapper;
using DPMS.EmailEngine.EmailTemplates;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.ViewModels.Email;
using MailKit.Security;
using MimeKit;
using Quiz.EmailEngine;
using System.Net;
using System.Net.Mail;

namespace DPMS_WebAPI.Services
{

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IRazorViewToStringRenderer _renderer;
        private readonly IMapper _mapper;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, IRazorViewToStringRenderer renderer, IMapper mapper, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _renderer = renderer;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task SendAsync(string from, string to, string senderName, string receiverName, string subject, string body)
        {
            _logger.LogInformation("Sending email to {to}, with subject {subject}", to, subject);

            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(senderName, from));
            message.To.Add(new MailboxAddress(receiverName, to));
            message.Subject = subject;

            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = body
            };

            try
            {
                using var client = new MailKit.Net.Smtp.SmtpClient();

                // Validate SMTP settings before usage
                string smtpHost = _configuration["Smtp:Host"];
                string smtpPort = _configuration["Smtp:Port"];
                string smtpUser = _configuration["Smtp:Username"];
                string smtpPass = _configuration["Smtp:Password"];

                if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpPort) ||
                    string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPass))
                {
                    _logger.LogError("SMTP configuration is missing or invalid.");
                    throw new InvalidOperationException("SMTP configuration is missing or invalid.");
                }

                int port = int.Parse(smtpPort);

                await client.ConnectAsync(smtpHost, port, SecureSocketOptions.StartTls);

                // Authenticate only if required
                if (!string.IsNullOrWhiteSpace(smtpUser) && !string.IsNullOrWhiteSpace(smtpPass))
                {
                    await client.AuthenticateAsync(smtpUser, smtpPass);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {to}", to);
                throw;
            }
        }

        public async Task SendAccountCredentials(AccountCredentials credentials)
        {
            AccountCredentialsVM model = _mapper.Map<AccountCredentialsVM>(credentials);

            string from = _configuration["Smtp:FromEmail"];
            string to = credentials.Email;
            string senderName = _configuration["Smtp:Username"];
            string receiverName = credentials.FullName;
            const string subject = "Account credentials";

            string htmlEmail = await _renderer.RenderViewToStringAsync<AccountCredentialsVM>("/EmailTemplates/AccountCredentials.cshtml", model);

            await SendAsync(from, to, senderName, receiverName, subject, htmlEmail);
        }

        public async Task<bool> SendResetPasswordEmailAsync(string email, string token, string baseUrl)
        {
            try
            {
                string resetLink = $"{baseUrl}/verify?token={token}";
                string subject = "Reset Your Password";
                string body = $@"
                <p>Hello,</p>
                <p>You requested a password reset. Click the link below to reset your password:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>If you did not request this, please ignore this email.</p>
                <p>Thanks,<br/>Your Company</p>";

                string smtpHost = _configuration["Smtp:Host"];
                int smtpPort = int.Parse(_configuration["Smtp:Port"]);
                bool enableSsl = bool.Parse(_configuration["Smtp:EnableSsl"]);
                string smtpUser = _configuration["Smtp:Username"];
                string smtpPass = _configuration["Smtp:Password"];
                string fromEmail = _configuration["Smtp:FromEmail"];

                using (System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = enableSsl;
                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);

                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true,
                    };

                    mailMessage.To.Add(email);
                    await client.SendMailAsync(mailMessage);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }
    }

}
