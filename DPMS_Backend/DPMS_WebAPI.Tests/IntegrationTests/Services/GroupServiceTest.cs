using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Enums;
using DPMS_WebAPI.Interfaces.Services;
using Microsoft.Extensions.Logging;
using DPMS_WebAPI.Services;

namespace DPMS_WebAPI.Tests.IntegrationTests.Services
{
    public class GroupServiceTest : ServiceTestEnvironment
    {
        private readonly IGroupService _groupService;
        private readonly ILogger<GroupService> _logger;
        public GroupServiceTest()
        {
            _logger = new LoggerFactory().CreateLogger<GroupService>();
            _groupService = new GroupService(_unitOfWork, _groupRepository, _featureRepository, _mapper, _logger);
        }

        #region AddAsync Tests
        [Fact]
        public async Task AddAsync_ShouldAddGroup_WhenGroupDoesNotExist()
        {
            // Arrange
            var newGroup = new Group { Name = "Test Group", Description = "Test Description" };

            // Act
            var result = await _groupService.AddAsync(newGroup);
            var groups = await _groupRepository.GetAllAsync();

            // Assert
            Assert.Single(groups);
            Assert.Equal("Test Group", groups.First().Name);
            Assert.Equal("Test Description", groups.First().Description);
            Assert.True(groups.First().IsGlobal); // Default value
        }


        [Fact]
        public async Task AddAsync_ShouldThrowException_WhenGroupAlreadyExists()
        {
            // Arrange
            var newGroup = new Group { Name = "Test Group" };
            await _groupService.AddAsync(newGroup);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _groupService.AddAsync(newGroup));
            Assert.Equal($"Group {newGroup.Name} already exists", exception.Message);
        }
        #endregion

        #region DeleteAsync Tests
        [Fact]
        public async Task DeleteAsync_ShouldDeleteGroup_WhenGroupIsNotEssential()
        {
            // Arrange
            var group = new Group { Id = Guid.NewGuid(), Name = "Custom Group" };
            await _groupService.AddAsync(group);

            // Act
            var result = await _groupService.DeleteAsync(group.Id);
            var groups = await _groupRepository.GetAllAsync();

            // Assert
            Assert.True(result);
            Assert.Empty(groups);
        }

        [Fact]
        public async Task DeleteAsync_ShouldNotDeleteGroup_WhenGroupIsEssential()
        {
            // Arrange
            var essentialGroup = new Group { Id = Guid.NewGuid(), Name = PermissionGroup.ADMIN_DPMS };
            await _groupService.AddAsync(essentialGroup);

            // Act
            var result = await _groupService.DeleteAsync(essentialGroup.Id);
            var groups = await _groupRepository.GetAllAsync();

            // Assert
            Assert.False(result);
            Assert.Single(groups);
            Assert.Equal(PermissionGroup.ADMIN_DPMS, groups.First().Name);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenGroupDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _groupService.DeleteAsync(nonExistentId);

            // Assert
            Assert.False(result);
        }
        #endregion

        #region UpdateAsync Tests
        [Fact]
        public async Task UpdateAsync_ShouldUpdateGroup_WhenGroupExists()
        {
            // Arrange
            var group = new Group { Id = Guid.NewGuid(), Name = "Old Name", Description = "Old Description" };
            await _groupService.AddAsync(group);
            var updateData = new CreateGroupVM { Name = "New Name", Description = "New Description" };

            // Act
            await _groupService.UpdateAsync(group.Id, updateData);
            var updatedGroup = await _groupRepository.GetByIdAsync(group.Id);

            // Assert
            Assert.NotNull(updatedGroup);
            Assert.Equal("New Name", updatedGroup.Name);
            Assert.Equal("New Description", updatedGroup.Description);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenGroupDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var updateData = new CreateGroupVM { Name = "New Name" };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _groupService.UpdateAsync(nonExistentId, updateData));
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateFeatures_WhenFeatureIdsAreProvided()
        {
            // Arrange
            var group = new Group { Id = Guid.NewGuid(), Name = "Test Group" };
            await _groupService.AddAsync(group);
            var f1 = await _unitOfWork.Features.AddAsync(new Feature
            {
                FeatureName = "Feature 1",
                State = FeatureState.ENABLED,
                Description = "Test Feature 1"
            });
            var f2 = await _unitOfWork.Features.AddAsync(new Feature
            {
                FeatureName = "Feature 2",
                State = FeatureState.ENABLED,
                Description = "Test Feature 2"
            });
            await _unitOfWork.SaveChangesAsync();

            var updateData = new CreateGroupVM { Name = "Test Group", FeatureIds = new List<Guid> { f1.Id, f2.Id } };

            // Act
            await _groupService.UpdateAsync(group.Id, updateData);
            var updatedGroup = await _groupRepository.GetDetailAsync(group.Id, g => g.GroupFeatures);

            // Assert
            Assert.Equal(2, updatedGroup.GroupFeatures.Count);
            Assert.Contains(f1.Id, updatedGroup.GroupFeatures.Select(gf => gf.FeatureId));
            Assert.Contains(f2.Id, updatedGroup.GroupFeatures.Select(gf => gf.FeatureId));
        }
        #endregion

        #region User Management Tests
        [Fact]
        public async Task AddUserToGroup_ShouldAddUsers_WhenGroupExists()
        {
            // Arrange
            var group = new Group { Id = Guid.NewGuid(), Name = "Test Group" };
            await _groupService.AddAsync(group);
            var u1 = await _unitOfWork.Users.AddAsync(new User { Id = Guid.NewGuid(), Email = "test1@example.com", UserName = "testuser1", FullName = "Test User 1" });
            var u2 = await _unitOfWork.Users.AddAsync(new User { Id = Guid.NewGuid(), Email = "test2@example.com", UserName = "testuser2", FullName = "Test User 2" });
            await _unitOfWork.SaveChangesAsync();

            // Act
            var result = await _groupService.AddUserToGroup(group.Id, new List<Guid> { u1.Id, u2.Id });
            var updatedGroup = await _groupRepository.GetDetailAsync(group.Id, g => g.Users);

            // Asser    t
            Assert.Equal(2, result);
            Assert.Contains(u1.Id, updatedGroup.Users.Select(u => u.Id));
            Assert.Contains(u2.Id, updatedGroup.Users.Select(u => u.Id));
        }

        [Fact]
        public async Task DeleteUserFromGroup_ShouldRemoveUsers_WhenUsersExist()
        {
            // Arrange
            var group = new Group { Id = Guid.NewGuid(), Name = "Test Group" };

            // Add group and users
            var u1 = await _unitOfWork.Users.AddAsync(new User { Id = Guid.NewGuid(), Email = "test1@example.com", UserName = "testuser1", FullName = "Test User 1" });
            var u2 = await _unitOfWork.Users.AddAsync(new User { Id = Guid.NewGuid(), Email = "test2@example.com", UserName = "testuser2", FullName = "Test User 2" });
            await _unitOfWork.SaveChangesAsync();

            await _groupService.AddAsync(group);
            await _groupService.AddUserToGroup(group.Id, new List<Guid> { u1.Id, u2.Id });

            // Act
            await _groupService.DeleteUserFromGroup(group.Id, new List<Guid> { u1.Id });
            var updatedGroup = await _groupRepository.GetDetailAsync(group.Id, g => g.Users);

            // Assert
            Assert.Single(updatedGroup.Users);
            Assert.Contains(u2.Id, updatedGroup.Users.Select(u => u.Id));
        }

        [Fact]
        public async Task UpdateUserInGroup_ShouldUpdateUsers_WhenGroupExists()
        {
            // Arrange
            var group = new Group { Id = Guid.NewGuid(), Name = "Test Group" };

            // Add group and users
            var u1 = await _unitOfWork.Users.AddAsync(new User { Id = Guid.NewGuid(), Email = "test1@example.com", UserName = "testuser1", FullName = "Test User 1" });
            var u2 = await _unitOfWork.Users.AddAsync(new User { Id = Guid.NewGuid(), Email = "test2@example.com", UserName = "testuser2", FullName = "Test User 2" });
            var u3 = await _unitOfWork.Users.AddAsync(new User { Id = Guid.NewGuid(), Email = "test3@example.com", UserName = "testuser3", FullName = "Test User 3" });
            await _unitOfWork.SaveChangesAsync();

            var initialUserIds = new List<Guid> { u1.Id };
            var newUserIds = new List<Guid> { u2.Id, u3.Id };
            await _groupService.AddAsync(group);
            await _groupService.AddUserToGroup(group.Id, initialUserIds);

            // Act
            await _groupService.UpdateUserInGroup(group.Id, newUserIds);
            var updatedGroup = await _groupRepository.GetDetailAsync(group.Id, g => g.Users);

            // Assert
            Assert.Equal(2, updatedGroup.Users.Count);
            Assert.Equal(newUserIds, updatedGroup.Users.Select(u => u.Id));
        }
        #endregion

        #region UserBelongToGroup Tests
        [Fact]
        public async Task UserBelongToGroupAsync_ShouldReturnTrue_WhenUserIsInGroup()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var group = new Group
            {
                Id = Guid.NewGuid(),
                Name = "Test Group",
                Users = new List<User>
            {
                new User
                {
                    Id = userId,
                    Email = "test@example.com",
                    UserName = "testuser",
                    FullName = "Test User"
                }
            }
            };
            await _groupService.AddAsync(group);

            // Act
            var result = await _groupService.UserBelongToGroupAsync(userId, group.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UserBelongToGroupAsync_ShouldReturnFalse_WhenUserIsNotInGroup()
        {
            // Arrange
            var group = new Group { Id = Guid.NewGuid(), Name = "Test Group" };
            await _groupService.AddAsync(group);

            // Act
            var result = await _groupService.UserBelongToGroupAsync(Guid.NewGuid(), group.Id);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UserBelongToGroupAsync_ShouldReturnFalse_WhenGroupDoesNotExist()
        {
            // Act
            var result = await _groupService.UserBelongToGroupAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.False(result);
        }
        #endregion

        #region GetGroupDetail Tests
        [Fact]
        public async Task GetGroupDetailAsync_ShouldReturnGroupDetails_WhenGroupExists()
        {
            // Arrange

            // Add group and features
            var f1 = await _unitOfWork.Features.AddAsync(new Feature
            {
                FeatureName = "Feature 1",
                State = FeatureState.ENABLED,
                Description = "Test Feature 1"
            });
            var f2 = await _unitOfWork.Features.AddAsync(new Feature
            {
                FeatureName = "Feature 2",
                State = FeatureState.ENABLED,
                Description = "Test Feature 2"
            });
            await _unitOfWork.SaveChangesAsync();

            var group = new Group
            {
                Id = Guid.NewGuid(),
                Name = "Test Group",
                Users = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    Email = "test@example.com",
                    UserName = "testuser",
                    FullName = "Test User"
                }
            },
                GroupFeatures = new List<GroupFeature> { new GroupFeature { FeatureId = f1.Id }, new GroupFeature { FeatureId = f2.Id } }
            };
            await _groupService.AddAsync(group);

            // Act
            var result = await _groupService.GetGroupDetailAsync(group.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Group", result.Name);
            Assert.Equal(2, result.Features.Count);
            Assert.Contains(f1.Id, result.Features.Select(f => f.Id));
            Assert.Contains(f2.Id, result.Features.Select(f => f.Id));
        }

        [Fact]
        public async Task GetGroupDetailAsync_ShouldThrowKeyNotFoundException_WhenGroupDoesNotExist()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _groupService.GetGroupDetailAsync(Guid.NewGuid()));
        }
        #endregion
    }

}
