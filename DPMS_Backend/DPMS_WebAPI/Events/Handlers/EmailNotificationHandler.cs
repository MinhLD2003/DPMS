using AutoMapper;
using DPMS.EmailEngine.EmailTemplates;
using DPMS_WebAPI.Events.EventModels;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using MediatR;

namespace DPMS_WebAPI.Events.Handlers
{
    public class EmailNotificationHandler : INotificationHandler<UserAddedToSystemNotification>,
        INotificationHandler<UserRemovedFromSystemNotification>,
        INotificationHandler<DPIAStartedNotification>,
        INotificationHandler<UserAddedToDPIANotification>,
        INotificationHandler<DPIAReviewRequestNotification>,
        INotificationHandler<DPIAApprovalNotification>,
        INotificationHandler<ResponsibilityCompletedNotification>
    {
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogger<EmailNotificationHandler> _logger;
        private readonly IMapper _mapper;

        public EmailNotificationHandler(IEmailTemplateService emailTemplateService, ILogger<EmailNotificationHandler> logger, IMapper mapper)
        {
            _emailTemplateService = emailTemplateService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task Handle(UserAddedToSystemNotification notification, CancellationToken cancellationToken)
        {
            try
            {
                UserAddedToSystemVM model = _mapper.Map<UserAddedToSystemVM>(notification);
                bool sendSuccess = await _emailTemplateService.SendUserAddedToSystemNotificationAsync(model);

                if (sendSuccess)
                {
                    _logger.LogInformation("Email notification to {email}, notification on added to system {system} sent successfully",
                        notification.Email,
                        notification.SystemName);
                }
                else
                {
                    _logger.LogInformation("Email notification to {email}, notification on added to system {system} sent failed",
                        notification.Email,
                        notification.SystemName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send (user added to external system) notification email to {email}", notification.Email);
                // Consider adding retry logic or queuing for later delivery
            }
        }

        public Task Handle(UserRemovedFromSystemNotification notification, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task Handle(DPIAStartedNotification notification, CancellationToken cancellationToken)
        {
            bool sendSuccess = await _emailTemplateService.SendDPIAStartNotificationAsync(notification);

            if (sendSuccess)
            {
                _logger.LogInformation("Email notifcation system: SendDPIAStartNotificationAsync for {DPIAName} successfully", notification.Data.DPIAName);
            }
            else
            {
                _logger.LogError("Email notifcation system: SendDPIAStartNotificationAsync for {DPIAName} failed", notification.Data.DPIAName);
            }
        }

        public async Task Handle(UserAddedToDPIANotification notification, CancellationToken cancellationToken)
        {
            bool sendSuccess = await _emailTemplateService.SendUserAddedToDPIANotificationAsync(notification);

            if (sendSuccess)
            {
                _logger.LogInformation("Email notifcation system: SendUserAddedToDPIANotificationAsync for {DPIATitle} successfully", notification.DPIATitle);
            }
            else
            {
                _logger.LogError("Email notifcation system: SendUserAddedToDPIANotificationAsync for {DPIATitle} failed", notification.DPIATitle);
            }

        }

        public async Task Handle(DPIAReviewRequestNotification notification, CancellationToken cancellationToken)
        {
            bool sendSuccess = await _emailTemplateService.SendDPIAReviewRequestNotificationAsync(notification);
            if (sendSuccess)
            {
                _logger.LogInformation("Email notifcation system: SendDPIAReviewRequestNotificationAsync for {DPIAName} successfully", notification.DPIAName);
            }
            else
            {
                _logger.LogError("Email notifcation system: SendDPIAReviewRequestNotificationAsync for {DPIAName} failed", notification.DPIAName);
            }
        }

        public async Task Handle(DPIAApprovalNotification notification, CancellationToken cancellationToken)
        {
            bool sendSuccess = await _emailTemplateService.SendDPIAApprovalNotification(notification);
            if (sendSuccess)
            {
                _logger.LogInformation("Email notifcation system: SendDPIAApprovalNotification for {DPIAName} successfully", notification.DPIAName);
            }
            else
            {
                _logger.LogError("Email notifcation system: SendDPIAApprovalNotification for {DPIAName} failed", notification.DPIAName);
            }
        }

            public async Task Handle(ResponsibilityCompletedNotification notification, CancellationToken cancellationToken)
        {
            bool sendSuccess = await _emailTemplateService.SendResponsibilityCompletedNotificationAsync(notification);
            if (sendSuccess)
            {
                _logger.LogInformation("Email notifcation system: SendResponsibilityCompletedNotificationAsync for {ResponsibilityName} successfully", notification.ResponsibilityName);
            }
            else
            {
                _logger.LogError("Email notifcation system: SendResponsibilityCompletedNotificationAsync for {ResponsibilityName} failed", notification.ResponsibilityName);
            }
        }
    }
}
