using DPMS_WebAPI.Enums;
using DPMS_WebAPI.Events.EventModels;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using MediatR;

namespace DPMS_WebAPI.Events.Handlers
{
    public class ExternalSystemHandler : INotificationHandler<FICSubmittedNotification>, INotificationHandler<DPIACreatedNotification>
    {
        private IExternalSystemService _systemService;
        private readonly ILogger<ExternalSystemHandler> _logger;

        public ExternalSystemHandler(IExternalSystemService systemService, ILogger<ExternalSystemHandler> logger)
        {
            _systemService = systemService;
            _logger = logger;
        }

        /// <summary>
        /// When FIC is filled for an external system, system's status WaitingForFIC --> WaitingForDPIA
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task Handle(FICSubmittedNotification notification, CancellationToken cancellationToken)
        {
            ExternalSystem? system = await _systemService.GetByIdAsync(notification.Data.SystemId);
            if (system == null)
            {
                throw new Exception("System not found");
            }

            _logger.LogInformation("Notification system: FIC for system {systemName} has been submitted", system.Name);
            if (system.Status == ExternalSystemStatus.WaitingForFIC)
            {
                system.Status = ExternalSystemStatus.WaitingForDPIA;
            }
            else
            {
                _logger.LogWarning("Notification system: System {name} is not in WaitingForFIC status. Current system status will remain unchanged.", system.Name);
            }
            await _systemService.UpdateAsync(system);
        }

        /// <summary>
        /// When DPIA created for an external system, system's status WaitingForDPIA --> DPIA Created
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task Handle(DPIACreatedNotification notification, CancellationToken cancellationToken)
        {
            ExternalSystem? system = await _systemService.GetByIdAsync(notification.Data.ExternalSystemId);

            // if system exists (usually because of this is DPIA Type 01)
            if (system != null)
            {


                if (system.Status == ExternalSystemStatus.WaitingForDPIA)
                {
                    system.Status = ExternalSystemStatus.DPIACreated;
                    await _systemService.UpdateAsync(system);
                    _logger.LogInformation("Notification system: DPIA has been created for system {system}, system's status move {s1} --> {s2}", system.Name, ExternalSystemStatus.WaitingForDPIA.ToString(), ExternalSystemStatus.DPIACreated.ToString());
                }
                else
                {
                    _logger.LogWarning("Notification system: DPIA has been created for system {system}. However system is not in {s1}, status would remain unchanged", system.Name, ExternalSystemStatus.WaitingForDPIA.ToString());
                }
            }
        }
    }
}
