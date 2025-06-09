using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace DPMS_WebAPI.Tests.IntegrationTests.Services
{
    public class AuthServiceTests 
    {
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"Jwt:Key", "A_very_secret_key_that_i_never_used"},
                {"Jwt:Issuer", "test_issuer"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            
            var userRepository = new Mock<IUserRepository>();
            

            _authService = new AuthService(configuration, userRepository.Object);

        }

        // [Fact]
        // public void GenerateJwtToken_ShouldReturn_ValidToken()
        // {
        //     // Arrange
        //     var email = "test@example.com";
        //     var name = "Test User";

        //     // Act
        //     var token = _authService.GenerateJwtToken(email, name);

        //     // Assert
        //     Assert.NotNull(token);
        //     Assert.NotEmpty(token);
        // }
    }
}
        