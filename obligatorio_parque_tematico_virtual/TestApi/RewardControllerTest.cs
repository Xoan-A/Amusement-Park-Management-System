using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Text;
using System.Text.Json;
using Domain;
using IBusinessLogic;
using Models.In;
using Models.Out;
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using Microsoft.Data.Sqlite;
using System.Net;

namespace ApiTests;

[TestClass]
public class RewardControllerTest
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private HttpClient _adminClient = null!;
    private HttpClient _visitorClient = null!;
    private Mock<IRewardLogic> _mockRewardLogic = null!;
    private SqliteConnection _connection = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRewardLogic = new Mock<IRewardLogic>();

        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ServiceDescriptor? descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite(_connection));

                services.AddSingleton(_mockRewardLogic.Object);
            });
        });

        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();
        }

        _client = _factory.CreateClient();

        Microsoft.Extensions.Options.IOptions<Models.JwtSettings> jwtSettings = Microsoft.Extensions.Options.Options.Create(new Models.JwtSettings
        {
            SecretKey = "MySecretKeyForJWTTokenGeneration1234567890",
            Issuer = "ParqueTematico",
            Audience = "ParqueTematico",
            ExpirationHours = 1
        });
        BusinessLogic.TokenLogic tokenService = new BusinessLogic.TokenLogic(jwtSettings);

        UserResponse adminUser = new UserResponse
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            LastName = "User",
            Email = "admin@example.com",
            UserRoles = new List<string> { Role.ADMINISTRATOR }
        };
        string adminToken = tokenService.GenerateToken(adminUser);
        _adminClient = _factory.CreateClient();
        _adminClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        UserResponse visitorUser = new UserResponse
        {
            Id = Guid.NewGuid(),
            Name = "Visitor",
            LastName = "User",
            Email = "visitor@example.com",
            UserRoles = new List<string> { Role.VISITOR }
        };
        string visitorToken = tokenService.GenerateToken(visitorUser);
        _visitorClient = _factory.CreateClient();
        _visitorClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", visitorToken);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client?.Dispose();
        _adminClient?.Dispose();
        _visitorClient?.Dispose();
        _factory?.Dispose();
        _connection?.Close();
    }

    [TestMethod]
    public async Task GetAllRewards_AsAdmin_ReturnsOk()
    {
        // Arrange
        var rewards = new List<RewardModelOut>
        {
            new RewardModelOut
            {
                Id = Guid.NewGuid(),
                Name = "Reward 1",
                Description = "Description 1",
                PointsCost = 100,
                AvailableQuantity = 10,
                IsAvailable = true
            },
            new RewardModelOut
            {
                Id = Guid.NewGuid(),
                Name = "Reward 2",
                Description = "Description 2",
                PointsCost = 200,
                AvailableQuantity = 5,
                IsAvailable = true
            }
        };

        _mockRewardLogic.Setup(s => s.GetAllRewards()).Returns(rewards);

        // Act
        var response = await _adminClient.GetAsync("/api/rewards");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("Reward 1"));
        Assert.IsTrue(content.Contains("Reward 2"));
    }

    [TestMethod]
    public async Task GetAllRewards_AsVisitor_ReturnsOk()
    {
        // Arrange
        var rewards = new List<RewardModelOut>
        {
            new RewardModelOut
            {
                Id = Guid.NewGuid(),
                Name = "Reward 1",
                Description = "Description 1",
                PointsCost = 100,
                AvailableQuantity = 10,
                IsAvailable = true
            }
        };

        _mockRewardLogic.Setup(s => s.GetAllRewards()).Returns(rewards);

        // Act
        var response = await _visitorClient.GetAsync("/api/rewards");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetRewardById_ExistingReward_ReturnsOk()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        var reward = new RewardModelOut
        {
            Id = rewardId,
            Name = "Test Reward",
            Description = "Test description",
            PointsCost = 300,
            AvailableQuantity = 7,
            IsAvailable = true
        };

        _mockRewardLogic.Setup(s => s.GetRewardById(rewardId)).Returns(reward);

        // Act
        var response = await _adminClient.GetAsync($"/api/rewards/{rewardId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("Test Reward"));
    }

    [TestMethod]
    public async Task GetRewardById_NonExistingReward_ReturnsNotFound()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        _mockRewardLogic.Setup(s => s.GetRewardById(rewardId))
            .Throws(new KeyNotFoundException("Reward not found"));

        // Act
        var response = await _adminClient.GetAsync($"/api/rewards/{rewardId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task CreateReward_AsAdmin_ReturnsCreated()
    {
        // Arrange
        var rewardModelIn = new RewardModelIn
        {
            Name = "New Reward",
            Description = "New reward description",
            PointsCost = 500,
            AvailableQuantity = 10,
            RequiredMembershipLevel = MembershipLevel.Premium
        };

        var createdReward = new RewardModelOut
        {
            Id = Guid.NewGuid(),
            Name = rewardModelIn.Name,
            Description = rewardModelIn.Description,
            PointsCost = rewardModelIn.PointsCost,
            AvailableQuantity = rewardModelIn.AvailableQuantity,
            RequiredMembershipLevel = rewardModelIn.RequiredMembershipLevel,
            IsAvailable = true
        };

        _mockRewardLogic.Setup(s => s.CreateReward(It.IsAny<RewardModelIn>())).Returns(createdReward);

        var json = JsonSerializer.Serialize(rewardModelIn);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _adminClient.PostAsync("/api/rewards", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        Assert.IsNotNull(response.Headers.Location);
    }

    [TestMethod]
    public async Task CreateReward_AsVisitor_ReturnsForbidden()
    {
        // Arrange
        var rewardModelIn = new RewardModelIn
        {
            Name = "New Reward",
            Description = "New reward description",
            PointsCost = 500,
            AvailableQuantity = 10
        };

        var json = JsonSerializer.Serialize(rewardModelIn);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _visitorClient.PostAsync("/api/rewards", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task CreateReward_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var rewardModelIn = new RewardModelIn
        {
            Name = "New Reward",
            Description = "New reward description",
            PointsCost = 500,
            AvailableQuantity = 10
        };

        var json = JsonSerializer.Serialize(rewardModelIn);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/rewards", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task CreateReward_DuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var rewardModelIn = new RewardModelIn
        {
            Name = "Duplicate Name",
            Description = "Description",
            PointsCost = 100,
            AvailableQuantity = 10
        };

        _mockRewardLogic.Setup(s => s.CreateReward(It.IsAny<RewardModelIn>()))
            .Throws(new ArgumentException("A reward with this name already exists"));

        var json = JsonSerializer.Serialize(rewardModelIn);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _adminClient.PostAsync("/api/rewards", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task UpdateReward_AsAdmin_ReturnsOk()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        var rewardModelIn = new RewardModelIn
        {
            Name = "Updated Reward",
            Description = "Updated description",
            PointsCost = 600,
            AvailableQuantity = 8,
            RequiredMembershipLevel = MembershipLevel.VIP
        };

        var updatedReward = new RewardModelOut
        {
            Id = rewardId,
            Name = rewardModelIn.Name,
            Description = rewardModelIn.Description,
            PointsCost = rewardModelIn.PointsCost,
            AvailableQuantity = rewardModelIn.AvailableQuantity,
            RequiredMembershipLevel = rewardModelIn.RequiredMembershipLevel,
            IsAvailable = true
        };

        _mockRewardLogic.Setup(s => s.UpdateReward(rewardId, It.IsAny<RewardModelIn>())).Returns(updatedReward);

        var json = JsonSerializer.Serialize(rewardModelIn);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _adminClient.PutAsync($"/api/rewards/{rewardId}", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task UpdateReward_AsVisitor_ReturnsForbidden()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        var rewardModelIn = new RewardModelIn
        {
            Name = "Updated Reward",
            Description = "Updated description",
            PointsCost = 600,
            AvailableQuantity = 8
        };

        var json = JsonSerializer.Serialize(rewardModelIn);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _visitorClient.PutAsync($"/api/rewards/{rewardId}", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task UpdateReward_NonExistingReward_ReturnsNotFound()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        var rewardModelIn = new RewardModelIn
        {
            Name = "Updated Reward",
            Description = "Updated description",
            PointsCost = 600,
            AvailableQuantity = 8
        };

        _mockRewardLogic.Setup(s => s.UpdateReward(rewardId, It.IsAny<RewardModelIn>()))
            .Throws(new KeyNotFoundException("Reward not found"));

        var json = JsonSerializer.Serialize(rewardModelIn);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _adminClient.PutAsync($"/api/rewards/{rewardId}", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteReward_AsAdmin_ReturnsNoContent()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        _mockRewardLogic.Setup(s => s.DeleteReward(rewardId));

        // Act
        var response = await _adminClient.DeleteAsync($"/api/rewards/{rewardId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteReward_AsVisitor_ReturnsForbidden()
    {
        // Arrange
        var rewardId = Guid.NewGuid();

        // Act
        var response = await _visitorClient.DeleteAsync($"/api/rewards/{rewardId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteReward_NonExistingReward_ReturnsNotFound()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        _mockRewardLogic.Setup(s => s.DeleteReward(rewardId))
            .Throws(new KeyNotFoundException("Reward not found"));

        // Act
        var response = await _adminClient.DeleteAsync($"/api/rewards/{rewardId}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAvailableRewards_ReturnsOnlyAvailable()
    {
        // Arrange
        var availableRewards = new List<RewardModelOut>
        {
            new RewardModelOut
            {
                Id = Guid.NewGuid(),
                Name = "Available 1",
                Description = "Has stock",
                PointsCost = 100,
                AvailableQuantity = 5,
                IsAvailable = true
            },
            new RewardModelOut
            {
                Id = Guid.NewGuid(),
                Name = "Available 2",
                Description = "Has stock",
                PointsCost = 200,
                AvailableQuantity = 3,
                IsAvailable = true
            }
        };

        _mockRewardLogic.Setup(s => s.GetAvailableRewards()).Returns(availableRewards);

        // Act
        var response = await _visitorClient.GetAsync("/api/rewards/available");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("Available 1"));
        Assert.IsTrue(content.Contains("Available 2"));
    }
}
