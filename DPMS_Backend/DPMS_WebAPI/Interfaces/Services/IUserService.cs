using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.ExternalSystem;
using DPMS_WebAPI.ViewModels.User;
using FluentResults;

namespace DPMS_WebAPI.Interfaces.Services
{
	public interface IUserService : IService<User> 
    {
        Task<User> UpdateUserPassword(string password, string salt, string email);
        Task<User?> GetUserByEmailAsync(string email);
        Task UpdateLastLoginTimeStamp(User user);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="email">Email of the account need to change password</param>
        /// <param name="model">Object contain new password</param>
        /// <returns></returns>
        Task ChangePassword(string email, ChangePasswordVM model);
        /// <summary>
        /// Get all External System an account manage
        /// </summary>
        /// <param name="email">Email of the account</param>
        /// <returns></returns>
        Task<List<ExternalSystemVM>> GetManageSystems(string email);
        Task UpdateUserStatus(UpdateUserStatusVM model);
        Task<Result<ProfileVM>> GetUserProfileAsync(Guid userId);
        Task<Result<bool>> CheckUserInGroup(Guid userId, string groupName);
        Task<bool> CheckUserExist(string email);
    }
}