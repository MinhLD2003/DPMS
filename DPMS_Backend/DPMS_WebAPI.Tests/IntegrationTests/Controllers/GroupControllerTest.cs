using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Controllers;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DPMS_WebAPI.Tests.IntegrationTests.Controllers
{
    public class GroupControllerTest : APITestEnvironment
    {
        private readonly GroupController _controller;
        
        private readonly ILogger<GroupController> _controllerLogger;

        public GroupControllerTest()
        {
            _controllerLogger = new LoggerFactory().CreateLogger<GroupController>();
            _controller = new GroupController(
                _groupService,
                _mapper,
                _controllerLogger,
                _userService
            );
        }

        [Fact]
        public async Task GetGroups_ShouldReturnOkResult_WithGroups()
        {
            // Arrange
            var queryParams = new QueryParams();
            var groups = new List<Group>
            {
                new Group { Id = Guid.NewGuid(), Name = "Test Group 1" },
                new Group { Id = Guid.NewGuid(), Name = "Test Group 2" }
            };
            foreach (var group in groups)
            {
                await _groupRepository.AddAsync(group);
            }
            await _unitOfWork.SaveChangesAsync();

            // Act
            var result = await _controller.GetGroups(queryParams);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedResponse<GroupVM>>(okResult.Value);
            Assert.Equal(2, returnValue.Data.Count);
            Assert.Equal("Test Group 1", returnValue.Data.First().Name);
        }

        [Fact]
        public async Task CreateGroup_ShouldReturnOkResult_WhenGroupIsCreated()
        {
            // Arrange
            var groupVM = new GroupVM
            {
                Name = "Test Group",
                Description = "Test Description"
            };

            // Act
            var result = await _controller.CreateGroup(groupVM);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Group>(okResult.Value);
            Assert.Equal(groupVM.Name, returnValue.Name);
            Assert.Equal(groupVM.Description, returnValue.Description);
        }

        [Fact]
        public async Task CreateGroup_ShouldReturnBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var groupVM = new GroupVM
            {
                Name = null,
                Description = "Test Description"
            };

            // Act
            var result = await _controller.CreateGroup(groupVM);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task GetUsersInGroup_ShouldReturnOkResult_WithUsers()
        {
            // Arrange
            var groupName = PermissionGroup.QA;

            var group = await CreateGroup(groupName);
            
            var user1 = await CreateUser("User 1", "user1@test.com", "user1");
            var user2 = await CreateUser("User 2", "user2@test.com", "user2");
            await AddUserToGroup(user1.Id, group.Id);
            await AddUserToGroup(user2.Id, group.Id);

            await _unitOfWork.SaveChangesAsync();

            // Act
            var result = await _controller.GetUsersInGroup(groupName);

            // Assert
            var actionResult = Assert.IsType<ActionResult<List<UserVM>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnValue = Assert.IsType<List<UserVM>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Equal("User 1", returnValue[0].FullName);
        }

        [Fact]
        public async Task AddUsersToGroup_ShouldReturnOkResult_WhenUsersAreAdded()
        {
            // Arrange
            var group = new Group { Id = Guid.NewGuid(), Name = "Test Group" };
            await _groupRepository.AddAsync(group);
            
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), FullName = "User 1", Email = "user1@test.com", UserName = "user1" },
                new User { Id = Guid.NewGuid(), FullName = "User 2", Email = "user2@test.com", UserName = "user2" }
            };
            foreach (var user in users)
            {
                await _userRepository.AddAsync(user);
            }
            await _unitOfWork.SaveChangesAsync();

            var userIds = users.Select(u => u.Id).ToList();

            // Act
            var result = await _controller.AddUsersToGroup(group.Id, userIds);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Added user to group sucessfully", okResult.Value);
        }

        [Fact]
        public async Task AddUsersToGroup_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var userIds = new List<Guid>();
            _controller.ModelState.AddModelError("userIds", "User IDs are required");

            // Act
            var result = await _controller.AddUsersToGroup(groupId, userIds);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task GetGroupById_ShouldReturnOkResult_WithGroup()
        {
            // Arrange
            var group = new Group { Id = Guid.NewGuid(), Name = "Test Group", Description = "Test Description" };
            await _groupRepository.AddAsync(group);
            await _unitOfWork.SaveChangesAsync();

            // Act
            var result = await _controller.GetGroupById(group.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<GroupDetailVM>(okResult.Value);
            Assert.Equal(group.Id, returnValue.Id);
            Assert.Equal("Test Group", returnValue.Name);
        }

        [Fact]
        public async Task UpdateGroup_ShouldReturnOkResult_WhenGroupIsUpdated()
        {
            // Arrange
            var group = new Group { Id = Guid.NewGuid(), Name = "Test Group", Description = "Test Description" };
            await _groupRepository.AddAsync(group);
            await _unitOfWork.SaveChangesAsync();

            var groupVM = new GroupVM
            {
                Id = group.Id,
                Name = "Updated Group",
                Description = "Updated Description"
            };

            // Act
            var result = await _controller.UpdateGroup(group.Id, groupVM);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Updated group successfully", okResult.Value);
        }

        [Fact]
        public async Task DeleteGroup_ShouldReturnOkResult_WhenGroupIsDeleted()
        {
            // Arrange
            var group = new Group { Id = Guid.NewGuid(), Name = "Test Group" };
            await _groupRepository.AddAsync(group);
            await _unitOfWork.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteGroup(group.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Deleted group successfully", okResult.Value);
        }

        [Fact]
        public async Task DeleteGroup_ShouldReturnBadRequest_WhenDeletionFails()
        {
            // Arrange
            var groupId = Guid.NewGuid();

            // Act
            var result = await _controller.DeleteGroup(groupId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Delete group failed", badRequestResult.Value);
        }

        [Fact]
        public async Task GetUsersNotInGroup_ShouldReturnListOfUsers()
        {
            // Arrange
            var group = await CreateGroup("Test Group");
            var user1 = await CreateUser("User 1", "user1@test.com", "user1");
            var user2 = await CreateUser("User 2", "user2@test.com", "user2");

            await _unitOfWork.SaveChangesAsync();

            // Act
            var result = await _controller.GetUsersNotInGroup(group.Id);

            // Assert
            var actionResult = Assert.IsType<ActionResult<List<UserVM>>>(result);
            
            Assert.Equal(2, actionResult.Value.Count);
        }

        [Fact]
        public async Task UpdateUserInGroup_ShouldReturnOkResult_WhenUsersAreUpdated()
        {
            // Arrange
            var group = new Group { Id = Guid.NewGuid(), Name = "Test Group" };
            await _groupRepository.AddAsync(group);
            
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), FullName = "User 1", Email = "user1@test.com", UserName = "user1" },
                new User { Id = Guid.NewGuid(), FullName = "User 2", Email = "user2@test.com", UserName = "user2" }
            };
            foreach (var user in users)
            {
                await _userRepository.AddAsync(user);
            }
            await _unitOfWork.SaveChangesAsync();

            var userIds = users.Select(u => u.Id).ToList();

            // Act
            var result = await _controller.UpdateUserInGroup(group.Id, userIds);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Updated users in group successfully", okResult.Value);
        }

        [Fact]
        public async Task RemoveUserFromGroup_ShouldReturnOkResult_WhenUsersAreRemoved()
        {
            // Arrange
            var group = new Group { Id = Guid.NewGuid(), Name = "Test Group" };
            await _groupRepository.AddAsync(group);
            
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), FullName = "User 1", Email = "user1@test.com", UserName = "user1" },
                new User { Id = Guid.NewGuid(), FullName = "User 2", Email = "user2@test.com", UserName = "user2" }
            };
            foreach (var user in users)
            {
                await _userRepository.AddAsync(user);
            }
            await _unitOfWork.SaveChangesAsync();

            var userIds = users.Select(u => u.Id).ToList();

            // Act
            var result = await _controller.RemoveUserFromGroup(group.Id, userIds);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Removed user from group sucessfully", okResult.Value);
        }
    
        

    }
}
