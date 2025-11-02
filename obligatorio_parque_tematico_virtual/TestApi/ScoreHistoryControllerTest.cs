using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Domain;
using IBusinessLogic;
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using Microsoft.Data.Sqlite;
using Models.Out;

namespace ApiTests;

[TestClass]
public class ScoreHistoryControllerTest
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _adminClient = null!;
    private HttpClient _visitorClient = null!;
    private Mock<IScoreHistoryLogic> _mockScoreHistoryLogic = null!;
    private SqliteConnection _connection = null!;
    private Guid _visitorUserId;

    [TestInitialize]
    public void Setup()
    {
        _mockScoreHistoryLogic = new Mock<IScoreHistoryLogic>();

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

                services.AddSingleton(_mockScoreHistoryLogic.Object);
            });
        });

        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();
        }

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

        _visitorUserId = Guid.NewGuid();
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
        _adminClient?.Dispose();
        _visitorClient?.Dispose();
        _factory?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }

    [TestMethod]
    public async Task GetMyScoreHistory_AsVisitor_ReturnsOk()
    {
        // Arrange
        var history = new List<ScoreHistoryModelOut>
        {
            new ScoreHistoryModelOut
            {
                Id = Guid.NewGuid(),
                VisitorId = _visitorUserId,
                Points = 100,
                Origin = "AttractionVisit",
                StrategyName = "PerAttraction",
                Description = "Visited Roller Coaster",
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockScoreHistoryLogic.Setup(l => l.GetMyScoreHistory(_visitorUserId)).Returns(history);

        // Act
        var response = await _visitorClient.GetAsync("/api/score-history/my-history");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetVisitorHistory_AsAdministrator_ReturnsOk()
    {
        // Arrange
        var visitorId = Guid.NewGuid();
        var history = new List<ScoreHistoryModelOut>
        {
            new ScoreHistoryModelOut
            {
                Id = Guid.NewGuid(),
                VisitorId = visitorId,
                Points = 50,
                Origin = "EventParticipation",
                StrategyName = "PerEvent",
                Description = "Participated in event",
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockScoreHistoryLogic.Setup(l => l.GetVisitorScoreHistory(visitorId, null, null)).Returns(history);

        // Act
        var response = await _adminClient.GetAsync($"/api/score-history/visitor/{visitorId}");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAllHistory_AsAdministrator_ReturnsOk()
    {
        // Arrange
        var history = new List<ScoreHistoryModelOut>
        {
            new ScoreHistoryModelOut
            {
                Id = Guid.NewGuid(),
                VisitorId = Guid.NewGuid(),
                VisitorName = "John Doe",
                Points = 100,
                Origin = "AttractionVisit",
                StrategyName = "PerAttraction",
                Description = "Test",
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockScoreHistoryLogic.Setup(l => l.GetAllScoreHistory()).Returns(history);

        // Act
        var response = await _adminClient.GetAsync("/api/score-history");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetMyScoreHistory_WithBusinessLogicException_ReturnsBadRequest()
    {
        // Arrange
        _mockScoreHistoryLogic.Setup(l => l.GetMyScoreHistory(_visitorUserId))
            .Throws(new ArgumentException("Invalid visitor"));

        // Act
        var response = await _visitorClient.GetAsync("/api/score-history/my-history");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task GetVisitorHistory_VisitorNotFound_ReturnsNotFound()
    {
        // Arrange
        var visitorId = Guid.NewGuid();
        _mockScoreHistoryLogic.Setup(l => l.GetVisitorScoreHistory(visitorId, null, null))
            .Throws(new KeyNotFoundException("Visitor not found"));

        // Act
        var response = await _adminClient.GetAsync($"/api/score-history/visitor/{visitorId}");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAllHistory_WithInvalidDateRange_ReturnsBadRequest()
    {
        // Arrange
        _mockScoreHistoryLogic.Setup(l => l.GetAllScoreHistory())
            .Throws(new ArgumentException("Invalid data"));

        // Act
        var response = await _adminClient.GetAsync("/api/score-history");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
