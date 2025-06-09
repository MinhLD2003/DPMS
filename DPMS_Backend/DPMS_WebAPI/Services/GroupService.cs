using AutoMapper;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using FluentResults;
using Microsoft.VisualBasic;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace DPMS_WebAPI.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupService : BaseService<Group>, IGroupService
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GroupService> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="groupRepository"></param>
        /// <param name="groupFeatureRepository"></param>
        /// <param name="mapper"></param>
        public GroupService(
           IUnitOfWork unitOfWork,
           IGroupRepository groupRepository,
           IFeatureRepository groupFeatureRepository,
           IMapper mapper,
           ILogger<GroupService> logger
           )
           : base(unitOfWork)
        {
            _groupRepository = groupRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// May only delete user-custom group
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override Task<bool> DeleteAsync(object id)
        {
            // Get all constant fields of the struct
            Group? group = GetByIdAsync(id).Result;
            if (group == null)
                return Task.FromResult<bool>(false);

            // Get all constant fields of the struct
            var fields = typeof(PermissionGroup)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral && !f.IsInitOnly)
                .Select(f => f.GetRawConstantValue()); // Filters only constants

            if (fields.Contains(group.Name))
            {
                _logger.LogWarning("Cannot delete groups that are essential to DPMS");
                return Task.FromResult<bool>(false);
            }

            return base.DeleteAsync(id);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override IRepository<Group> Repository => _unitOfWork.Groups;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public async Task<int> AddUserToGroup(Guid groupId, List<Guid> userIds)
        {
            await _groupRepository.AddUserToGroup(groupId, userIds);

            return await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public override async Task<Group> AddAsync(Group group)
        {
            // by default, all Group created externally from API is global
            group.IsGlobal = true;

            Group? g = (await FindAsync(g => g.Name == group.Name)).FirstOrDefault();
            if (g != null)
            {
                throw new Exception($"Group {group.Name} already exists");
            }

            await _groupRepository.AddAsync(group);
            await _unitOfWork.SaveChangesAsync();
            return group;
        }

        /// <summary>
        /// ThangHQ note: Please override UpdateAsync from BaseService instead
        /// </summary>
        /// <param name="id"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task UpdateAsync(Guid id, CreateGroupVM group)
        {
            var existingGroup = await _groupRepository.GetDetailAsync(id,
                                 g => g.GroupFeatures,
                                 g => g.Users);

            if (existingGroup == null)
            {
                throw new KeyNotFoundException("Group not found");
            }

            _mapper.Map(group, existingGroup);
            _groupRepository.Update(existingGroup);
            if (group.FeatureIds != null)
            {
                var currentFeatureIds = existingGroup.GroupFeatures!.Select(gf => gf.FeatureId).ToList();

                var newFeatureIds = group.FeatureIds.Except(currentFeatureIds).ToList();
                // Add new features
                var newGroupFeatures = newFeatureIds.Select(featureId => new GroupFeature
                {
                    GroupId = existingGroup.Id,
                    FeatureId = featureId
                }).ToList();

                var removedFeatureIds = currentFeatureIds.Except(group.FeatureIds).ToList();
                var removedGroupFeatures = existingGroup.GroupFeatures!.Where(gf => removedFeatureIds.Contains(gf.FeatureId)).ToList();

                if (newGroupFeatures.Any()) { 
                    await _unitOfWork.GroupFeatures.BulkAddAsync(newGroupFeatures); 
                }
                if (removedGroupFeatures.Any()) { await _unitOfWork.GroupFeatures.BulkDeleteAsync(removedGroupFeatures); }
            }

            await _unitOfWork.SaveChangesAsync(); // Save changes
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task UpdateUserInGroup(Guid groupId, List<Guid> userIds)
        {
            var group = await _groupRepository.GetDetailAsync(groupId, g => g.Users!);

            if (group == null)
            {
                throw new KeyNotFoundException("Group not found");
            }

            var currentUsers = group.Users!.Select(u => u.Id).ToList();

            var newUsers = userIds.Except(currentUsers).ToList();
            var removedUsers = currentUsers.Except(userIds).ToList();

            if (newUsers.Any())
            {
                await _groupRepository.AddUserToGroup(groupId, newUsers);
            }

            if (removedUsers.Any())
            {
                await _groupRepository.DeleteUserFromGroup(groupId, removedUsers);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task DeleteUserFromGroup(Guid groupId, List<Guid> userIds)
        {
            try
            {
                await _groupRepository.DeleteUserFromGroup(groupId, userIds);
                await _unitOfWork.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async Task<bool> UserBelongToGroupAsync(Guid userId, Guid groupId)
        {
            var group = await _groupRepository.GetDetailAsync(groupId, g => g.Users);

            if (group == null)
            {
                return false;
            }

            if (group.Users!.Count == 0)
            {
                return false;
            }

            return group.Users.Any(u => u.Id == userId);
            // return group != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public async Task<bool> UserBelongToGroupAsync(string email, string groupName)
        {
            var group = await _groupRepository.GetUsersInGroup(groupName);

            var user = group.FirstOrDefault(u => u.Email == email);
            return user != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<GroupDetailVM> GetGroupDetailAsync(Guid groupId)
        {
            var group = await _groupRepository.GetDetailAsync(groupId,
                g => g.Users!,
                g => g.GroupFeatures!,
                g => g.Features!);

            if (group == null)
            {
                throw new KeyNotFoundException("Group not found");
            }

            return _mapper.Map<GroupDetailVM>(group);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public async Task<List<UserVM>> GetUsersInGroup(string groupName)
        {
            var users = await _groupRepository.GetUsersInGroup(groupName);
            users = users.Where(u => u.Status == Enums.UserStatus.Activated).ToList();
            return _mapper.Map<List<UserVM>>(users);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<UserVM>> FetchUserInGlobalGroups()
        {
            QueryParams queryParams = new QueryParams
            {
                PageNumber = 1,
                PageSize = 1000,
            };
            var groups = await _unitOfWork.Groups.GetPagedAsync(queryParams, g => g.Users);

            var users = groups.Data
                .Where(g => g.IsGlobal == true 
                    && g.SystemId == null 
                    && (g.Name == PermissionGroup.BO 
                    || g.Name == PermissionGroup.PD)
                    && g.Users != null )
                .SelectMany(g => g.Users )
                .Distinct()
                .Where(u => u.Status == Enums.UserStatus.Activated)
                .ToList();
            return _mapper.Map<List<UserVM>>(users);
        }

    }
}