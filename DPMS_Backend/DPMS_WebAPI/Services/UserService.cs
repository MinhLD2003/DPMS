using AutoMapper;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Utils;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.ExternalSystem;
using DPMS_WebAPI.ViewModels.User;
using FluentResults;

namespace DPMS_WebAPI.Services
{
    public class UserService : BaseService<User>, IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IUserRepository userRepository, IMapper mapper) : base(unitOfWork)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        protected override IRepository<User> Repository
        {
            get { return _userRepository; }
        }

        public async Task ChangePassword(string email, ChangePasswordVM model)
        {
            User? user = await GetUserByEmailAsync(email);
            if (user == null)
            {
                throw new Exception($"Account {email} does not exist");
            }

            string salt = user.Salt;
            if (PasswordUtils.HashPassword(model.OldPassword, salt) != user.Password)
            {
                throw new Exception("Old password is incorrect");
            }

            // change password
            string newPassword = PasswordUtils.HashPassword(model.NewPassword, salt);
            user.Password = newPassword;

            await UpdateAsync(user);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

        public Task<User> UpdateUserPassword(string password, string salt, string email)
        {
            return _userRepository.UpdateUserPassword(password, salt, email);
        }

        public async Task<List<Feature>> GetFeaturesByUserEmailAsync(string email)
        {
            return await _userRepository.GetFeaturesByUserEmailAsync(email);
        }

        public async Task<List<ExternalSystemVM>> GetManageSystems(string email)
        {
            // get all system this user manage
            List<ExternalSystem> systems = await _userRepository.GetManageSystems(email);

            // get SystemVM instance from ExternalSystem
            List<ExternalSystemVM> vm = _mapper.Map<List<ExternalSystemVM>>(systems);
            return vm;
        }

        public async Task UpdateLastLoginTimeStamp(User user)
        {
            await _userRepository.UpdateLastLoginTimeStamp(user);
        }

        public async Task UpdateUserStatus(UpdateUserStatusVM model)
        {
            var user = await _userRepository.GetByIdAsync(model.Id);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            user.Status = model.Status;
            await UpdateAsync(user);
        }

        public async Task<Result<ProfileVM>> GetUserProfileAsync(Guid userId)
        {
            Result<User> user = await _userRepository.GetUserProfileAsync(userId);
            if (user.IsFailed)
            {
                return Result.Fail<ProfileVM>(user.Errors);
            }

            ProfileVM profile = _mapper.Map<ProfileVM>(user.Value);
            return profile;
        }

        public async Task<Result<bool>> CheckUserInGroup(Guid userId, string groupName)
        {
            return await _userRepository.CheckUserInGroup(userId, groupName);
        }

        public async Task<bool> CheckUserExist(string email)
        {
            return await _userRepository.CheckUserExist(email);
        }
    }
}