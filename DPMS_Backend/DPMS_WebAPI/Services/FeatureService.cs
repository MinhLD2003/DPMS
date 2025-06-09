using AutoMapper;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DPMS_WebAPI.Services
{
    public class FeatureService : BaseService<Feature>, IFeatureService
    {
        private readonly DPMSContext _context;
        private readonly IMapper _mapper;

        public FeatureService(IUnitOfWork unitOfWork, IMapper mapper, DPMSContext context)
           : base(unitOfWork)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        protected override IRepository<Feature> Repository => _unitOfWork.Features;

        // Need to modify code to BaseRepo and BaseService
        public bool AddFeaturesToGroup(List<Guid> featureIds, Guid groupId)
        {
            Group? group = _context.Groups.Include(g => g.Features).FirstOrDefault(g => g.Id == groupId);
            if (group == null)
            {
                throw new Exception("Group not found");
            }

            if (featureIds.Count == 0)
            {
                throw new Exception("Feature list if empty");
            }

            List<Feature> features = _context.Features.Where(f => featureIds.Contains(f.Id)).ToList();
            group.Features = features;
            //foreach (var feature in features)
            //{
            //    group.Features.Add(feature);
            //}

            _context.SaveChanges();
            return true;
        }


        public async Task<List<FeatureVM>> GetListNestedFeatures(Guid groupId)
        {
            // Get and map features by group ID
            var listFeaturesByGroupId = _mapper.Map<List<FeatureVM>>(await _unitOfWork.Features.GetFeaturesByGroupId(groupId));

            // Get and map all features with their hierarchy
            var listFeatures = await _unitOfWork.Features.GetListNestedFeatures();
            List<FeatureVM> listFeatureVM = _mapper.Map<List<FeatureVM>>(listFeatures);

            // Create a HashSet of IDs for faster lookup
            var groupFeatureIds = new HashSet<Guid>(listFeaturesByGroupId.Select(f => f.Id));

            // Define a local recursive function to process features and return true if any child is checked
            bool ProcessFeatures(FeatureVM feature)
            {
                // Check if this feature is directly in the group
                bool isDirectlyChecked = groupFeatureIds.Contains(feature.Id);

                // Process children recursively and check if any child is checked
                bool hasCheckedChild = false;
                if (feature.Children != null && feature.Children.Any())
                {
                    foreach (var child in feature.Children)
                    {
                        bool childChecked = ProcessFeatures(child);
                        hasCheckedChild = hasCheckedChild || childChecked;
                    }
                }

                // Mark this feature as checked if it's directly checked or has any checked children
                feature.isChecked = isDirectlyChecked || hasCheckedChild;

                return (bool)feature.isChecked;
            }

            // Process each top-level feature
            foreach (var feature in listFeatureVM)
            {
                ProcessFeatures(feature);
            }

            return listFeatureVM;
        }
        /// <summary>
        /// TODO: Implement parent's feature, child features
        /// </summary>
        /// <param name="email"></param>
        /// <param name="featureName"></param>
        /// <returns></returns>
        public bool UserHasFeature(string email, string featureName)
        {
            return _context.Database.SqlQuery<int>($"SELECT 1 FROM Users JOIN UserGroups ON Users.Id = UserGroups.UserId JOIN Groups ON Groups.Id = UserGroups.GroupId JOIN GroupFeatures ON GroupFeatures.GroupId = Groups.Id JOIN Features ON Features.Id = GroupFeatures.FeatureId WHERE Users.Email = {email} AND Features.FeatureName = {featureName};").ToList().Any();
        }
    }
}
