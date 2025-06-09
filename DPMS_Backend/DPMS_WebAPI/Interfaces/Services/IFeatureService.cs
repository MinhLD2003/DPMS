using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;

namespace DPMS_WebAPI.Interfaces.Services
{
  public interface IFeatureService : IService<Feature>
  {
    bool UserHasFeature(string email, string featureName);
    bool AddFeaturesToGroup(List<Guid> featureIds, Guid groupId);
    // Feature FindFeatureByName(string featureName);
    Task<List<FeatureVM>> GetListNestedFeatures(Guid groupId);
    }
}