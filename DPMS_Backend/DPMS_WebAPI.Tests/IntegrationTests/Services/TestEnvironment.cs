using AutoMapper;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.MapperProfiles;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Repositories;
using DPMS_WebAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;

namespace DPMS_WebAPI.Tests.IntegrationTests
{
    public class TestEnvironment
    {
        protected readonly DPMSContext _context;
        protected readonly IMapper _mapper;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public TestEnvironment()
        {
            var services = new ServiceCollection();

            // Register required services
            services.AddScoped<AuthService>();
            services.AddDbContext<DPMSContext>(options =>
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

            services.AddAutoMapper(typeof(AutoMapperProfiles));
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // Mock IHttpContextAccessor to simulate an authenticated user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                new Claim(ClaimTypes.Email, "test@example.com")
            };
            
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var mockHttpContext = new DefaultHttpContext { User = claimsPrincipal };

            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(mockHttpContext);
            services.AddSingleton(mockHttpContextAccessor.Object);
            _httpContextAccessor = mockHttpContextAccessor.Object;

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            // Register User repository
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IdentityService>();
            services.AddScoped<IUserService, UserService>();
            

            _serviceProvider = services.BuildServiceProvider();
            _context = _serviceProvider.GetRequiredService<DPMSContext>();
            _mapper = _serviceProvider.GetRequiredService<IMapper>();
        }

        // public ServiceProvider ServiceProvider { get; private set; }
    }
}