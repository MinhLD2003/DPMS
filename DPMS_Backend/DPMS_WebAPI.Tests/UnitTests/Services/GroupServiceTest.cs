using System.Linq.Expressions;
using AutoMapper;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Services;
using DPMS_WebAPI.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;


namespace DPMS_WebAPI.Tests.UnitTests.Services
{
    public class GroupServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IGroupRepository> _mockGroupRepo;
        private readonly Mock<IFeatureRepository> _mockFeatureRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<GroupService>> _mockLogger;
        private readonly GroupService _service;

        public GroupServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockGroupRepo = new Mock<IGroupRepository>();
            _mockFeatureRepo = new Mock<IFeatureRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<GroupService>>();

            // Setup UnitOfWork.Groups to return the mocked group repo
            _mockUnitOfWork.Setup(u => u.Groups).Returns(_mockGroupRepo.Object);

            _service = new GroupService(
                _mockUnitOfWork.Object,
                _mockGroupRepo.Object,
                _mockFeatureRepo.Object,
                _mockMapper.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task AddAsync_Should_Add_New_Group_When_Not_Exists()
        {
            // Arrange
            var newGroup = new Group
            {
                Id = Guid.NewGuid(),
                Name = "Test Group",
                IsGlobal = false // should be set to true inside the service
            };
            _mockGroupRepo
                .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Group, bool>>>()))
                .ReturnsAsync(new List<Group>()); // No group with same name exists

            _mockGroupRepo
                .Setup(repo => repo.AddAsync(It.IsAny<Group>()))
                .ReturnsAsync(newGroup);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.AddAsync(newGroup);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Group", result.Name);
            Assert.True(result.IsGlobal); // Service should set this to true

            _mockGroupRepo.Verify(repo => repo.AddAsync(It.IsAny<Group>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddAsync_Should_Throw_Exception_When_Group_Exists()
        {
            // Arrange
            var existingGroup = new Group { Id = Guid.NewGuid(), Name = "Test Group" };

            _mockGroupRepo
                .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Group, bool>>>()))
                .ReturnsAsync(new List<Group> { existingGroup });

            var newGroup = new Group
            {
                Id = Guid.NewGuid(),
                Name = "Test Group"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.AddAsync(newGroup));
            Assert.Equal("Group Test Group already exists", ex.Message);

            _mockGroupRepo.Verify(repo => repo.AddAsync(It.IsAny<Group>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        // Add duplicate group name test
        [Fact]
        public async Task AddAsync_Should_Throw_Exception_When_Group_Name_Is_Duplicate()
        {
            // Arrange
            var existingGroup = new Group { Id = Guid.NewGuid(), Name = "Test Group" };

            _mockGroupRepo
                .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Group, bool>>>()))
                .ReturnsAsync(new List<Group> { existingGroup });

            var newGroup = new Group
            {
                Id = Guid.NewGuid(),
                Name = "Test Group"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.AddAsync(newGroup));
            Assert.Equal("Group Test Group already exists", ex.Message);

            _mockGroupRepo.Verify(repo => repo.AddAsync(It.IsAny<Group>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        // Delete group test
        [Fact]
        public async Task DeleteAsync_Should_Delete_Group_When_Exists()
        {
            // Arrange
            var existingGroup = new Group { Id = Guid.NewGuid(), Name = "Test Group" };

            _mockGroupRepo
                .Setup(repo => repo.GetByIdAsync(existingGroup.Id)).ReturnsAsync(existingGroup);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _service.DeleteAsync(existingGroup.Id);

            // Assert
            _mockGroupRepo.Verify(repo => repo.DeleteAsync(existingGroup.Id), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        // Delete non-existent group test
        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_Group_Does_Not_Exist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            _mockGroupRepo
                .Setup(repo => repo.GetByIdAsync(nonExistentId))
                .ReturnsAsync((Group)null);

            // Act
            var result = await _service.DeleteAsync(nonExistentId);

            // Assert
            Assert.False(result);
            _mockGroupRepo.Verify(repo => repo.DeleteAsync(nonExistentId), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        // Delete constant group test
        [Fact]
        public async Task DeleteAsync_Should_Throw_Exception_When_Group_Is_Constant()
        {
            // Arrange
            var constantGroup = new Group { Id = Guid.NewGuid(), Name = PermissionGroup.ADMIN_DPMS };

            _mockGroupRepo
                .Setup(repo => repo.GetByIdAsync(constantGroup.Id)).ReturnsAsync(constantGroup);

            // Act
            var result = await _service.DeleteAsync(constantGroup.Id);

            // Assert
            Assert.False(result);

            _mockGroupRepo.Verify(repo => repo.DeleteAsync(constantGroup.Id), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        // Update group test
        [Fact]
        public async Task UpdateAsync_Should_Update_Group_When_Exists()
        {
            // Arrange
            var existingGroup = new Group { Id = Guid.NewGuid(), Name = "Test Group" };

            _mockGroupRepo
                .Setup(repo => repo.GetDetailAsync(existingGroup.Id, It.IsAny<Expression<Func<Group, object>>[]>()))
                .ReturnsAsync(existingGroup);

            // Act
            await _service.UpdateAsync(existingGroup.Id, new CreateGroupVM { Name = "Updated Group" });
            // Assert
            _mockGroupRepo.Verify(repo => repo.Update(It.IsAny<Group>()), Times.Once);

            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        // Update non-existent group test
        [Fact]
        public async Task UpdateAsync_Should_Throw_Exception_When_Group_Does_Not_Exist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            _mockGroupRepo
                .Setup(repo => repo.GetDetailAsync(nonExistentId, It.IsAny<Expression<Func<Group, object>>>()))
                .ReturnsAsync((Group)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(nonExistentId, new CreateGroupVM { Name = "Updated Group" }));
            Assert.Equal("Group not found", ex.Message);

            _mockGroupRepo.Verify(repo => repo.Update(It.IsAny<Group>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        }



        // Add user to group test
        [Fact]
        public async Task AddUserToGroup_Should_Add_User_To_Group_When_Exists()
        {
            // Arrange
            var group = new Group { Id = Guid.NewGuid(), Name = "Test Group" };
            var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "test", FullName = "Test User" };

            _mockGroupRepo
                .Setup(repo => repo.GetDetailAsync(group.Id, It.IsAny<Expression<Func<Group, object>>>()))
                .ReturnsAsync(group);

            // Act
            await _service.AddUserToGroup(group.Id, new List<Guid> { user.Id });

            // Assert
            _mockGroupRepo.Verify(repo => repo.AddUserToGroup(group.Id, new List<Guid> { user.Id }), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);

        }

        // Update user in group test
        [Fact]
        public async Task UpdateUserInGroup_Should_Update_User_In_Group_When_Exists()
        {
            var userInGroupId = Guid.NewGuid();
            var userToAddId = Guid.NewGuid();
            // Arrange
            var group = new Group
            {
                Id = Guid.NewGuid(),
                Name = "Test Group",
                Users = new List<User>() {
                    new User {
                        Id = userInGroupId,
                        Email = "test@test.com",
                        UserName = "test",
                        FullName = "Test User"
                    }
                }
            };

            _mockGroupRepo
                .Setup(repo => repo.GetDetailAsync(It.IsAny<Guid>(), It.IsAny<Expression<Func<Group, object>>>()))
                .ReturnsAsync(group);

            _mockGroupRepo
                .Setup(repo => repo.AddUserToGroup(group.Id, It.Is<List<Guid>>(l => l.Contains(userToAddId))))
                .ReturnsAsync(1);

            _mockGroupRepo
                .Setup(repo => repo.DeleteUserFromGroup(group.Id, It.Is<List<Guid>>(l => l.Contains(userInGroupId))))
                .ReturnsAsync(1);

            // Act
            await _service.UpdateUserInGroup(group.Id, new List<Guid> { userToAddId });

            // Assert
            _mockGroupRepo.Verify(repo => repo.AddUserToGroup(group.Id, It.Is<List<Guid>>(l => l.Contains(userToAddId))), Times.Once);
            _mockGroupRepo.Verify(repo => repo.DeleteUserFromGroup(group.Id, It.Is<List<Guid>>(l => l.Contains(userInGroupId))), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        // Update user in non-existent group test
        [Fact]
        public async Task UpdateUserInGroup_Should_Throw_Exception_When_Group_Does_Not_Exist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            _mockGroupRepo
                .Setup(repo => repo.GetDetailAsync(It.IsAny<Guid>(), It.IsAny<Expression<Func<Group, object>>>()))
                .ReturnsAsync((Group)null);

            var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "test", FullName = "Test User" };

            _mockGroupRepo
                .Setup(repo => repo.AddUserToGroup(It.IsAny<Guid>(), It.IsAny<List<Guid>>()))
                .ReturnsAsync(1);

            _mockGroupRepo
                .Setup(repo => repo.DeleteUserFromGroup(It.IsAny<Guid>(), It.IsAny<List<Guid>>()))
                .ReturnsAsync(1);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateUserInGroup(nonExistentId, new List<Guid> { user.Id }));
            Assert.Equal("Group not found", ex.Message);

            _mockGroupRepo.Verify(repo => repo.AddUserToGroup(nonExistentId, new List<Guid> { user.Id }), Times.Never);
            _mockGroupRepo.Verify(repo => repo.DeleteUserFromGroup(nonExistentId, new List<Guid> { user.Id }), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        // User belong to group test
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UserBelongToGroupAsync_Should_Return_True_When_User_Belongs_To_Group(bool expectedResult)
        {
            // Arrange
            var group = new Group { Id = Guid.NewGuid(), Name = "Test Group" };
            var user = new User { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "test", FullName = "Test User" };

            if (expectedResult)
            {
                group.Users = new List<User> { user };
            }
            else
            {
                group.Users = new List<User>();
            }

            _mockGroupRepo
                .Setup(repo => repo.GetDetailAsync(
                    It.IsAny<Guid>(),
                    It.Is<Expression<Func<Group, object>>>(x => x.ToString().Contains("Users"))
                ))
                .ReturnsAsync(group);

            // Act
            var result = await _service.UserBelongToGroupAsync(user.Id, group.Id);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockGroupRepo.Verify(
                repo => repo.GetDetailAsync(
                    group.Id,
                    It.Is<Expression<Func<Group, object>>>(x => x.ToString().Contains("Users"))
                ),
                Times.Once
            );
        }



        // Get all groups test
        [Fact]
        public async Task GetAllAsync_Should_Return_All_Groups()
        {
            // Arrange
            var groups = new List<Group>
            {
                new Group { Id = Guid.NewGuid(), Name = "Group 1" },
                new Group { Id = Guid.NewGuid(), Name = "Group 2" }
            };

            _mockGroupRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(groups);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        // Get group detail test
        [Fact]
        public async Task GetGroupDetailAsync_Should_Return_Group_Detail_When_Exists()
        {
            // Arrange
            var group = new Group
            {
                Id = Guid.NewGuid(),
                Name = "Test Group",
                IsGlobal = true,
                Users = new List<User>(),
                GroupFeatures = new List<GroupFeature>(),
                Features = new List<Feature>()
            };

            var expectedVm = new GroupDetailVM
            {
                Id = group.Id,
                Name = group.Name
            };

            _mockGroupRepo
                .Setup(repo => repo.GetDetailAsync(group.Id, It.IsAny<Expression<Func<Group, object>>[]>()))
                .ReturnsAsync(group);

            _mockMapper
                .Setup(m => m.Map<GroupDetailVM>(group))
                .Returns(expectedVm);

            // Act
            var result = await _service.GetGroupDetailAsync(group.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(group.Id, result.Id);
            Assert.Equal(group.Name, result.Name);
        }

        // Get group detail test
        [Fact]
        public async Task GetGroupDetailAsync_Should_Throw_Exception_When_Group_Does_Not_Exist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            _mockGroupRepo
                .Setup(repo => repo.GetDetailAsync(nonExistentId, It.IsAny<Expression<Func<Group, object>>[]>()))
                .ReturnsAsync((Group)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetGroupDetailAsync(nonExistentId));
            Assert.Equal("Group not found", ex.Message);

            _mockGroupRepo.Verify(repo => repo.GetDetailAsync(nonExistentId, It.IsAny<Expression<Func<Group, object>>[]>()), Times.Once);
        }

        // Get users in group test
        [Fact]
        public async Task GetUsersInGroup_Should_Return_Users_In_Group_When_Exists()
        {
            // Arrange
            var group = new Group { Id = Guid.NewGuid(), Name = "Test Group" };
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "test", FullName = "Test User" },
                new User { Id = Guid.NewGuid(), Email = "test2@test.com", UserName = "test2", FullName = "Test User 2" }
            };

            group.Users = users;

            _mockGroupRepo
                .Setup(repo => repo.GetUsersInGroup(group.Name))
                .ReturnsAsync(users);

            _mockMapper
                .Setup(m => m.Map<List<UserVM>>(users))
                .Returns(users.Select(u => new UserVM { Id = u.Id, Email = u.Email, FullName = u.FullName }).ToList());

            // Act  
            var result = await _service.GetUsersInGroup(group.Name);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        // Fetch users in global group test
        [Fact]
        public async Task FetchUserInGlobalGroups_Should_Return_Users_In_Global_Group_When_Exists()
        {
            // Arrange
            var group = new Group { Id = Guid.NewGuid(), Name = PermissionGroup.ADMIN_DPMS, IsGlobal = true };

            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), Email = "test@test.com", UserName = "test", FullName = "Test User" },
                new User { Id = Guid.NewGuid(), Email = "test2@test.com", UserName = "test2", FullName = "Test User 2" }
            };

            group.Users = users;
 
            _mockUnitOfWork.Setup(u => u.Groups.GetPagedAsync(It.IsAny<QueryParams>(),
                It.IsAny<Expression<Func<Group, object>>[]>()))
                .ReturnsAsync(new PagedResponse<Group>
                {
                    Data = new List<Group> { group },
                    PageNumber = 1,
                    PageSize = 1
                });

            _mockMapper
                .Setup(m => m.Map<List<UserVM>>(users))
                .Returns(users.Select(u => new UserVM { Id = u.Id, Email = u.Email, FullName = u.FullName }).ToList());

            // Act
            var result = await _service.FetchUserInGlobalGroups();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

    }

}