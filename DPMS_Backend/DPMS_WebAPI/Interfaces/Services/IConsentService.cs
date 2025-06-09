using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.Consent;
using FluentResults;

namespace DPMS_WebAPI.Interfaces.Services
{
    public interface IConsentService : IService<Consent>
    {
        Task SubmitConsent(SubmitConsentVM vm);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="systemId"></param>
        /// <returns></returns>
        Task<Stream> ExportConsentLog(Guid? systemId);
        Task<Consent> GetConsentByEmail (string email);
        Task<PagedResponse<Consent>> GetConsentLogWithPurpose(QueryParams queryParams);
        Task<bool> ValidateConsentToken(string token);
        Task<string> CreateConsentToken (Guid systemId);
        Task UpdateToken (string token);
        Task<Guid> GetSystemFromToken(string token);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="systemId">External System ID that we want to download template</param>
        /// <returns></returns>
        Task<Stream> DownloadImportTemplateAsync(Guid systemId);
        Task<Result> DoImportConsentAsync(Stream data);
    }
}
