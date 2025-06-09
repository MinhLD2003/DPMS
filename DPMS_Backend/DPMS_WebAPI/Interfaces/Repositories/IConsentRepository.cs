using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.Consent;
using System.Linq.Expressions;

namespace DPMS_WebAPI.Interfaces.Repositories
{
    #pragma warning disable CS1591
    
    public interface IConsentRepository : IRepository<Consent>
    {
        Task<Consent> GetConsentByEmailAsync(string email);
        Task<PagedResponse<Consent>> GetConsentsWithPurposeAsync(QueryParams queryParams);
    }
}
