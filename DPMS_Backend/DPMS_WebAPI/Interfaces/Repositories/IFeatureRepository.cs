
using DPMS_WebAPI.Models;

namespace DPMS_WebAPI.Interfaces.Repositories
{
    #pragma warning disable CS1591
    
    public interface IFeatureRepository : IRepository<Feature>
    {
        Task<List<Feature>> GetListNestedFeatures(); 
        Task<List<Feature>> GetFeaturesByGroupId(Guid groupId);
    }
}