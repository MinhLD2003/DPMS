using AutoMapper;
using DPMS_WebAPI.Controllers;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.Feature;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;


namespace DPMS_WebAPI.Tests.Controllers
{
    public class FeatureControllerTests
    {
        private readonly Mock<IFeatureService> _mockFeatureService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<FeatureController>> _mockLogger;
        private readonly FeatureController _controller;

        public FeatureControllerTests()
        {
            _mockFeatureService = new Mock<IFeatureService>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<FeatureController>>();

            _controller = new FeatureController(
                _mockFeatureService.Object,
                _mockMapper.Object,
                _mockLogger.Object
            );

            // Setup default HttpContext for controller
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        #region GetListNestedFeatures Tests

        [Fact]
        public async Task GetListNestedFeatures_ReturnsOk_WhenFeaturesRetrievedSuccessfully()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var features = new List<FeatureVM> { new FeatureVM { Id = Guid.NewGuid(), FeatureName = "Test Feature" } };
            _mockFeatureService.Setup(s => s.GetListNestedFeatures(groupId)).ReturnsAsync(features);

            // Act
            var result = await _controller.GetListNestedFeatures(groupId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(features, okResult.Value);
        }

        [Fact]
        public async Task GetListNestedFeatures_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            _mockFeatureService.Setup(s => s.GetListNestedFeatures(groupId)).ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetListNestedFeatures(groupId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Test exception", badRequestResult.Value);
        }

        #endregion

        #region GetFeatures Tests

        [Fact]
        public async Task GetFeatures_ReturnsOk_WhenFeaturesRetrievedSuccessfully()
        {
            // Arrange
            var queryParams = new QueryParams();
            var features = new PagedResponse<Feature> { Data = new List<Feature> { new Feature() { FeatureName = "f1" } } };
            _mockFeatureService.Setup(s => s.GetPagedAsync(queryParams)).ReturnsAsync(features);

            // Act
            var result = await _controller.GetFeatures(queryParams);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(features, okResult.Value);
        }

        [Fact]
        public async Task GetFeatures_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var queryParams = new QueryParams();
            _mockFeatureService.Setup(s => s.GetPagedAsync(queryParams)).ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetFeatures(queryParams);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Test exception", badRequestResult.Value);
        }

        #endregion

        #region GetFeature Tests

        [Fact]
        public async Task GetFeature_ReturnsOk_WhenFeatureRetrievedSuccessfully()
        {
            // Arrange
            var featureId = Guid.NewGuid();
            var feature = new Feature() { FeatureName = "f1" };
            _mockFeatureService.Setup(s => s.GetDetailAsync(featureId, x => x.Children)).ReturnsAsync(feature);

            // Act
            var result = await _controller.GetFeature(featureId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(feature, okResult.Value);
        }

        [Fact]
        public async Task GetFeature_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var featureId = Guid.NewGuid();
            _mockFeatureService.Setup(s => s.GetDetailAsync(featureId, x => x.Children)).ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetFeature(featureId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Test exception", badRequestResult.Value);
        }

        #endregion

        #region CreateFeature Tests

        [Fact]
        public async Task CreateFeature_ReturnsOk_WhenFeatureCreatedSuccessfully()
        {
            // Arrange
            var featureVM = new CreateFeatureVM { FeatureName = "New Feature" };
            var feature = new Feature { FeatureName = "New Feature" };
            _mockMapper.Setup(m => m.Map<Feature>(featureVM)).Returns(feature);

            // Act
            var result = await _controller.CreateFeature(featureVM);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            _mockFeatureService.Verify(s => s.AddAsync(feature), Times.Once);
        }

        [Fact]
        public async Task CreateFeature_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var featureVM = new CreateFeatureVM { FeatureName = "New Feature" };
            var feature = new Feature { FeatureName = "New Feature" };
            _mockMapper.Setup(m => m.Map<Feature>(featureVM)).Returns(feature);
            _mockFeatureService.Setup(s => s.AddAsync(feature)).ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.CreateFeature(featureVM);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Test exception", badRequestResult.Value);
        }

        #endregion

        #region UpdateFeature Tests

        [Fact]
        public async Task UpdateFeature_ReturnsOk_WhenFeatureUpdatedSuccessfully()
        {
            // Arrange
            var featureId = Guid.NewGuid();
            var featureVM = new FeatureVM
            {
                Id = featureId,
                FeatureName = "Updated Feature Name",
                Description = "Updated description for the feature",
                Url = "/updated-feature-url",
                ParentId = null,
                HttpMethod = HttpMethodType.PUT
            };

            var feature = new Feature
            {
                Id = featureId,
                FeatureName = "Updated Feature Name",
                Description = "Updated description for the feature",
                Url = "/updated-feature-url",
                ParentId = null,
                HttpMethod = HttpMethodType.PUT
            };

            _mockMapper.Setup(m => m.Map<Feature>(featureVM)).Returns(feature);

            // Act
            var result = await _controller.UpdateFeature(featureId, featureVM);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            _mockFeatureService.Verify(s => s.UpdateAsync(feature), Times.Once);
        }


        [Fact]
        public async Task UpdateFeature_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var featureId = Guid.NewGuid();
            var featureVM = new FeatureVM { Id = featureId, FeatureName = "Updated Feature" };
            var feature = new Feature { Id = featureId, FeatureName = "Updated Feature" };
            _mockMapper.Setup(m => m.Map<Feature>(featureVM)).Returns(feature);
            _mockFeatureService.Setup(s => s.UpdateAsync(feature)).ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.UpdateFeature(featureId, featureVM);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Test exception", badRequestResult.Value);
        }

        #endregion

        #region AddFeatureToGroup Tests

        [Fact]
        public void AddFeatureToGroup_ReturnsOk_WhenFeaturesAddedSuccessfully()
        {
            // Arrange
            var model = new FeatureGroupAssignmentModel
            {
                FeatureIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
                GroupId = Guid.NewGuid()
            };
            _mockFeatureService.Setup(s => s.AddFeaturesToGroup(model.FeatureIds, model.GroupId)).Returns(true); // Simulate successful add

            // Act
            var result = _controller.AddFeatureToGroup(model);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public void AddFeatureToGroup_ReturnsBadRequest_WhenExceptionOccurs()
        {
            // Arrange
            var model = new FeatureGroupAssignmentModel
            {
                FeatureIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
                GroupId = Guid.NewGuid()
            };
            _mockFeatureService.Setup(s => s.AddFeaturesToGroup(model.FeatureIds, model.GroupId)).Throws(new Exception("Test exception"));

            // Act
            var result = _controller.AddFeatureToGroup(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal("Test exception", badRequestResult.Value);
            //_mockLogger.Verify(
            //    x => x.LogError(
            //        It.IsAny<Exception>(),
            //        It.IsAny<string>(),
            //        "Test exception"),
            //    Times.Once);
        }

        #endregion
    }
}