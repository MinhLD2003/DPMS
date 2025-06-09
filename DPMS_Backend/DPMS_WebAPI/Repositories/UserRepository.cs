using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.User;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace DPMS_WebAPI.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(DPMSContext context) : base(context)
        {
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Status == Enums.UserStatus.Activated);
            return user;
        }

        public async Task<User?> UpdateUserPassword(string password, string salt, string email)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return null;
            }

            user.Password = password;
            user.Salt = salt;
            user.LastModifiedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<List<Feature?>> GetFeaturesByUserEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return new List<Feature?>();
            }

            var features = await _context.GroupFeatures
                .Where(gf => _context.UserGroups
                    .Any(ug => ug.GroupId == gf.GroupId && ug.UserId == user.Id))
                .Select(gf => gf.Feature)
                .Distinct()
                .ToListAsync();

            return features;
        }

        public async Task<List<ExternalSystem>> GetManageSystems(string email)
        {
            User? user = await _context.Users.Include(u => u.Groups).FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || user.Groups == null)
            {
                return new List<ExternalSystem>();
            }

            if (user.Groups.Any(g => new[] { PermissionGroup.ADMIN_DPMS, PermissionGroup.DPO }.Contains(g.Name)))
            {
                return await _context.ExternalSystems.ToListAsync();
            }

            List<Guid?> systemIds = user.Groups.Where(g => g.SystemId != null).Distinct().Select(g => g.SystemId).ToList();
            List<ExternalSystem> systems = await _context.ExternalSystems.Where(es => systemIds.Contains(es.Id)).ToListAsync();
            return systems;
        }

        public async Task UpdateLastLoginTimeStamp(User user)
        {
            user.LastTimeLogin = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task<Result<User>> GetUserProfileAsync(Guid userId)
        {
            bool found = _context.Users.Any(u => u.Id == userId);
            if (!found)
            {
                return Result.Fail<User>("User does not exist");
            }

            User profile = await _context.Users
                .Include(u => u.Groups)
                .ThenInclude(g => g.System)
                .Include(u => u.CreatedBy)
                .Include(u => u.LastModifiedBy)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return profile;
        }

        public async Task<Result<bool>> CheckUserInGroup(Guid userId, string groupName)
        {
            bool userExist = await _context.Users.AnyAsync(u=>u.Id == userId);
            if(!userExist)
            {
                return Result.Fail<bool>("User does not exist");
            }

            bool groupExist = await _context.Groups.AnyAsync(g=>g.Name==groupName);
            if(!groupExist)
            {
                return Result.Fail<bool>("Group does not exist");
            }

            var result = await _context.UserGroups.AnyAsync(ug => ug.UserId == userId && ug.Group.Name == groupName);
            return Result.Ok(result);
        }

        public async Task<bool> CheckUserExist(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
    }
}