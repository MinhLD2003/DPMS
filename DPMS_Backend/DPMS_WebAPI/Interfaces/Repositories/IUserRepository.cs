using DPMS_WebAPI.Models;
using FluentResults;

namespace DPMS_WebAPI.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> UpdateUserPassword(string password, string salt, string email);
        Task<User?> GetUserByEmailAsync(string email);
        Task<List<Feature?>> GetFeaturesByUserEmailAsync(string email);
        Task UpdateLastLoginTimeStamp(User user);
        /// <summary>
        /// Get all external systems an account manage. If there is no account with this email, or this account
        /// does not have any group (therefore sure that account does not manage any system), this method return empty list
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<List<ExternalSystem>> GetManageSystems(string email);
        Task<Result<User>> GetUserProfileAsync(Guid userId);
        Task<Result<bool>> CheckUserInGroup(Guid userId, string groupName);
        Task<bool> CheckUserExist(string email);
    }
}