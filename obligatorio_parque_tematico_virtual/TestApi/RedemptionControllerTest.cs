using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Domain;
using IBusinessLogic;
using Models.Out;
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using Microsoft.Data.Sqlite;
using System.Net;
using Microsoft.IdentityModel.Tokens;

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

        Microsoft.Extensions.Options.IOptions<Models.JwtSettings> jwtSettings =
            Microsoft.Extensions.Options.Options.Create(new Models.JwtSettings
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
        Guid rewardId = Guid.NewGuid();
        RedemptionHistoryModelOut redemption = new RedemptionHistoryModelOut
        {
            Id = Guid.NewGuid(),
            VisitorId = _visitorUserId,
            RewardId = rewardId,
            RedeemedAt = DateTime.Now,
            PointsSpent = 500
        };

        _mockRedemptionLogic.Setup(s => s.RedeemReward(_visitorUserId, rewardId)).ReturnsAsync(redemption);

        HttpResponseMessage response = await _visitorClient.PostAsync($"/api/redemptions/redeem/{rewardId}", null);

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }

    [TestMethod]
    public async Task RedeemReward_AsAdmin_ReturnsForbidden()
    {
        Guid rewardId = Guid.NewGuid();

        HttpResponseMessage response = await _adminClient.PostAsync($"/api/redemptions/redeem/{rewardId}", null);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task RedeemReward_Unauthenticated_ReturnsUnauthorized()
    {
        Guid rewardId = Guid.NewGuid();

        HttpResponseMessage response = await _client.PostAsync($"/api/redemptions/redeem/{rewardId}", null);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task RedeemReward_InsufficientPoints_ReturnsBadRequest()
    {
        Guid rewardId = Guid.NewGuid();
        _mockRedemptionLogic.Setup(s => s.RedeemReward(_visitorUserId, rewardId))
            .ThrowsAsync(new InvalidOperationException("Insufficient points"));

        HttpResponseMessage response = await _visitorClient.PostAsync($"/api/redemptions/redeem/{rewardId}", null);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task RedeemReward_RewardNotFound_ReturnsNotFound()
    {
        Guid rewardId = Guid.NewGuid();
        _mockRedemptionLogic.Setup(s => s.RedeemReward(_visitorUserId, rewardId))
            .ThrowsAsync(new KeyNotFoundException("Reward not found"));

        HttpResponseMessage response = await _visitorClient.PostAsync($"/api/redemptions/redeem/{rewardId}", null);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task GetMyRedemptionHistory_AsVisitor_ReturnsOk()
    {
        List<RedemptionHistoryModelOut> history = new List<RedemptionHistoryModelOut>
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

        _mockRedemptionLogic.Setup(s => s.GetRedemptionHistory(_visitorUserId)).ReturnsAsync(history);

        HttpResponseMessage response = await _visitorClient.GetAsync("/api/redemptions/my-history");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("500"));
        Assert.IsTrue(content.Contains("300"));
    }

    [TestMethod]
    public async Task GetMyRedemptionHistory_Unauthenticated_ReturnsUnauthorized()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/redemptions/my-history");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetVisitorRedemptionHistory_AsAdmin_ReturnsOk()
    {
        Guid visitorId = Guid.NewGuid();
        List<RedemptionHistoryModelOut> history = new List<RedemptionHistoryModelOut>
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

        _mockRedemptionLogic.Setup(s => s.GetRedemptionHistory(visitorId)).ReturnsAsync(history);

        HttpResponseMessage response = await _adminClient.GetAsync($"/api/redemptions/visitor/{visitorId}/history");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetVisitorRedemptionHistory_AsVisitor_ReturnsForbidden()
    {
        Guid visitorId = Guid.NewGuid();

        HttpResponseMessage response = await _visitorClient.GetAsync($"/api/redemptions/visitor/{visitorId}/history");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetMyRedemptionHistoryWithDateRange_AsVisitor_ReturnsOk()
    {
        DateTime dateFrom = DateTime.Now.AddDays(-7);
        DateTime dateTo = DateTime.Now;
        List<RedemptionHistoryModelOut> history = new List<RedemptionHistoryModelOut>
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

        _mockRedemptionLogic.Setup(s =>
                s.GetRedemptionHistoryWithDateRange(_visitorUserId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(history);

        HttpResponseMessage response =
            await _visitorClient.GetAsync(
                $"/api/redemptions/my-history?dateFrom={dateFrom:yyyy-MM-dd}&dateTo={dateTo:yyyy-MM-dd}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetVisitorRedemptionHistory_VisitorNotFound_ReturnsNotFound()
    {
        Guid visitorId = Guid.NewGuid();
        _mockRedemptionLogic.Setup(s => s.GetRedemptionHistory(visitorId))
            .ThrowsAsync(new KeyNotFoundException("Visitor not found"));

        HttpResponseMessage response = await _adminClient.GetAsync($"/api/redemptions/visitor/{visitorId}/history");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task GetMyRedemptionHistoryWithDateRange_InvalidDateRange_ReturnsBadRequest()
    {
        DateTime dateFrom = DateTime.Now;
        DateTime dateTo = DateTime.Now.AddDays(-7);
        _mockRedemptionLogic.Setup(s =>
                s.GetRedemptionHistoryWithDateRange(_visitorUserId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ThrowsAsync(new ArgumentException("Invalid date range"));

        HttpResponseMessage response =
            await _visitorClient.GetAsync(
                $"/api/redemptions/my-history?dateFrom={dateFrom:yyyy-MM-dd}&dateTo={dateTo:yyyy-MM-dd}");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task RedeemReward_RewardNotAvailable_ReturnsBadRequest()
    {
        Guid rewardId = Guid.NewGuid();
        _mockRedemptionLogic.Setup(s => s.RedeemReward(_visitorUserId, rewardId))
            .Throws(new ArgumentException("Reward is not available"));

        HttpResponseMessage response = await _visitorClient.PostAsync($"/api/redemptions/redeem/{rewardId}", null);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task GetVisitorRedemptionHistoryWithDateRange_InvalidDateRange_ReturnsBadRequest()
    {
        Guid visitorId = Guid.NewGuid();
        DateTime dateFrom = DateTime.Now;
        DateTime dateTo = DateTime.Now.AddDays(-7);
        _mockRedemptionLogic.Setup(s =>
                s.GetRedemptionHistoryWithDateRange(visitorId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Throws(new ArgumentException("Invalid date range"));

        HttpResponseMessage response = await _adminClient.GetAsync(
            $"/api/redemptions/visitor/{visitorId}/history?dateFrom={dateFrom:yyyy-MM-dd}&dateTo={dateTo:yyyy-MM-dd}");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task GetMyRedemptionHistory_WithOnlyDateFrom_ReturnsAllHistory()
    {
        DateTime dateFrom = DateTime.Now.AddDays(-7);
        List<RedemptionHistoryModelOut> expectedHistory = new List<RedemptionHistoryModelOut>();
        _mockRedemptionLogic.Setup(s => s.GetRedemptionHistory(It.IsAny<Guid>())).ReturnsAsync(expectedHistory);

        HttpResponseMessage response =
            await _visitorClient.GetAsync($"/api/redemptions/my-history?dateFrom={dateFrom:yyyy-MM-dd}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        _mockRedemptionLogic.Verify(s => s.GetRedemptionHistory(It.IsAny<Guid>()), Times.Once);
        _mockRedemptionLogic.Verify(
            s => s.GetRedemptionHistoryWithDateRange(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()),
            Times.Never);
    }

    [TestMethod]
    public async Task GetMyRedemptionHistory_WithOnlyDateTo_ReturnsAllHistory()
    {
        DateTime dateTo = DateTime.Now;
        List<RedemptionHistoryModelOut> expectedHistory = new List<RedemptionHistoryModelOut>();
        _mockRedemptionLogic.Setup(s => s.GetRedemptionHistory(It.IsAny<Guid>())).ReturnsAsync(expectedHistory);

        HttpResponseMessage response =
            await _visitorClient.GetAsync($"/api/redemptions/my-history?dateTo={dateTo:yyyy-MM-dd}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        _mockRedemptionLogic.Verify(s => s.GetRedemptionHistory(It.IsAny<Guid>()), Times.Once);
        _mockRedemptionLogic.Verify(
            s => s.GetRedemptionHistoryWithDateRange(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()),
            Times.Never);
    }

    [TestMethod]
    public async Task GetVisitorRedemptionHistory_WithOnlyDateFrom_ReturnsAllHistory()
    {
        Guid visitorId = Guid.NewGuid();
        DateTime dateFrom = DateTime.Now.AddDays(-7);
        List<RedemptionHistoryModelOut> expectedHistory = new List<RedemptionHistoryModelOut>();
        _mockRedemptionLogic.Setup(s => s.GetRedemptionHistory(visitorId)).ReturnsAsync(expectedHistory);

        HttpResponseMessage response =
            await _adminClient.GetAsync($"/api/redemptions/visitor/{visitorId}/history?dateFrom={dateFrom:yyyy-MM-dd}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        _mockRedemptionLogic.Verify(s => s.GetRedemptionHistory(visitorId), Times.Once);
        _mockRedemptionLogic.Verify(
            s => s.GetRedemptionHistoryWithDateRange(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()),
            Times.Never);
    }

    [TestMethod]
    public async Task GetVisitorRedemptionHistory_WithOnlyDateTo_ReturnsAllHistory()
    {
        Guid visitorId = Guid.NewGuid();
        DateTime dateTo = DateTime.Now;
        List<RedemptionHistoryModelOut> expectedHistory = new List<RedemptionHistoryModelOut>();
        _mockRedemptionLogic.Setup(s => s.GetRedemptionHistory(visitorId)).ReturnsAsync(expectedHistory);

        HttpResponseMessage response =
            await _adminClient.GetAsync($"/api/redemptions/visitor/{visitorId}/history?dateTo={dateTo:yyyy-MM-dd}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        _mockRedemptionLogic.Verify(s => s.GetRedemptionHistory(visitorId), Times.Once);
        _mockRedemptionLogic.Verify(
            s => s.GetRedemptionHistoryWithDateRange(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()),
            Times.Never);
    }

    [TestMethod]
    public async Task GetMyRedemptionHistory_WithMissingNameIdentifierClaim_ThrowsUnauthorized()
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = System.Text.Encoding.UTF8.GetBytes("MySecretKeyForJWTTokenGeneration1234567890");
        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Visitor")
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        string tokenString = tokenHandler.WriteToken(token);

        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenString);

        HttpResponseMessage response = await client.GetAsync("/api/redemptions/my-history");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task RedeemReward_WithMissingNameIdentifierClaim_ThrowsUnauthorized()
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = System.Text.Encoding.UTF8.GetBytes("MySecretKeyForJWTTokenGeneration1234567890");
        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Visitor")
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        string tokenString = tokenHandler.WriteToken(token);

        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenString);

        Guid rewardId = Guid.NewGuid();
        HttpResponseMessage response = await client.PostAsync($"/api/redemptions/redeem/{rewardId}", null);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}