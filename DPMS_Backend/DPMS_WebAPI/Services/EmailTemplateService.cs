using AutoMapper;
using DPMS.EmailEngine.EmailTemplates;
using DPMS_WebAPI.Events.EventModels;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models.NonDbModels;
using DPMS_WebAPI.ViewModels.Email;
using Quiz.EmailEngine;

namespace DPMS_WebAPI.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IEmailSender _emailSender;
        private readonly IRazorViewToStringRenderer _renderer;
        private readonly EmailConfiguration _emailConfig;
        private readonly IMapper _mapper;

        public EmailTemplateService(
            IEmailSender emailSender,
            IRazorViewToStringRenderer renderer,
            EmailConfiguration emailConfig,
            IMapper mapper)
        {
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            _emailConfig = emailConfig ?? throw new ArgumentNullException(nameof(emailConfig));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<bool> SendAccountCredentialsAsync(AccountCredentials credentials)
        {
            // Map credentials to view model
            AccountCredentialsVM model = _mapper.Map<AccountCredentialsVM>(credentials);

            // Render the email template to HTML
            string htmlBody = await _renderer.RenderViewToStringAsync<AccountCredentialsVM>(
                "/EmailTemplates/AccountCredentials.cshtml",
                model);

            // Create the email message
            var message = new EmailMessage
            {
                From = _emailConfig.DefaultFromEmail,
                FromDisplayName = _emailConfig.DefaultFromName,
                Subject = "Your Account Credentials",
                Body = htmlBody,
                IsHtml = true
            };

            // Add the recipient
            message.AddRecipient(credentials.Email, credentials.FullName);

            // Send the email
            return await _emailSender.SendAsync(message);
        }

        public Task<bool> SendDPIAApprovalNotification(DPIAApprovalNotification notification)
        {
            // Map notification to view model
            DPIAApprovalVM model = _mapper.Map<DPIAApprovalVM>(notification);

            // Render the email template to HTML
            string htmlBody = _renderer.RenderViewToStringAsync<DPIAApprovalVM>(
                "/EmailTemplates/DPIAApproval.cshtml",
                model).Result;

            // Create the email message
            var message = new EmailMessage
            {
                From = _emailConfig.DefaultFromEmail,
                FromDisplayName = _emailConfig.DefaultFromName,
                Subject = $"DPIA {model.DPIAName} approval",
                Body = htmlBody,
                IsHtml = true
            };

            // Add the recipient
            message.AddRecipient(notification.Email, notification.FullName);

            // Send the email
            return _emailSender.SendAsync(message);
        }

        public async Task<bool> SendDPIAReviewRequestNotificationAsync(DPIAReviewRequestNotification notification)
        {
            // Map notification to view model
            DPIAReviewRequest model = _mapper.Map<DPIAReviewRequest>(notification);

            // Render the email template to HTML
            string htmlBody = await _renderer.RenderViewToStringAsync<DPIAReviewRequest>(
                "/EmailTemplates/DPIAReviewRequest.cshtml",
                model);

            // Create the email message
            var message = new EmailMessage
            {
                From = _emailConfig.DefaultFromEmail,
                FromDisplayName = _emailConfig.DefaultFromName,
                Subject = $"DPIA {model.DPIAName} review request",
                Body = htmlBody,
                IsHtml = true
            };

            // Add the recipient
            message.AddRecipient(notification.Email, notification.FullName);

            // Send the email
            return await _emailSender.SendAsync(message);
        }

        public async Task<bool> SendDPIAStartNotificationAsync(DPIAStartedNotification notification)
        {
            DPIAStartVM model = _mapper.Map<DPIAStartVM>(notification.Data);

            string htmlBody = await _renderer.RenderViewToStringAsync<DPIAStartVM>(
                "/EmailTemplates/DPIAStart.cshtml",
                model);

            // Create the email message
            var message = new EmailMessage
            {
                From = _emailConfig.DefaultFromEmail,
                FromDisplayName = _emailConfig.DefaultFromName,
                Subject = $"DPIA {model.DPIAName} started",
                Body = htmlBody,
                IsHtml = true
            };

            // Add the recipient
            foreach (var email in notification.Data.Emails)
            {
                message.AddRecipient(email);
            }

            // Send the email
            return await _emailSender.SendAsync(message);
        }

        public Task<bool> SendResetPasswordEmailAsync(string email, string token)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SendUserAddedToDPIANotificationAsync(UserAddedToDPIANotification notification)
        {
            // Map notification to view model
            UserAddedToDPIAVM model = _mapper.Map<UserAddedToDPIAVM>(notification);

            // Render the email template to HTML
            string htmlBody = await _renderer.RenderViewToStringAsync<UserAddedToDPIAVM>(
                "/EmailTemplates/UserAddedToDPIA.cshtml",
                model);

            // Create the email message
            var message = new EmailMessage
            {
                From = _emailConfig.DefaultFromEmail,
                FromDisplayName = _emailConfig.DefaultFromName,
                Subject = $"You have been added to {model.DPIATitle}",
                Body = htmlBody,
                IsHtml = true
            };

            // Add the recipient
            message.AddRecipient(notification.Email, notification.FullName);

            // Send the email
            return await _emailSender.SendAsync(message);
        }

        public async Task<bool> SendUserAddedToSystemNotificationAsync(UserAddedToSystemVM model)
        {
            // Render the email template to HTML
            string htmlBody = await _renderer.RenderViewToStringAsync<UserAddedToSystemVM>(
                "/EmailTemplates/UserAddedToSystem.cshtml",
                model);

            // Create the email message
            var message = new EmailMessage
            {
                From = _emailConfig.DefaultFromEmail,
                FromDisplayName = _emailConfig.DefaultFromName,
                Subject = $"You have been added to {model.SystemName}",
                Body = htmlBody,
                IsHtml = true
            };

            // Add the recipient
            message.AddRecipient(model.Email, model.FullName);

            // Send the email
            return await _emailSender.SendAsync(message);
        }

        public async Task<bool> SendResponsibilityCompletedNotificationAsync(ResponsibilityCompletedNotification notification)
        {
            ResponsibilityCompletedVM model = _mapper.Map<ResponsibilityCompletedVM>(notification);

            string htmlBody = await _renderer.RenderViewToStringAsync<ResponsibilityCompletedVM>(
                "/EmailTemplates/ResponsibilityCompleted.cshtml",
                model); 
            
            var message = new EmailMessage
            {
                From = _emailConfig.DefaultFromEmail,
                FromDisplayName = _emailConfig.DefaultFromName,
                Subject = $"Responsibility {model.ResponsibilityName} completed",
                Body = htmlBody,
                IsHtml = true
            };

            // Add the recipient
            message.AddRecipient(notification.Email, notification.FullName);

            // Send the email
            return await _emailSender.SendAsync(message);
        }
    }
}
