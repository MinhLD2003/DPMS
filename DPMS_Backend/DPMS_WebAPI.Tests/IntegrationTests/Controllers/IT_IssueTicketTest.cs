using AutoMapper;
using DPMS_WebAPI.Constants;
using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Tests.IntegrationTests.Helper;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.IssueTicket;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace DPMS_WebAPI.Tests.IntegrationTests.Controllers
{
    public class IT_IssueTicketTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly IMapper _mapper;
        private readonly IIssueTicketService _issueTicketService;
        private readonly IIssueTicketDocumentService _issueTicketDocumentService;

        public IT_IssueTicketTest(WebApplicationFactory<Program> factory)
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
                        options.UseInMemoryDatabase("IssueTicketTestingDb");
                    });
                    services.AddAuthentication(defaultScheme: "Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "Test",
                            options => { });

                    // Ensure authentication services are properly ordered
                    services.AddAuthorization(options =>
                    {
                        options.AddPolicy(Policies.FeatureRequired, policy =>
                            policy.RequireClaim("Feature", "IssueTicketManagement"));
                    });
                    // Build the service provider
                    var serviceProvider = services.BuildServiceProvider();

                    using (var scope = serviceProvider.CreateScope())
                    {
                        var scopedServices = scope.ServiceProvider;
                        var db = scopedServices.GetRequiredService<DPMSContext>();

                        db.Database.EnsureCreated();
                        db.IssueTickets.RemoveRange(db.IssueTickets);
                        db.IssueTicketDocuments.RemoveRange(db.IssueTicketDocuments);
                        db.SaveChanges();

                    }
                });
            });

            _client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            _mapper = serviceProvider.GetRequiredService<IMapper>();
            _issueTicketService = serviceProvider.GetRequiredService<IIssueTicketService>();
            _issueTicketDocumentService = serviceProvider.GetRequiredService<IIssueTicketDocumentService>();
        }

        #region CreateIssueTicket Tests

        [Fact]
        public async Task CreateIssueTicket_ShouldReturnCreatedStatus_WhenModelIsValid()
        {
            // Arrange
            var issueTicketVM = new IssueTicketVM
            {
                Title = "Integration Test Ticket",
                Description = "This is a test ticket created in integration test",
                TicketType = TicketType.DPIA,
                IssueTicketStatus = IssueTicketStatus.Pending
            };

            using var content = new MultipartFormDataContent();

            // Add each property individually to the form
            content.Add(new StringContent(issueTicketVM.Title), "Title");
            content.Add(new StringContent(issueTicketVM.Description), "Description");
            content.Add(new StringContent(issueTicketVM.TicketType.ToString()), "TicketType");
            content.Add(new StringContent(issueTicketVM.IssueTicketStatus.ToString()), "IssueTicketStatus");

            // Add mock file
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("test content"));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
            content.Add(fileContent, "files", "test.pdf");

            // Act
            var response = await _client.PostAsync("/api/IssueTicket", content);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            var returnValue = JsonConvert.DeserializeObject<IssueTicket>(responseContent);
            Assert.Equal(issueTicketVM.Title, returnValue.Title);
            Assert.Equal(issueTicketVM.Description, returnValue.Description);
            Assert.Equal(issueTicketVM.TicketType, returnValue.TicketType);
            Assert.Equal(issueTicketVM.IssueTicketStatus, returnValue.IssueTicketStatus);

            // Verify the ticket was saved in the database
            var getResponse = await _client.GetAsync($"/api/IssueTicket/{returnValue.Id}");
            getResponse.EnsureSuccessStatusCode();
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var savedTicket = JsonConvert.DeserializeObject<IssueTicketVM>(getContent);
            Assert.Equal(issueTicketVM.Title, savedTicket.Title);
        }
        [Fact]
        public async Task CreateIssueTicket_ShouldReturnBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            var invalidTicket = new IssueTicketVM
            {
                // Missing required Title
                Description = "This is an invalid ticket",
                TicketType = TicketType.DPIA
            };

            using var content = new MultipartFormDataContent();
            var ticketJson = JsonConvert.SerializeObject(invalidTicket);
            content.Add(new StringContent(ticketJson, Encoding.UTF8, "application/json"), "issueTicket");

            // Act
            var response = await _client.PostAsync("/api/IssueTicket", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region GetIssueTickets Tests

        [Fact]
        public async Task GetIssueTickets_ShouldReturnOkResult_WithPagedTickets()
        {
            // Arrange - Create test tickets
            await CreateTicketViaHttp("Test Ticket 1", "Description 1", TicketType.DPIA, IssueTicketStatus.Pending);
            await CreateTicketViaHttp("Test Ticket 2", "Description 2", TicketType.Risk, IssueTicketStatus.Accept);

            // Act
            var response = await _client.GetAsync("/api/IssueTicket?PageNumber=1&PageSize=10");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var returnValue = JsonConvert.DeserializeObject<PagedResponse<IssueTicketVM>>(content);
            Assert.Equal(2, returnValue.TotalRecords);
            Assert.Contains(returnValue.Data, t => t.Title == "Test Ticket 1");
            Assert.Contains(returnValue.Data, t => t.Title == "Test Ticket 2");
        }

        [Fact]
        public async Task GetIssueTickets_ShouldReturnFilteredResults_WhenStatusFilterApplied()
        {
            // Arrange
            await CreateTicketViaHttp("Pending Ticket", "This is pending", TicketType.DPIA, IssueTicketStatus.Pending);
            await CreateTicketViaHttp("Accepted Ticket", "This is accepted", TicketType.Risk, IssueTicketStatus.Accept);

            // Act
            var response = await _client.GetAsync("/api/IssueTicket?PageNumber=1&PageSize=10&Filters[IssueTicketStatus]=Pending");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var returnValue = JsonConvert.DeserializeObject<PagedResponse<IssueTicketVM>>(content);
            Assert.Single(returnValue.Data);
            Assert.Equal("Pending Ticket", returnValue.Data[0].Title);
            Assert.Equal(IssueTicketStatus.Pending, returnValue.Data[0].IssueTicketStatus);
        }

        #endregion

        #region GetIssueTicketById Tests

        [Fact]
        public async Task GetIssueTicketById_ShouldReturnOkResult_WithTicket_WhenIdExists()
        {
            // Arrange
            var ticketId = await CreateTicketViaHttp("Detail Test Ticket", "Test description", TicketType.DPIA, IssueTicketStatus.Pending);

            // Act
            var response = await _client.GetAsync($"/api/IssueTicket/{ticketId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var returnValue = JsonConvert.DeserializeObject<IssueTicketVM>(content);
            Assert.Equal(ticketId, returnValue.Id);
            Assert.Equal("Detail Test Ticket", returnValue.Title);
            Assert.Equal(TicketType.DPIA, returnValue.TicketType);
        }

        [Fact]
        public async Task GetIssueTicketById_ShouldReturnNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/IssueTicket/{nonExistentId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains($"Issue ticket with ID {nonExistentId} not found", content);
        }

        #endregion

        #region UpdateIssueTicket Tests

        [Fact]
        public async Task UpdateIssueTicket_ShouldReturnOkResult_WhenUpdateIsSuccessful()
        {
            // Arrange
            var ticketId = await CreateTicketViaHttp("Original Title", "Original description", TicketType.DPIA, IssueTicketStatus.Pending);

            var updateVM = new IssueTicketCreateVM
            {
                Title = "Updated Title",
                Description = "Updated description",
                TicketType = TicketType.Risk
            };

            using var content = new MultipartFormDataContent();

            // Add form fields correctly - similar to how you did in the CreateTicket tests
            content.Add(new StringContent(updateVM.Title), "Title");
            content.Add(new StringContent(updateVM.Description), "Description");
            content.Add(new StringContent(updateVM.TicketType.ToString()), "TicketType");

            // Add new file
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("updated content"));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
            content.Add(fileContent, "newFiles", "updated.pdf");

            // Add empty removedFiles list (or with values if needed)
            content.Add(new StringContent(""), "removedFiles");

            // Act
            var response = await _client.PutAsync($"/api/IssueTicket/{ticketId}", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var returnValue = JsonConvert.DeserializeObject<IssueTicket>(responseContent);
            Assert.Equal(ticketId, returnValue.Id);
            Assert.Equal("Updated Title", returnValue.Title);
            Assert.Equal("Updated description", returnValue.Description);
            Assert.Equal(TicketType.Risk, returnValue.TicketType);

            // Verify the ticket was updated in the database
            var getResponse = await _client.GetAsync($"/api/IssueTicket/{ticketId}");
            getResponse.EnsureSuccessStatusCode();
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var updatedTicket = JsonConvert.DeserializeObject<IssueTicketVM>(getContent);
            Assert.Equal("Updated Title", updatedTicket.Title);
        }
        [Fact]
        public async Task UpdateIssueTicket_ShouldReturnNotFound_WhenTicketDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent("Updated Title"), "Title");
            content.Add(new StringContent("Updated description"), "Description");
            content.Add(new StringContent(TicketType.Risk.ToString()), "TicketType");

            // Add empty newFiles and removedFiles to satisfy the API
            content.Add(new StringContent(""), "removedFiles");

            // Act
            var response = await _client.PutAsync($"/api/IssueTicket/{nonExistentId}", content);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains($"Issue ticket with ID {nonExistentId} not found", responseContent);
        }

        //[Fact]
        //public async Task UpdateIssueTicket_ShouldReturnBadRequest_WhenModelIsInvalid()
        //{
        //    // Arrange
        //    var ticketId = await CreateTicketViaHttp("Original Title", "Original description", TicketType.DPIA, IssueTicketStatus.Pending);

        //    // Create a MultipartFormDataContent with missing required fields
        //    using var content = new MultipartFormDataContent();

        //    // Deliberately omit Title (if it's required)
        //    content.Add(new StringContent("Updated description"), "Description");
        //    content.Add(new StringContent(TicketType.Risk.ToString()), "TicketType");

        //    // Add empty newFiles and removedFiles to satisfy the API
        //    content.Add(new StringContent(""), "removedFiles");

        //    // Act
        //    var response = await _client.PutAsync($"/api/IssueTicket/{ticketId}", content);

        //    // Assert
        //    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        //}

        #endregion

        #region UpdateIssueTicketStatus Tests

        [Fact]
        public async Task UpdateIssueTicketStatus_ShouldReturnOkResult_WhenStatusIsUpdated()
        {
            // Arrange
            var ticketId = await CreateTicketViaHttp("Status Test Ticket", "Test description", TicketType.DPIA, IssueTicketStatus.Pending);
            var newStatus = IssueTicketStatus.Accept;

            // Create proper JSON content for the status
            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(newStatus),
                Encoding.UTF8,
                "application/json"
            );

            // Act
            var response = await _client.PutAsync(
                $"/api/IssueTicket/{ticketId}/update-status",
                jsonContent
            );

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var returnValue = JsonConvert.DeserializeObject<IssueTicket>(content);
            Assert.Equal(ticketId, returnValue.Id);
            Assert.Equal(IssueTicketStatus.Accept, returnValue.IssueTicketStatus);

            // Verify the status was updated in the database
            var getResponse = await _client.GetAsync($"/api/IssueTicket/{ticketId}");
            getResponse.EnsureSuccessStatusCode();
            var getContent = await getResponse.Content.ReadAsStringAsync();
            var updatedTicket = JsonConvert.DeserializeObject<IssueTicketVM>(getContent);
            Assert.Equal(IssueTicketStatus.Accept, updatedTicket.IssueTicketStatus);
        }

        [Fact]
        public async Task UpdateIssueTicketStatus_ShouldReturnNotFound_WhenTicketDoesNotExist()
        {
            // Arrange
            var nonExistentId = "9a3b7c8d-1e2f-4a5b-6c7d-8e9f0a1b2c3d";
            var newStatus = IssueTicketStatus.Done;

            // Create proper JSON content for the status
            var jsonContent = new StringContent(
                 System.Text.Json.JsonSerializer.Serialize(newStatus),
                Encoding.UTF8,
                "application/json"
            );

            // Act
            var response = await _client.PutAsync(
                $"/api/IssueTicket/{nonExistentId}/update-status",
                jsonContent
            );

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains($"Issue ticket with ID {nonExistentId} not found", content);
        }

        #endregion

        #region DeleteIssueTicket Tests

        [Fact]
        public async Task DeleteIssueTicket_ShouldReturnNoContent_WhenDeletionIsSuccessful()
        {
            // Arrange
            var ticketId = await CreateTicketViaHttp("Ticket to Delete", "This will be deleted", TicketType.System, IssueTicketStatus.Pending);

            // Act
            var response = await _client.DeleteAsync($"/api/IssueTicket/{ticketId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify the ticket was deleted from the database
            var getResponse = await _client.GetAsync($"/api/IssueTicket/{ticketId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteIssueTicket_ShouldReturnNotFound_WhenTicketDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/IssueTicket/{nonExistentId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains($"Issue ticket with ID {nonExistentId} not found", content);
        }

        #endregion

        #region Helper Methods

        private async Task<Guid> CreateTicketViaHttp(string title, string description, TicketType ticketType, IssueTicketStatus status)
        {
            var issueTicketVM = new IssueTicketVM
            {
                Title = title,
                Description = description,
                TicketType = ticketType,
                IssueTicketStatus = status
            };

            using var content = new MultipartFormDataContent();

            // Add each property individually to the form
            content.Add(new StringContent(issueTicketVM.Title), "Title");
            content.Add(new StringContent(issueTicketVM.Description), "Description");
            content.Add(new StringContent(issueTicketVM.TicketType.ToString()), "TicketType");
            content.Add(new StringContent(issueTicketVM.IssueTicketStatus.ToString()), "IssueTicketStatus");

            // Add a mock file for completeness
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("test content"));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
            content.Add(fileContent, "files", "test.pdf");

            var response = await _client.PostAsync("/api/IssueTicket", content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var ticket = JsonConvert.DeserializeObject<IssueTicket>(responseContent);
            return ticket.Id;
        }
        #endregion
    }
}
