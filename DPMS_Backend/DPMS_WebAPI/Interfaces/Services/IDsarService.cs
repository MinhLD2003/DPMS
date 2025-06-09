using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels.DSAR;
using FluentResults;

namespace DPMS_WebAPI.Interfaces.Services
{
    public interface IDsarService : IService<DSAR>
    {
        //Task NotifyDsar();
        Task ChangeStatus();
        Task BulkUpdatetStatus(List<UpdateStatusVM> vms);
        Task<Stream> DownloadImportTemplate(User loggedInUser);
        Task<Result> DoImportDsarAsync(Stream data);
    }
}
