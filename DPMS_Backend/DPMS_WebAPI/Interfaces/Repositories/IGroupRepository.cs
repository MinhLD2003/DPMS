using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;

namespace DPMS_WebAPI.Interfaces.Repositories
{
	public interface IGroupRepository : IRepository<Group>
    {
        Task<int> AddUserToGroup(Guid groupId, List<Guid> userIds);
        Task<bool> UserBelongToGroupAsync(Guid userId, Guid groupId);
        Task<bool> UserBelongToGroupAsync(string email, string groupName);
        Task<int> DeleteUserFromGroup(Guid groupId, List<Guid> userIds);
        Task<List<User>> GetUsersInGroup(string groupName);
        Task<List<Group>> GetGroupsBySystemIdAsync(Guid systemId);
    }
}