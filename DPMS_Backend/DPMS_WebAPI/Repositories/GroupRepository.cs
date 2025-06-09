using AutoMapper;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DPMS_WebAPI.Repositories
{
    public class GroupRepository : BaseRepository<Group>, IGroupRepository
    {
        //private readonly DPMSContext _context;

        public GroupRepository(DPMSContext context) : base(context)
        {
            //_context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<int> AddUserToGroup(Guid groupId, List<Guid> userIds)
        {
            var group = await _context.Groups
               .Include(g => g.Users)
               .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                throw new Exception($"Group with id {groupId} not found");
            }

            var userIdSet = userIds.ToHashSet();
            var existingUserIdSet = group.Users.Select(u => u.Id).ToHashSet();

            var newUsers = await _context.Users
                .Where(u => userIdSet.Contains(u.Id) && !existingUserIdSet.Contains(u.Id))
                .ToListAsync();

            if (!newUsers.Any())
            {
                return -1;
            }

            group.Users.AddRange(newUsers);
            return 1;
        }

        public async Task<int> DeleteUserFromGroup(Guid groupId, List<Guid> userIds)
        {
            var group = await _context.Groups
                .Include(g => g.Users)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                throw new Exception($"Group with id {groupId} not found");
            }

            var userIdSet = userIds.ToHashSet();
            var existingUserIdSet = group.Users.Select(u => u.Id).ToHashSet();

            var usersToRemove = group.Users.Where(u => userIdSet.Contains(u.Id)).ToList();

            if (!usersToRemove.Any())
            {
                return -1;
            }

            group.Users.RemoveAll(u => userIdSet.Contains(u.Id));
            return 1;
        }

        public async Task<List<User>> GetUsersInGroup(string groupName)
        {
            Group? group = await _context.Groups.Include(g => g.Users).FirstOrDefaultAsync(g => g.Name == groupName);

            if (group == null)
            {
                return new List<User>();
            }
            return group.Users.ToList();
        }

        public async Task<List<Group>> GetGroupsBySystemIdAsync(Guid systemId)
        {
            return await _context.Groups.Where(g => g.SystemId == systemId).ToListAsync();
        }

        public async Task<bool> UserBelongToGroupAsync(Guid userId, Guid groupId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UserBelongToGroupAsync(string email, string groupName)
        {
            throw new NotImplementedException();
        }
    }
}
