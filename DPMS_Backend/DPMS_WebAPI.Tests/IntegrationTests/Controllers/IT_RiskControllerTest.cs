using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.ViewModels;
using DPMS_WebAPI.ViewModels.Risk;

using DPMS_WebAPI.Tests.IntegrationTests.Helper;
using Microsoft.AspNetCore.Authentication;
using DPMS_WebAPI.Constants;

namespace DPMS_WebAPI.Tests.IntegrationTests.Controllers
{
    public class IT_RiskControllerTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly List<Guid> _createdRiskIds = new List<Guid>();
        private readonly string _dbName;

        public IT_RiskControllerTest(WebApplicationFactory<Program> factory)
        {
            _dbName = $"RiskTestingDb_{Guid.NewGuid()}";

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing"); // Ensure we're using the testing environment

                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<DPMSContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database with a consistent name for this test instance
                    services.AddDbContext<DPMSContext>(options =>
                    {
                        options.UseInMemoryDatabase(_dbName);
                    });

                    // Configure test authentication
                    services.AddAuthentication(defaultScheme: "Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "Test",
                            options => { });

                    // Ensure authentication services are properly ordered
                    services.AddAuthorization(options =>
                    {
                        options.AddPolicy(Policies.FeatureRequired, policy =>
                            policy.RequireClaim("Feature", "RiskManagement"));
                    });

                    // Build the service provider to ensure our services are properly registered
                    var sp = services.BuildServiceProvider();

                    // Create a scope to obtain a reference to the database context
                    using (var scope = sp.CreateScope())
                    {
                        var scopedServices = scope.ServiceProvider;
                        var db = scopedServices.GetRequiredService<DPMSContext>();

                        // Ensure the database is created and seed it
                        db.Database.EnsureCreated();
                        SeedDatabase(db);
                    }
                });
            });

            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }
        private void SeedDatabase(DPMSContext context)
        {
            try
            {
                // Clear existing risks
                if (context.Risks != null && context.Risks.Any())
                {
                    context.Risks.RemoveRange(context.Risks);
                    context.SaveChanges();
                }

                // Create test user for CreatedBy
                var testUser = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = "testuser@example.com",
                    FullName = "Test User",
                    Email = "testuser@example.com",
                };

                if (!context.Users.Any(u => u.UserName == testUser.UserName))
                {
                    context.Users.Add(testUser);
                    context.SaveChanges();
                }
                else
                {
                    testUser = context.Users.First(u => u.UserName == testUser.UserName);
                }

                // Add test risks
                var risks = new List<Risk>
                {
                    new Risk
                    {
                        Id = Guid.NewGuid(),
                        RiskName = "Test Risk 1",
                        Mitigation = "Implement controls",
                        RiskContingency = "Backup plan for risk 1",
                        Category = RiskCategory.Technical,
                        Strategy = ResponseStrategy.Mitigate,
                        RiskImpact = 4,
                        RiskProbability = 3,
                        Priority = 12,
                        RiskImpactAfterMitigation = 2,
                        RiskProbabilityAfterMitigation = 1,
                        PriorityAfterMitigation = 2,
                        RiskOwner = "John Doe",
                        RaisedAt = DateTime.UtcNow.AddDays(-5),
                        CreatedById = testUser.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    },
                    new Risk
                    {
                        Id = Guid.NewGuid(),
                        RiskName = "Test Risk 2",
                        Mitigation = "Staff training",
                        RiskContingency = "Alternative approach for risk 2",
                        Category = RiskCategory.Organizational,
                        Strategy = ResponseStrategy.Prevent,
                        RiskImpact = 3,
                        RiskProbability = 4,
                        Priority = 12,
                        RiskImpactAfterMitigation = 2,
                        RiskProbabilityAfterMitigation = 2,
                        PriorityAfterMitigation = 4,
                        RiskOwner = "Jane Smith",
                        RaisedAt = DateTime.UtcNow.AddDays(-3),
                        CreatedById = testUser.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-3)
                    }
                };

                foreach (var risk in risks)
                {
                    context.Risks.Add(risk);
                }

                context.SaveChanges();

                // Store the created IDs for later use in tests
                _createdRiskIds.Clear();
                _createdRiskIds.AddRange(risks.Select(r => r.Id));

                // Debug check - verify the risks were actually saved
                var count = context.Risks.Count();
                Console.WriteLine($"Seeded {count} risks to the database");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding database: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task GetRisks_ReturnsSuccessAndCorrectData()
        {
            // Arrange - Ensure database is seeded
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DPMSContext>();
                if (!db.Risks.Any())
                {
                    SeedDatabase(db);
                }
            }

            // Act
            var response = await _client.GetAsync("/api/Risk");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var contentType = response.Content.Headers.ContentType?.ToString();
            Assert.NotNull(contentType);
            Assert.Contains("application/json", contentType);

            var result = JsonConvert.DeserializeObject<PagedResponse<RiskListVM>>(responseContent);

            Assert.NotNull(result);
            Assert.True(result.TotalRecords > 0, $"Expected records but got {result.TotalRecords}");
            Assert.Contains(result.Data, g => g.RiskName == "Test Risk 1");
            Assert.Contains(result.Data, g => g.RiskName == "Test Risk 2");
        }

        [Fact]
        public async Task GetRisks_WithPagination_ReturnsCorrectPage()
        {
            // Arrange - Ensure database is seeded
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DPMSContext>();
                if (!db.Risks.Any())
                {
                    SeedDatabase(db);
                }
            }

            // Act
            var response = await _client.GetAsync("/api/Risk?pageNumber=1&pageSize=1");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<PagedResponse<RiskListVM>>(content);

            Assert.NotNull(result);
            Assert.True(result.TotalRecords > 0, "Expected at least one record");
            Assert.Single(result.Data); // Only 1 item per page
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(1, result.PageSize);
        }

        [Fact]
        public async Task GetRisks_WithFilter_ReturnsFilteredResults()
        {
            // Arrange - Ensure database is seeded
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DPMSContext>();
                if (!db.Risks.Any())
                {
                    SeedDatabase(db);
                }

                // Ensure we have at least one Technical risk
                if (!db.Risks.Any(r => r.Category == RiskCategory.Technical))
                {
                    var testUser = db.Users.FirstOrDefault();
                    if (testUser == null)
                    {
                        testUser = new User
                        {
                            Id = Guid.NewGuid(),
                            UserName = "testuser@example.com",
                            FullName = "Test User",
                            Email = "testuser@example.com",
                        };
                        db.Users.Add(testUser);
                        db.SaveChanges();
                    }

                    var technicalRisk = new Risk
                    {
                        Id = Guid.NewGuid(),
                        RiskName = "Technical Test Risk",
                        Category = RiskCategory.Technical,
                        Strategy = ResponseStrategy.Mitigate,
                        RiskImpact = 3,
                        RiskProbability = 3,
                        Priority = 9,
                        RiskOwner = "Technical Owner", // Added RiskOwner
                        Mitigation = "Technical mitigation", // Added Mitigation
                        RiskContingency = "Technical contingency", // Added Contingency
                        CreatedById = testUser.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    db.Risks.Add(technicalRisk);
                    db.SaveChanges();
                    _createdRiskIds.Add(technicalRisk.Id);
                }
            }

            // Act - Filter by Category=Technical
            var response = await _client.GetAsync("/api/Risk?Category=0"); // 0 = Technical

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<PagedResponse<RiskListVM>>(content);

            Assert.NotNull(result);
            // Check if we have at least one result
            Assert.True(result.TotalRecords > 0, "Expected at least one result with Category=Technical");

            // Check if all returned results have the correct category
            foreach (var risk in result.Data)
            {
                Assert.Equal(RiskCategory.Technical, risk.Category);
            }
        }

        [Fact]
        public async Task GetRiskById_ReturnsCorrectRisk()
        {
            // Arrange - Create a risk to get
            var newRisk = new RiskVM  // Changed to CreateRiskVM
            {
                RiskName = "Risk for GetById Test",
                Mitigation = "Test mitigation",
                RiskContingency = "Test contingency", // Added required field
                Category = RiskCategory.Technical,
                Strategy = ResponseStrategy.Mitigate,
                RiskImpact = 3,
                RiskProbability = 3,
                Priority = 9,
                RiskOwner = "Test Owner",
                RaisedAt = DateTime.UtcNow
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(newRisk),
                Encoding.UTF8,
                "application/json");

            var createResponse = await _client.PostAsync("/api/Risk", jsonContent);

            if (!createResponse.IsSuccessStatusCode)
            {
                var errorContent = await createResponse.Content.ReadAsStringAsync();
                Assert.True(false, $"Failed to create test risk. Status: {createResponse.StatusCode}. Response: {errorContent}");
            }

            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createdRisk = JsonConvert.DeserializeObject<Risk>(createContent);
            Assert.NotNull(createdRisk);
            var riskId = createdRisk.Id;

            // Act
            var response = await _client.GetAsync($"/api/Risk/{riskId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RiskVM>(content);

            Assert.NotNull(result);
            Assert.Equal(riskId, result.Id);
            Assert.Equal("Risk for GetById Test", result.RiskName);

            // Clean up
            _createdRiskIds.Add(riskId);
        }

        [Fact]
        public async Task GetRiskById_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync($"/api/Risk/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateRisk_AddsRiskAndReturnsCreated()
        {
            // Arrange
            var newRisk = new RiskVM  // Changed to CreateRiskVM
            {
                RiskName = "New Test Risk",
                Mitigation = "Document procedures",
                RiskContingency = "Fallback plan for new risk",
                Category = RiskCategory.Communication,
                Strategy = ResponseStrategy.Transfer,
                RiskImpact = 2,
                RiskProbability = 2,
                Priority = 4,
                RiskImpactAfterMitigation = 1,
                RiskProbabilityAfterMitigation = 1,
                PriorityAfterMitigation = 1,
                RiskOwner = "Sam Wilson",
                RaisedAt = DateTime.UtcNow
            };

            // Act
            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(newRisk),
                Encoding.UTF8,
                "application/json");

            var response = await _client.PostAsync("/api/Risk", jsonContent);

            // Debug output if failing
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Create response failed: {response.StatusCode}");
                Console.WriteLine($"Error content: {errorContent}");
                Assert.True(false, $"Failed to create risk. Status: {response.StatusCode}. Response: {errorContent}");
            }

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // Check that the risk was created
            var responseContent = await response.Content.ReadAsStringAsync();
            var createdRisk = JsonConvert.DeserializeObject<Risk>(responseContent);
            Assert.NotNull(createdRisk);
            Assert.Equal("New Test Risk", createdRisk.RiskName);

            // Verify that the Location header points to the new resource
            Assert.NotNull(response.Headers.Location);

            // Verify by getting the risk
            var getResponse = await _client.GetAsync($"/api/Risk/{createdRisk.Id}");
            getResponse.EnsureSuccessStatusCode();

            // Cleanup - add to created IDs for potential deletion in other tests
            _createdRiskIds.Add(createdRisk.Id);
        }

        [Fact]
        public async Task UpdateRisk_ModifiesRiskAndReturnsSuccess()
        {
            // Arrange - Create a risk to update
            var newRisk = new RiskVM  // Changed to use CreateRiskVM instead of RiskVM
            {
                RiskName = "Risk to Update",
                Mitigation = "Initial mitigation",
                RiskContingency = "Initial contingency",
                Category = RiskCategory.Technical,
                Strategy = ResponseStrategy.Mitigate,
                RiskImpact = 3,
                RiskProbability = 3,
                Priority = 9,
                RiskImpactAfterMitigation = 2,
                RiskProbabilityAfterMitigation = 2,
                PriorityAfterMitigation = 4,
                RiskOwner = "Initial Owner"
            };

            var createContent = new StringContent(
                JsonConvert.SerializeObject(newRisk),
                Encoding.UTF8,
                "application/json");

            var createResponse = await _client.PostAsync("/api/Risk", createContent);

            if (!createResponse.IsSuccessStatusCode)
            {
                var errorContent = await createResponse.Content.ReadAsStringAsync();
                Assert.True(false, $"Failed to create test risk. Status: {createResponse.StatusCode}. Response: {errorContent}");
            }

            var createdRiskContent = await createResponse.Content.ReadAsStringAsync();
            var createdRisk = JsonConvert.DeserializeObject<Risk>(createdRiskContent);
            Assert.NotNull(createdRisk);
            var riskId = createdRisk.Id;

            // Get the risk to update
            var getResponse = await _client.GetAsync($"/api/Risk/{riskId}");
            getResponse.EnsureSuccessStatusCode();

            var getRiskContent = await getResponse.Content.ReadAsStringAsync();
            var riskToUpdate = JsonConvert.DeserializeObject<RiskVM>(getRiskContent);

            // Modify the risk
            riskToUpdate.RiskName = "Updated Risk Name";
            riskToUpdate.RiskImpact = 5;
            riskToUpdate.RiskOwner = "Updated Owner";

            // Set valid values for after mitigation fields - these are required by the validator
            riskToUpdate.RiskImpactAfterMitigation = 2;
            riskToUpdate.RiskProbabilityAfterMitigation = 1;
            riskToUpdate.PriorityAfterMitigation = 2;

            // Ensure we're not missing any required fields
            if (string.IsNullOrEmpty(riskToUpdate.RiskContingency))
            {
                riskToUpdate.RiskContingency = "Updated contingency";
            }
            if (string.IsNullOrEmpty(riskToUpdate.Mitigation))
            {
                riskToUpdate.Mitigation = "Updated mitigation";
            }

            // Act
            var updateContent = new StringContent(
                JsonConvert.SerializeObject(riskToUpdate),
                Encoding.UTF8,
                "application/json");

            var updateResponse = await _client.PutAsync($"/api/Risk/{riskId}", updateContent);

            // Debug output if failing
            if (!updateResponse.IsSuccessStatusCode)
            {
                var errorContent = await updateResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Update failed: {updateResponse.StatusCode}");
                Console.WriteLine($"Update error: {errorContent}");

                // Add more detailed error info
                var parsedError = JsonConvert.DeserializeObject<Dictionary<string, object>>(errorContent);
                if (parsedError != null && parsedError.ContainsKey("errors"))
                {
                    var errors = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(parsedError["errors"].ToString());
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Field {error.Key}: {string.Join(", ", error.Value)}");
                    }
                }

                Assert.True(false, $"Failed to update risk. Status: {updateResponse.StatusCode}. Response: {errorContent}");
            }

            // Assert
            updateResponse.EnsureSuccessStatusCode();

            // Verify the update
            var verifyResponse = await _client.GetAsync($"/api/Risk/{riskId}");
            verifyResponse.EnsureSuccessStatusCode();

            var verifyContent = await verifyResponse.Content.ReadAsStringAsync();
            var updatedRisk = JsonConvert.DeserializeObject<RiskVM>(verifyContent);

            Assert.Equal("Updated Risk Name", updatedRisk.RiskName);
            Assert.Equal(5, updatedRisk.RiskImpact);
            Assert.Equal("Updated Owner", updatedRisk.RiskOwner);
            // Also verify the mitigation values were preserved
            Assert.Equal(2, updatedRisk.RiskImpactAfterMitigation);
            Assert.Equal(1, updatedRisk.RiskProbabilityAfterMitigation);
            Assert.Equal(2, updatedRisk.PriorityAfterMitigation);

            // Cleanup
            _createdRiskIds.Add(riskId);
        }


        [Fact]
        public async Task ResolveRisk_UpdatesMitigationValues()
        {
            // Arrange - Create a risk to resolve
            var newRisk = new RiskVM  // Changed to CreateRiskVM
            {
                RiskName = "Risk to Resolve",
                Mitigation = "Initial mitigation",
                RiskContingency = "Initial contingency", // Added required field
                Category = RiskCategory.Technical,
                Strategy = ResponseStrategy.Mitigate,
                RiskImpact = 4,
                RiskProbability = 4,
                Priority = 16,
                RiskOwner = "Resolve Test Owner"
            };

            var createContent = new StringContent(
                JsonConvert.SerializeObject(newRisk),
                Encoding.UTF8,
                "application/json");

            var createResponse = await _client.PostAsync("/api/Risk", createContent);

            if (!createResponse.IsSuccessStatusCode)
            {
                var errorContent = await createResponse.Content.ReadAsStringAsync();
                Assert.True(false, $"Failed to create test risk. Status: {createResponse.StatusCode}. Response: {errorContent}");
            }

            var createdRiskContent = await createResponse.Content.ReadAsStringAsync();
            var createdRisk = JsonConvert.DeserializeObject<Risk>(createdRiskContent);
            Assert.NotNull(createdRisk);
            var riskId = createdRisk.Id;

            var resolveModel = new RiskResolveVM
            {
                RiskImpactAfterMitigation = 1,
                RiskProbabilityAfterMitigation = 1,
                PriorityAfterMitigation = 1
            };

            // Act
            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(resolveModel),
                Encoding.UTF8,
                "application/json");

            var response = await _client.PutAsync($"/api/Risk/resolve-risk/{riskId}", jsonContent);

            // Debug output if failing
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Resolve response failed: {response.StatusCode}");
                Console.WriteLine($"Resolve error: {errorContent}");
                Assert.True(false, $"Failed to resolve risk. Status: {response.StatusCode}. Response: {errorContent}");
            }

            // Assert
            response.EnsureSuccessStatusCode();

            // Verify the update
            var verifyResponse = await _client.GetAsync($"/api/Risk/{riskId}");
            verifyResponse.EnsureSuccessStatusCode();

            var verifyContent = await verifyResponse.Content.ReadAsStringAsync();
            var updatedRisk = JsonConvert.DeserializeObject<RiskVM>(verifyContent);

            Assert.Equal(1, updatedRisk.RiskImpactAfterMitigation);
            Assert.Equal(1, updatedRisk.RiskProbabilityAfterMitigation);
            Assert.Equal(1, updatedRisk.PriorityAfterMitigation);

            // Cleanup
            _createdRiskIds.Add(riskId);
        }

        [Fact]
        public async Task DeleteRisk_RemovesRiskAndReturnsNoContent()
        {
            // Arrange - Create a risk to delete
            var newRisk = new RiskVM  // Changed to CreateRiskVM
            {
                RiskName = "Risk to Delete",
                Mitigation = "This risk will be deleted",
                RiskContingency = "This contingency will be deleted", // Added required field
                Category = RiskCategory.Schedule,
                Strategy = ResponseStrategy.Acceptance,
                RiskImpact = 3,
                RiskProbability = 3,
                Priority = 9,
                RiskOwner = "Delete Tester"
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(newRisk),
                Encoding.UTF8,
                "application/json");

            var createResponse = await _client.PostAsync("/api/Risk", jsonContent);

            if (!createResponse.IsSuccessStatusCode)
            {
                var errorContent = await createResponse.Content.ReadAsStringAsync();
                Assert.True(false, $"Failed to create test risk. Status: {createResponse.StatusCode}. Response: {errorContent}");
            }

            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createdRisk = JsonConvert.DeserializeObject<Risk>(createContent);
            Assert.NotNull(createdRisk);
            var riskId = createdRisk.Id;

            // Verify the risk exists
            var verifyCreateResponse = await _client.GetAsync($"/api/Risk/{riskId}");
            verifyCreateResponse.EnsureSuccessStatusCode();

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/Risk/{riskId}");

            // Debug output if failing
            if (deleteResponse.StatusCode != HttpStatusCode.NoContent)
            {
                var errorContent = await deleteResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Delete response failed: {deleteResponse.StatusCode}");
                Console.WriteLine($"Delete error: {errorContent}");
                Assert.True(false, $"Failed to delete risk. Status: {deleteResponse.StatusCode}. Response: {errorContent}");
            }

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // Verify the deletion
            var verifyDeleteResponse = await _client.GetAsync($"/api/Risk/{riskId}");
            Assert.Equal(HttpStatusCode.NotFound, verifyDeleteResponse.StatusCode);
        }

        [Fact]
        public async Task ExportRisks_ReturnsFile()
        {
            // Arrange - Ensure we have risks to export
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DPMSContext>();
                if (!db.Risks.Any())
                {
                    SeedDatabase(db);
                }
            }

            // Act
            var response = await _client.GetAsync("/api/Risk/export");

            // Debug output if failing
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Export response failed: {response.StatusCode}");
                Console.WriteLine($"Export error: {errorContent}");
                Assert.True(false, $"Failed to export risks. Status: {response.StatusCode}. Response: {errorContent}");
            }

            // Assert
            response.EnsureSuccessStatusCode();

            Assert.Equal("application/octet-stream", response.Content.Headers.ContentType?.ToString());

            // Check that we got some content
            var content = await response.Content.ReadAsByteArrayAsync();
            Assert.NotEmpty(content);

            // Check that the filename is in Content-Disposition header
            Assert.NotNull(response.Content.Headers.ContentDisposition);
            var contentDisposition = response.Content.Headers.ContentDisposition.ToString();
            Assert.Contains("Risk_Export_", contentDisposition);
            Assert.Contains(".xlsx", contentDisposition);
        }

        [Fact]
        public async Task CreateRisk_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var invalidRisk = new RiskVM  // Changed to CreateRiskVM
            {
                // Missing required fields: RiskName, RiskOwner, etc.
                Category = RiskCategory.Communication,
                Strategy = ResponseStrategy.Transfer
            };

            // Act
            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(invalidRisk),
                Encoding.UTF8,
                "application/json");

            var response = await _client.PostAsync("/api/Risk", jsonContent);

            // Debug
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Invalid model response: {response.StatusCode}");
            Console.WriteLine($"Invalid model content: {responseContent}");

            // Assert - Either BadRequest or Unprocessable Entity are acceptable
            Assert.True(
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.UnprocessableEntity,
                $"Expected BadRequest or UnprocessableEntity but got {response.StatusCode}"
            );
        }

        [Fact]
        public async Task UpdateRisk_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var riskUpdate = new RiskVM
            {
                Id = nonExistentId,
                RiskName = "Non-existent Risk",
                Mitigation = "This update should fail",
                RiskContingency = "This update should fail",
                Category = RiskCategory.Communication,
                Strategy = ResponseStrategy.Transfer,
                RiskImpact = 3,
                RiskProbability = 3,
                Priority = 9,
                RiskOwner = "Non-existent Owner"
            };

            // Act
            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(riskUpdate),
                Encoding.UTF8,
                "application/json");

            var response = await _client.PutAsync($"/api/Risk/{nonExistentId}", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ResolveRisk_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var resolveModel = new RiskResolveVM
            {
                RiskImpactAfterMitigation = 1,
                RiskProbabilityAfterMitigation = 1,
                PriorityAfterMitigation = 1
            };

            // Act
            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(resolveModel),
                Encoding.UTF8,
                "application/json");

            var response = await _client.PutAsync($"/api/Risk/resolve-risk/{nonExistentId}", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
