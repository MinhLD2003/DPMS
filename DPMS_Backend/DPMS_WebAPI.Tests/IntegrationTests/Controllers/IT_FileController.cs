using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;


namespace DPMS_WebAPI.Tests.IntegrationTests.Controllers
{
    public class IT_FileControllerTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly IFileRepository _fileRepository;

        public IT_FileControllerTest(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<DPMSContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database
                    services.AddDbContext<DPMSContext>(options =>
                    {
                        options.UseInMemoryDatabase("FileControllerTestingDb");
                    });

                    // Build the service provider
                    var serviceProvider = services.BuildServiceProvider();

                    using (var scope = serviceProvider.CreateScope())
                    {
                        var scopedServices = scope.ServiceProvider;
                        var db = scopedServices.GetRequiredService<DPMSContext>();
                        db.Database.EnsureCreated();

                        // Clear any file-related data if needed
                        // db.SaveChanges();
                    }
                });
            });

            _client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            _fileRepository = serviceProvider.GetRequiredService<IFileRepository>();
        }

        #region UploadFile Tests

        [Fact]
        public async Task UploadFile_ShouldReturnOkResult_WhenFileIsValid()
        {
            // Arrange
            var fileName = "testfile.txt";
            var fileContents = "This is test file content";

            // Create multipart form data content
            using var content = new MultipartFormDataContent();
            var fileContentBytes = Encoding.UTF8.GetBytes(fileContents);
            var fileStreamContent = new ByteArrayContent(fileContentBytes);
            fileStreamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
            content.Add(fileStreamContent, "file", fileName);

            // Act
            var response = await _client.PostAsync("/api/File", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var resultJson = await response.Content.ReadAsStringAsync();
            var resultObj = JObject.Parse(resultJson);

            Assert.True(resultObj.ContainsKey("fileUrl"));
            var fileUrl = resultObj["fileUrl"].ToString();
            Assert.NotEmpty(fileUrl);

            // Verify file can be retrieved
            var uploadedFileName = Path.GetFileName(fileUrl);
            var getResponse = await _client.GetAsync($"/api/File?fileName={uploadedFileName}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        }

        [Fact]
        public async Task UploadFile_ShouldReturnBadRequest_WhenFileIsEmpty()
        {
            // Arrange
            var fileName = "emptyfile.txt";
            var fileContents = "";

            // Create multipart form data content
            using var content = new MultipartFormDataContent();
            var fileContentBytes = Encoding.UTF8.GetBytes(fileContents);
            var fileStreamContent = new ByteArrayContent(fileContentBytes);
            fileStreamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
            content.Add(fileStreamContent, "file", fileName);

            // Act
            var response = await _client.PostAsync("/api/File", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var resultContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("File is empty", resultContent);
        }

        [Fact]
        public async Task UploadFile_ShouldReturnBadRequest_WhenNoFileIsProvided()
        {
            // Arrange - Send empty form content
            using var content = new MultipartFormDataContent();

            // Act
            var response = await _client.PostAsync("/api/File", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region DeleteFile Tests

        [Fact]
        public async Task DeleteFile_ShouldReturnOkResult_WhenFileExists()
        {
            // Arrange - First upload a file
            var fileName = "filetodelete.txt";
            var fileContents = "This file will be deleted";

            // Create multipart form data content for upload
            using var uploadContent = new MultipartFormDataContent();
            var fileContentBytes = Encoding.UTF8.GetBytes(fileContents);
            var fileStreamContent = new ByteArrayContent(fileContentBytes);
            fileStreamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
            uploadContent.Add(fileStreamContent, "file", fileName);

            // Upload file
            var uploadResponse = await _client.PostAsync("/api/File", uploadContent);
            uploadResponse.EnsureSuccessStatusCode();
            var uploadJson = await uploadResponse.Content.ReadAsStringAsync();
            var uploadObj = JObject.Parse(uploadJson);
            var fileUrl = uploadObj["fileUrl"].ToString();

            // Act - Delete the file
            var deleteResponse = await _client.DeleteAsync($"/api/File?fileUrl={fileUrl}");

            // Assert
            deleteResponse.EnsureSuccessStatusCode();
            var deleteJson = await deleteResponse.Content.ReadAsStringAsync();
            var deleteObj = JObject.Parse(deleteJson);
            Assert.True((bool)deleteObj["result"]);

            // Verify file is deleted
            var getResponse = await _client.GetAsync($"/api/File?fileName={Path.GetFileName(fileUrl)}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteFile_ShouldReturnBadRequest_WhenFileUrlIsEmpty()
        {
            // Arrange
            string fileUrl = "";

            // Act
            var response = await _client.DeleteAsync($"/api/File?fileUrl={fileUrl}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var resultContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("File URL is empty", resultContent);
        }

        #endregion

        #region GetFile Tests

        [Fact]
        public async Task GetFile_ShouldReturnFileContent_WhenFileExists()
        {
            // Arrange - First upload a file
            var fileName = "testgetfile.txt";
            var fileContents = "This is content for get file test";

            // Create multipart form data content for upload
            using var uploadContent = new MultipartFormDataContent();
            var fileContentBytes = Encoding.UTF8.GetBytes(fileContents);
            var fileStreamContent = new ByteArrayContent(fileContentBytes);
            fileStreamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
            uploadContent.Add(fileStreamContent, "file", fileName);

            // Upload file
            var uploadResponse = await _client.PostAsync("/api/File", uploadContent);
            uploadResponse.EnsureSuccessStatusCode();
            var uploadJson = await uploadResponse.Content.ReadAsStringAsync();
            var uploadObj = JObject.Parse(uploadJson);
            var fileUrl = uploadObj["fileUrl"].ToString();
            var uploadedFileName = Path.GetFileName(fileUrl);

            // Act
            var getResponse = await _client.GetAsync($"/api/File?fileName={uploadedFileName}");

            // Assert
            getResponse.EnsureSuccessStatusCode();
            Assert.Equal("application/octet-stream", getResponse.Content.Headers.ContentType.ToString());
            var downloadedContent = await getResponse.Content.ReadAsStringAsync();
            Assert.Equal(fileContents, downloadedContent);
        }

        [Fact]
        public async Task GetFile_ShouldReturnNotFound_WhenFileDoesNotExist()
        {
            // Arrange
            var nonExistentFileName = "nonexistent.txt";

            // Act
            var response = await _client.GetAsync($"/api/File?fileName={nonExistentFileName}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var resultContent = await response.Content.ReadAsStringAsync();
            var resultObj = JObject.Parse(resultContent);
            Assert.Equal("File not found", resultObj["message"].ToString());
        }

        #endregion
    }
}
