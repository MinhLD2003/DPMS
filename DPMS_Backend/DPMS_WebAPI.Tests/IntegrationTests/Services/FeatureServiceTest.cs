using AutoMapper;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.MapperProfiles;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Repositories;
using DPMS_WebAPI.Services;
using DPMS_WebAPI.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace DPMS_WebAPI.Tests.IntegrationTests.Services
{
    public class FeatureServiceTest : TestEnvironment
    {
        private readonly FeatureService _featureService;

        public FeatureServiceTest(ITestOutputHelper output)
        {
            // _featureService = new FeatureService(_context, _mapper);
        }

        // [Fact]
        // public async Task CreateFeatureAsync_ShouldReturnFeatureVM_WhenFeatureIsCreated()
        // {
        //     // Arrange
        //     var featureVM = new CreateFeatureVM { FeatureName = "Test Feature" };

        //     // Act
        //     var result = await _featureService.CreateFeatureAsync(featureVM);

        //     // Assert
        //     Assert.NotNull(result);
        //     Assert.Equal("Test Feature", result.FeatureName);
        // }


    }
}