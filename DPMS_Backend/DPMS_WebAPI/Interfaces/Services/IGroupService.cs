using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;

namespace DPMS_WebAPI.Interfaces.Services
{
    public interface IGroupService : IService<Group> 
    {
        /// <summary>
        /// Please remove this
        /// </summary>
        /// <param name="id"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        Task UpdateAsync(Guid id, CreateGroupVM group);
        Task<int> AddUserToGroup(Guid groupId, List<Guid> userIds);
        Task<bool> UserBelongToGroupAsync(Guid userId, Guid groupId);
        Task<bool> UserBelongToGroupAsync(string email, string groupName);
        Task<GroupDetailVM> GetGroupDetailAsync(Guid groupId);
        Task<List<UserVM>> GetUsersInGroup(string groupName);
        Task<List<UserVM>> FetchUserInGlobalGroups();
        Task UpdateUserInGroup(Guid groupId, List<Guid> userIds);
        Task DeleteUserFromGroup(Guid groupId, List<Guid> userIds);
	}

}