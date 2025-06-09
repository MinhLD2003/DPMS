using DPMS_WebAPI.Enums;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Services;
using DPMS_WebAPI.Utils;
using DPMS_WebAPI.ViewModels.User;
using FluentAssertions;
using Moq;

namespace DPMS_WebAPI.Tests.IntegrationTests.Services
{
    public class UserServiceTest : ServiceTestEnvironment
    {
        private readonly UserService _sut; // system under test
        private readonly UserService _userService;

        private readonly Mock<IUserRepository> _userRepositoryMock;

        public UserServiceTest()
        {
            _sut = new UserService(_unitOfWork, _userRepository, _mapper);
            _userRepositoryMock = new Mock<IUserRepository>();
            _userService = new UserService(_unitOfWork, _userRepositoryMock.Object, _mapper);
        }

        #region CheckUserExist
        [Fact]
        public async Task CheckUserExist_ShouldReturnsTrue_WhenEmailExists()
        {
            // Arrange
            User user = new User
            {
                Email = "haquangthangvnn@gmail",
                UserName = "ThangHQ",
                FullName = "Ha Quang Thang"
            };
            await _sut.AddAsync(user);

            // Act
            bool userExists = await _sut.CheckUserExist(user.Email);

            // Assert
            Assert.True(userExists);
        }

        [Fact]
        public async Task CheckUserExist_ShouldReturnsFalse_WhenEmailNotExistsOrNull()
        {
            // Arrange
            User user = new User
            {
                Email = "haquangthangvnn@gmail",
                UserName = "ThangHQ",
                FullName = "Ha Quang Thang"
            };
            await _sut.AddAsync(user);

            // Act
            bool result1 = await _sut.CheckUserExist(null);
            bool result2 = await _sut.CheckUserExist("not exists email");

            // Assert
            Assert.False(result1);
            Assert.False(result2);
        }
        #endregion

        #region GetUserByEmailAsync
        [Theory]
        [InlineData("abc@gmail.com")]
        [InlineData("thang@gmail.com")]
        public async Task GetUserByEmailAsync_ShouldReturnUser_WhenEmailExists(string email)
        {
            // arrange
            User testData = new User { Email = email, UserName = "TEST", FullName = "TEST" };
            await _context.Users.AddAsync(testData);
            await _context.SaveChangesAsync();

            // act
            User? result = await _sut.GetUserByEmailAsync(email);

            // assert
            Assert.NotNull(result);
            Assert.Equal(result, testData);
        }

        [Theory]
        [InlineData("abc@gmail.com")]
        [InlineData("thang@gmail.com")]
        [InlineData("")]
        [InlineData(null)]
        public async Task GetUserByEmailAsync_ShouldReturnNull_WhenEmailNotExists(string email)
        {
            // arrange
            User testData = new User { Email = "not exist", UserName = "TEST", FullName = "TEST" };
            await _context.Users.AddAsync(testData);
            await _context.SaveChangesAsync();

            // act
            User? result = await _sut.GetUserByEmailAsync(email);

            // assert
            Assert.Null(result);
            Assert.NotEqual(result, testData);
        }
        #endregion

        #region ChangePassword
        [Fact]
        public async Task ChangePassword_ShouldChangePassword_WhenOldPasswordIsCorrect()
        {
            // Arrange
            var email = "test@example.com";
            var salt = "zEPs7tOhpajqv28jPmSs6McxNeBfTV65ZWj91BFTOnef/uGdhXOeI/UwSNuyhxNQ9mrDDR5hLsRSdZLUpUuUeR75gocaaerUxOEpApudRo7K87t3Xhb1ApT56xDXQ4g+Y/AdOmr88vSEmKmUvVQ//bC5M6a29i+OAmWNBHC8PZY=";
            var oldPassword = "Old123!";
            var newPassword = "New456!";
            var hashedOldPassword = PasswordUtils.HashPassword(oldPassword, salt);
            var hashedNewPassword = PasswordUtils.HashPassword(newPassword, salt);

            var user = new User
            {
                Email = email,
                Salt = salt,
                Password = hashedOldPassword,
                UserName = "TEST",
                FullName = "TEST"
            };

            // Use _context directly instead of _sut.AddAsync to ensure user is properly saved
            await _sut.AddAsync(user);

            var model = new ChangePasswordVM
            {
                OldPassword = oldPassword,
                NewPassword = newPassword
            };

            // Act
            await _sut.ChangePassword(email, model);

            // Assert
            // Refresh user from database to verify the password was updated
            var updatedUser = await _context.Users.FindAsync(user.Id);
            Assert.NotNull(updatedUser);
            Assert.Equal(hashedNewPassword, updatedUser.Password);
        }

        [Fact]
        public async Task ChangePassword_ShouldThrow_WhenUserDoesNotExist()
        {
            // Arrange
            var email = "test@example.com";
            var salt = "zEPs7tOhpajqv28jPmSs6McxNeBfTV65ZWj91BFTOnef/uGdhXOeI/UwSNuyhxNQ9mrDDR5hLsRSdZLUpUuUeR75gocaaerUxOEpApudRo7K87t3Xhb1ApT56xDXQ4g+Y/AdOmr88vSEmKmUvVQ//bC5M6a29i+OAmWNBHC8PZY=";
            var oldPassword = "Old123!";
            var newPassword = "New456!";
            var hashedOldPassword = PasswordUtils.HashPassword(oldPassword, salt);
            var hashedNewPassword = PasswordUtils.HashPassword(newPassword, salt);

            var user = new User
            {
                Email = "notfound@gmail.com",
                Salt = salt,
                Password = hashedOldPassword,
                UserName = "TEST",
                FullName = "TEST"
            };

            // add an account with another email to db
            await _sut.AddAsync(user);

            var model = new ChangePasswordVM
            {
                OldPassword = oldPassword,
                NewPassword = newPassword
            };

            // Act: email and model's email does not match & Assert
            Exception ex = await Assert.ThrowsAsync<Exception>(() => _sut.ChangePassword(email, model));
            Assert.Equal($"Account {email} does not exist", ex.Message);
        }

        [Fact]
        public async Task ChangePassword_ShouldThrow_WhenOldPasswordIsIncorrect()
        {
            // Arrange
            var email = "test@example.com";
            var salt = "zEPs7tOhpajqv28jPmSs6McxNeBfTV65ZWj91BFTOnef/uGdhXOeI/UwSNuyhxNQ9mrDDR5hLsRSdZLUpUuUeR75gocaaerUxOEpApudRo7K87t3Xhb1ApT56xDXQ4g+Y/AdOmr88vSEmKmUvVQ//bC5M6a29i+OAmWNBHC8PZY=";

            var user = new User
            {
                Email = email,
                Salt = salt,
                Password = PasswordUtils.HashPassword("CorrectOld", salt),
                UserName = "TEST",
                FullName = "TEST"
            };

            var model = new ChangePasswordVM
            {
                OldPassword = "WrongOld",
                NewPassword = "NewPassword"
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.ChangePassword(email, model));
            Assert.Equal("Old password is incorrect", ex.Message);
        }
        #endregion

        #region GetUserProfileAsync
        [Fact]
        public async Task GetUserProfileAsync_ReturnsFail_WhenUserDoesNotExist()
        {
            var result = await _sut.GetUserProfileAsync(Guid.NewGuid());

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle().Which.Message.Should().Be("User does not exist");
        }

        [Fact]
        public async Task GetUserProfileAsync_ReturnsUser_WhenUserExists()
        {
            // arrange
            var userId = Guid.NewGuid();

            var system = new ExternalSystem { Id = Guid.NewGuid(), Name = "DPMS" };
            var group = new Group { Id = Guid.NewGuid(), Name = "admin_group", System = system, IsGlobal = true };
            var creator = new User { Id = Guid.NewGuid(), Email = "creator@example.com", UserName = "TEST_CreatedBy", FullName = "TEST" };
            var modifier = new User { Id = Guid.NewGuid(), Email = "modifier@example.com", UserName = "TEST_ModifiedBy", FullName = "TEST" };

            var user = new User
            {
                Id = userId,
                Email = "user@example.com",
                Groups = new[] { group },
                CreatedBy = creator,
                LastModifiedBy = modifier,
                UserName = "TEST",
                FullName = "TEST"
            };

            await _sut.AddAsync(user);

            // act
            var result = await _sut.GetUserProfileAsync(userId);

            result.IsSuccess.Should().BeTrue();
            result.Value.Email.Should().Be("user@example.com");
            result.Value.Groups.Should().HaveCount(1);
            result.Value.Groups.First().Should().Be("admin_group");
            result.Value.CreatedBy.Should().Be("TEST_CreatedBy");
            result.Value.LastModifiedBy.Should().Be("TEST_ModifiedBy");
        }
        #endregion

        #region UpdateUserStatus
        [Fact]
        public async Task UpdateUserStatus_ShouldThrowException_WhenUserNotFound()
        {
            // Arrange
            var model = new UpdateUserStatusVM { Id = Guid.NewGuid(), Status = UserStatus.Activated };

            // Act & Assert: I did not insert user --> GetByIdAsync would return null because user does not exist
            Func<Task> act = async () => await _sut.UpdateUserStatus(model);
            await act.Should().ThrowAsync<Exception>().WithMessage("User not found");
        }

        [Fact]
        public async Task UpdateUserStatus_ShouldUpdateUserStatus_WhenUserFound()
        {
            // Arrange
            var model = new UpdateUserStatusVM { Id = Guid.NewGuid(), Status = UserStatus.Deactivated };
            var user = new User { Id = model.Id, Status = UserStatus.Deactivated, FullName = "TEST", UserName = "TEST", Email = "test@gmail.com" };

            await _sut.AddAsync(user);

            // Act
            await _sut.UpdateUserStatus(model);

            // Assert
            user.Status.Should().Be(model.Status); // Ensure the status was updated
        }
        #endregion

        #region UpdateUserPassword
        [Fact]
        public async Task UpdateUserPassword_ShouldReturnNull_WhenUserNotFound()
        {
            // Arrange
            string email = "test@example.com";
            string password = "newPassword123";
            string salt = "someSalt";

            // Mock the repository method to return null when user is not found
            _userRepositoryMock.Setup(repo => repo.UpdateUserPassword(password, salt, email))
                               .ReturnsAsync((User)null);

            // Act
            var result = await _userService.UpdateUserPassword(password, salt, email);

            // Assert
            result.Should().BeNull();  // If user is not found, the result should be null.
        }

        [Fact]
        public async Task UpdateUserPassword_ShouldUpdatePassword_WhenUserFound()
        {
            // Arrange
            string email = "test@example.com";
            string password = "newPassword123";
            string salt = "someSalt";
            var user = new User { Email = email, Password = password, Salt = salt, LastModifiedAt = DateTime.UtcNow, UserName = "TEST", FullName = "TEST" };

            // Mock the repository to return the user object when fetching by email
            _userRepositoryMock.Setup(repo => repo.UpdateUserPassword(password, salt, email))
                               .ReturnsAsync(user);

            // Act
            var result = await _userService.UpdateUserPassword(password, salt, email);

            // Assert
            result.Should().NotBeNull();
            result.Password.Should().Be(password);  // Check if the password was updated
            result.Salt.Should().Be(salt);  // Check if the salt was updated
            result.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));  // Check if the LastModifiedAt is updated
        }
        #endregion
    }
}