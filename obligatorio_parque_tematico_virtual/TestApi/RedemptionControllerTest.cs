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
public class RedemptionControllerTest
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private HttpClient _adminClient = null!;
    private HttpClient _visitorClient = null!;
    private Mock<IRedemptionLogic> _mockRedemptionLogic = null!;
    private SqliteConnection _connection = null!;
    private Guid _visitorUserId;

    [TestInitialize]
    public void Setup()
    {
        _mockRedemptionLogic = new Mock<IRedemptionLogic>();
        _visitorUserId = Guid.NewGuid();

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

                services.AddSingleton(_mockRedemptionLogic.Object);
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
            Id = _visitorUserId,
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
    public async Task RedeemReward_AsVisitor_ReturnsCreated()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        var redemption = new RedemptionHistoryModelOut
        {
            Id = Guid.NewGuid(),
            VisitorId = _visitorUserId,
            RewardId = rewardId,
            RedeemedAt = DateTime.Now,
            PointsSpent = 500
        };

        _mockRedemptionLogic.Setup(s => s.RedeemReward(_visitorUserId, rewardId)).Returns(redemption);

        var redeemRequest = new RedeemRewardModelIn
        {
            RewardId = rewardId
        };

        var json = JsonSerializer.Serialize(redeemRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _visitorClient.PostAsync("/api/redemptions/redeem", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }

    [TestMethod]
    public async Task RedeemReward_AsAdmin_ReturnsForbidden()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        var redeemRequest = new RedeemRewardModelIn
        {
            RewardId = rewardId
        };

        var json = JsonSerializer.Serialize(redeemRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _adminClient.PostAsync("/api/redemptions/redeem", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task RedeemReward_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        var redeemRequest = new RedeemRewardModelIn
        {
            RewardId = rewardId
        };

        var json = JsonSerializer.Serialize(redeemRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/redemptions/redeem", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task RedeemReward_InsufficientPoints_ReturnsBadRequest()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        _mockRedemptionLogic.Setup(s => s.RedeemReward(_visitorUserId, rewardId))
            .Throws(new InvalidOperationException("Insufficient points"));

        var redeemRequest = new RedeemRewardModelIn
        {
            RewardId = rewardId
        };

        var json = JsonSerializer.Serialize(redeemRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _visitorClient.PostAsync("/api/redemptions/redeem", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task RedeemReward_RewardNotFound_ReturnsNotFound()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        _mockRedemptionLogic.Setup(s => s.RedeemReward(_visitorUserId, rewardId))
            .Throws(new KeyNotFoundException("Reward not found"));

        var redeemRequest = new RedeemRewardModelIn
        {
            RewardId = rewardId
        };

        var json = JsonSerializer.Serialize(redeemRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _visitorClient.PostAsync("/api/redemptions/redeem", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task GetMyRedemptionHistory_AsVisitor_ReturnsOk()
    {
        // Arrange
        var history = new List<RedemptionHistoryModelOut>
        {
            new RedemptionHistoryModelOut
            {
                Id = Guid.NewGuid(),
                VisitorId = _visitorUserId,
                RewardId = Guid.NewGuid(),
                RedeemedAt = DateTime.Now.AddDays(-5),
                PointsSpent = 500
            },
            new RedemptionHistoryModelOut
            {
                Id = Guid.NewGuid(),
                VisitorId = _visitorUserId,
                RewardId = Guid.NewGuid(),
                RedeemedAt = DateTime.Now.AddDays(-2),
                PointsSpent = 300
            }
        };

        _mockRedemptionLogic.Setup(s => s.GetRedemptionHistory(_visitorUserId)).Returns(history);

        // Act
        var response = await _visitorClient.GetAsync("/api/redemptions/my-history");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("500"));
        Assert.IsTrue(content.Contains("300"));
    }

    [TestMethod]
    public async Task GetMyRedemptionHistory_Unauthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/redemptions/my-history");

        // Assert
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetVisitorRedemptionHistory_AsAdmin_ReturnsOk()
    {
        // Arrange
        var visitorId = Guid.NewGuid();
        var history = new List<RedemptionHistoryModelOut>
        {
            new RedemptionHistoryModelOut
            {
                Id = Guid.NewGuid(),
                VisitorId = visitorId,
                RewardId = Guid.NewGuid(),
                RedeemedAt = DateTime.Now,
                PointsSpent = 100
            }
        };

        _mockRedemptionLogic.Setup(s => s.GetRedemptionHistory(visitorId)).Returns(history);

        // Act
        var response = await _adminClient.GetAsync($"/api/redemptions/visitor/{visitorId}/history");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetVisitorRedemptionHistory_AsVisitor_ReturnsForbidden()
    {
        // Arrange
        var visitorId = Guid.NewGuid();

        // Act
        var response = await _visitorClient.GetAsync($"/api/redemptions/visitor/{visitorId}/history");

        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetMyRedemptionHistoryWithDateRange_AsVisitor_ReturnsOk()
    {
        // Arrange
        var dateFrom = DateTime.Now.AddDays(-7);
        var dateTo = DateTime.Now;
        var history = new List<RedemptionHistoryModelOut>
        {
            new RedemptionHistoryModelOut
            {
                Id = Guid.NewGuid(),
                VisitorId = _visitorUserId,
                RewardId = Guid.NewGuid(),
                RedeemedAt = DateTime.Now.AddDays(-3),
                PointsSpent = 200
            }
        };

        _mockRedemptionLogic.Setup(s => s.GetRedemptionHistoryWithDateRange(_visitorUserId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(history);

        // Act
        var response = await _visitorClient.GetAsync($"/api/redemptions/my-history?dateFrom={dateFrom:yyyy-MM-dd}&dateTo={dateTo:yyyy-MM-dd}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetVisitorRedemptionHistory_VisitorNotFound_ReturnsNotFound()
    {
        // Arrange
        var visitorId = Guid.NewGuid();
        _mockRedemptionLogic.Setup(s => s.GetRedemptionHistory(visitorId))
            .Throws(new KeyNotFoundException("Visitor not found"));

        // Act
        var response = await _adminClient.GetAsync($"/api/redemptions/visitor/{visitorId}/history");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task GetMyRedemptionHistoryWithDateRange_InvalidDateRange_ReturnsBadRequest()
    {
        // Arrange
        var dateFrom = DateTime.Now;
        var dateTo = DateTime.Now.AddDays(-7);
        _mockRedemptionLogic.Setup(s => s.GetRedemptionHistoryWithDateRange(_visitorUserId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Throws(new ArgumentException("Invalid date range"));

        // Act
        var response = await _visitorClient.GetAsync($"/api/redemptions/my-history?dateFrom={dateFrom:yyyy-MM-dd}&dateTo={dateTo:yyyy-MM-dd}");

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task RedeemReward_RewardNotAvailable_ReturnsBadRequest()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        _mockRedemptionLogic.Setup(s => s.RedeemReward(_visitorUserId, rewardId))
            .Throws(new ArgumentException("Reward is not available"));

        var redeemRequest = new RedeemRewardModelIn
        {
            RewardId = rewardId
        };

        var json = JsonSerializer.Serialize(redeemRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _visitorClient.PostAsync("/api/redemptions/redeem", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task GetVisitorRedemptionHistoryWithDateRange_InvalidDateRange_ReturnsBadRequest()
    {
        // Arrange
        var visitorId = Guid.NewGuid();
        var dateFrom = DateTime.Now;
        var dateTo = DateTime.Now.AddDays(-7);
        _mockRedemptionLogic.Setup(s => s.GetRedemptionHistoryWithDateRange(visitorId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Throws(new ArgumentException("Invalid date range"));

        // Act
        var response = await _adminClient.GetAsync($"/api/redemptions/visitor/{visitorId}/history?dateFrom={dateFrom:yyyy-MM-dd}&dateTo={dateTo:yyyy-MM-dd}");

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task GetMyRedemptionHistory_WithOnlyDateFrom_ReturnsAllHistory()
    {
        // Arrange
        var dateFrom = DateTime.Now.AddDays(-7);
        var expectedHistory = new List<RedemptionHistoryModelOut>();
        _mockRedemptionLogic.Setup(s => s.GetRedemptionHistory(It.IsAny<Guid>())).Returns(expectedHistory);

        // Act
        var response = await _visitorClient.GetAsync($"/api/redemptions/my-history?dateFrom={dateFrom:yyyy-MM-dd}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        _mockRedemptionLogic.Verify(s => s.GetRedemptionHistory(It.IsAny<Guid>()), Times.Once);
        _mockRedemptionLogic.Verify(s => s.GetRedemptionHistoryWithDateRange(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [TestMethod]
    public async Task GetMyRedemptionHistory_WithOnlyDateTo_ReturnsAllHistory()
    {
        // Arrange
        var dateTo = DateTime.Now;
        var expectedHistory = new List<RedemptionHistoryModelOut>();
        _mockRedemptionLogic.Setup(s => s.GetRedemptionHistory(It.IsAny<Guid>())).Returns(expectedHistory);

        // Act
        var response = await _visitorClient.GetAsync($"/api/redemptions/my-history?dateTo={dateTo:yyyy-MM-dd}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        _mockRedemptionLogic.Verify(s => s.GetRedemptionHistory(It.IsAny<Guid>()), Times.Once);
        _mockRedemptionLogic.Verify(s => s.GetRedemptionHistoryWithDateRange(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [TestMethod]
    public async Task GetVisitorRedemptionHistory_WithOnlyDateFrom_ReturnsAllHistory()
    {
        // Arrange
        var visitorId = Guid.NewGuid();
        var dateFrom = DateTime.Now.AddDays(-7);
        var expectedHistory = new List<RedemptionHistoryModelOut>();
        _mockRedemptionLogic.Setup(s => s.GetRedemptionHistory(visitorId)).Returns(expectedHistory);

        // Act
        var response = await _adminClient.GetAsync($"/api/redemptions/visitor/{visitorId}/history?dateFrom={dateFrom:yyyy-MM-dd}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        _mockRedemptionLogic.Verify(s => s.GetRedemptionHistory(visitorId), Times.Once);
        _mockRedemptionLogic.Verify(s => s.GetRedemptionHistoryWithDateRange(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [TestMethod]
    public async Task GetVisitorRedemptionHistory_WithOnlyDateTo_ReturnsAllHistory()
    {
        // Arrange
        var visitorId = Guid.NewGuid();
        var dateTo = DateTime.Now;
        var expectedHistory = new List<RedemptionHistoryModelOut>();
        _mockRedemptionLogic.Setup(s => s.GetRedemptionHistory(visitorId)).Returns(expectedHistory);

        // Act
        var response = await _adminClient.GetAsync($"/api/redemptions/visitor/{visitorId}/history?dateTo={dateTo:yyyy-MM-dd}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        _mockRedemptionLogic.Verify(s => s.GetRedemptionHistory(visitorId), Times.Once);
        _mockRedemptionLogic.Verify(s => s.GetRedemptionHistoryWithDateRange(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
    }

    [TestMethod]
    public async Task GetMyRedemptionHistory_WithMissingNameIdentifierClaim_ThrowsUnauthorized()
    {
        // Arrange - Create token without NameIdentifier claim
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var key = System.Text.Encoding.UTF8.GetBytes("MySecretKeyForJWTTokenGeneration1234567890");
        var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Visitor")
                // Missing NameIdentifier claim
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenString);

        // Act
        var response = await client.GetAsync("/api/redemptions/my-history");

        // Assert - Returns 401 when NameIdentifier claim is missing
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
