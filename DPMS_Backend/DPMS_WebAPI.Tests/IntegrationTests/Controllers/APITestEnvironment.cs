using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Services;
using Microsoft.Extensions.Logging;

namespace DPMS_WebAPI.Tests.IntegrationTests.Controllers
{
    public class APITestEnvironment : ServiceTestEnvironment
    {   

        protected readonly IGroupService _groupService;
        protected readonly ILogger<GroupService> _groupServiceLogger;
        protected readonly IUserService _userService;
        protected readonly ILogger<UserService> _userServiceLogger;
        protected readonly IIssueTicketDocumentService _issueTicketDocumentService;
        protected readonly ILogger<IssueTicketDocumentService> _issueTicketDocumentServiceLogger;
        protected readonly IIssueTicketService _issueTicketService;
        protected readonly ILogger<IssueTicketService> _issueTicketServiceLogger;

        public APITestEnvironment() : base()
        {
            _groupServiceLogger = new LoggerFactory().CreateLogger<GroupService>();
            _groupService = new GroupService(_unitOfWork, _groupRepository, _featureRepository, _mapper, _groupServiceLogger);

            _userServiceLogger = new LoggerFactory().CreateLogger<UserService>();
            _userService = new UserService(_unitOfWork, _userRepository, _mapper);

            _issueTicketDocumentServiceLogger = new LoggerFactory().CreateLogger<IssueTicketDocumentService>();
            _issueTicketDocumentService = new IssueTicketDocumentService(_unitOfWork, _fileRepository);

            _issueTicketServiceLogger = new LoggerFactory().CreateLogger<IssueTicketService>();
            _issueTicketService = new IssueTicketService(_unitOfWork, _mapper, _fileRepository);

        }

        public async Task<Group> CreateGroup(string name)
        {
            var group = new Group()
            {
                Name = name
            };
            await _groupRepository.AddAsync(group);
            return group;
        }

        public async Task<User> CreateUser(string fullName, string email, string userName)
        {
            var user = new User()
            {
                FullName = fullName,
                Email = email,  
                UserName = userName,
                Groups = new List<Group>()
            };
            await _userRepository.AddAsync(user);
            return user;
        }
        
        public async Task AddUserToGroup(Guid userId, Guid groupId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            var group = await _groupRepository.GetByIdAsync(groupId);
            user.Groups.Add(group);
        }

        public async Task RemoveUserFromGroup(Guid userId, Guid groupId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            var group = await _groupRepository.GetByIdAsync(groupId);
            user.Groups.Remove(group);
        }
    }
}
