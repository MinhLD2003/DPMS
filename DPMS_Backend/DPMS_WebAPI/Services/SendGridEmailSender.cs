using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models.NonDbModels;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace DPMS_WebAPI.Services
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly SendGridConfiguration _config;
        private readonly ILogger<SendGridEmailSender> _logger;

        public SendGridEmailSender(SendGridConfiguration config, ILogger<SendGridEmailSender> logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrEmpty(_config.ApiKey))
            {
                throw new ArgumentException("SendGrid API key cannot be null or empty", nameof(config));
            }

            _logger = logger;
        }

        public async Task<bool> SendAsync(EmailMessage message)
        {
            try
            {
                // Create SendGrid client with API key
                var client = new SendGridClient(_config.ApiKey);

                // Create email message
                var msg = new SendGridMessage();

                // Set sender
                string fromEmail = string.IsNullOrEmpty(message.From) ? _config.DefaultFromEmail : message.From;
                string fromName = string.IsNullOrEmpty(message.FromDisplayName) ? _config.DefaultFromName : message.FromDisplayName;
                msg.SetFrom(new EmailAddress(fromEmail, fromName));

                // Add recipients
                if (message.To == null || !message.To.Any())
                {
                    throw new ArgumentException("At least one recipient is required", nameof(message));
                }

                // Add To recipients
                var recipients = message.To.Select(r => new EmailAddress(r.Email, r.DisplayName)).ToList();
                msg.AddTos(recipients);

                // Add CC recipients if any
                if (message.Cc != null && message.Cc.Any())
                {
                    var ccRecipients = message.Cc.Select(r => new EmailAddress(r.Email, r.DisplayName)).ToList();
                    msg.AddCcs(ccRecipients);
                }

                // Add BCC recipients if any
                if (message.Bcc != null && message.Bcc.Any())
                {
                    var bccRecipients = message.Bcc.Select(r => new EmailAddress(r.Email, r.DisplayName)).ToList();
                    msg.AddBccs(bccRecipients);
                }

                // Set subject
                msg.SetSubject(message.Subject);

                // Set content (HTML or plain text)
                if (message.IsHtml)
                {
                    msg.AddContent(MimeType.Html, message.Body);
                }
                else
                {
                    msg.AddContent(MimeType.Text, message.Body);
                }

                // Add attachments if any
                if (message.Attachments != null && message.Attachments.Any())
                {
                    foreach (var attachment in message.Attachments)
                    {
                        msg.AddAttachment(
                            filename: attachment.FileName,
                            base64Content: Convert.ToBase64String(attachment.Content),
                            type: attachment.ContentType);
                    }
                }

                // Send the email
                _logger.LogInformation("Sending email with SendGrid");
                var response = await client.SendEmailAsync(msg);

                // Check if the email was sent successfully
                return response.StatusCode == System.Net.HttpStatusCode.Accepted ||
                       response.StatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via SendGrid");
                return false;
            }
        }
    }
}
