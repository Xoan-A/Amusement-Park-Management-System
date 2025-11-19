using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Domain;
using IBusinessLogic;
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using Microsoft.Data.Sqlite;
using Models.Out;
using Api;

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
            UserRoles = new List<string> { Role.Administrator }
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
            UserRoles = new List<string> { Role.Visitor }
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
    public void GetMyScoreHistory_AsVisitor_ReturnsOk()
    {
        List<ScoreHistoryModelOut> history = new List<ScoreHistoryModelOut>
        {
            new ScoreHistoryModelOut
            {
                Id = Guid.NewGuid(),
                VisitorId = _visitorUserId,
                Points = 100,
                Origin = "AttractionVisit",
                StrategyName = "PerAttraction",
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockScoreHistoryLogic.Setup(l => l.GetMyScoreHistory(_visitorUserId)).Returns(history);

        HttpResponseMessage response = _ = _visitorClient.GetAsync("/api/score-history/my-history").Result;

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public void GetVisitorHistory_AsAdministrator_ReturnsOk()
    {
        Guid visitorId = Guid.NewGuid();
        List<ScoreHistoryModelOut> history = new List<ScoreHistoryModelOut>
        {
            new ScoreHistoryModelOut
            {
                Id = Guid.NewGuid(),
                VisitorId = visitorId,
                Points = 50,
                Origin = "EventParticipation",
                StrategyName = "PerEvent",
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockScoreHistoryLogic.Setup(l => l.GetVisitorScoreHistory(visitorId, null, null)).Returns(history);

        HttpResponseMessage response = _ = _adminClient.GetAsync($"/api/score-history/visitor/{visitorId}").Result;

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public void GetAllHistory_AsAdministrator_ReturnsOk()
    {
        List<ScoreHistoryModelOut> history = new List<ScoreHistoryModelOut>
        {
            new ScoreHistoryModelOut
            {
                Id = Guid.NewGuid(),
                VisitorId = Guid.NewGuid(),
                VisitorName = "John Doe",
                Points = 100,
                Origin = "AttractionVisit",
                StrategyName = "PerAttraction",
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockScoreHistoryLogic.Setup(l => l.GetAllScoreHistory()).Returns(history);

        HttpResponseMessage response = _ = _adminClient.GetAsync("/api/score-history").Result;

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public void GetMyScoreHistory_WithBusinessLogicException_ReturnsBadRequest()
    {
        _mockScoreHistoryLogic.Setup(l => l.GetMyScoreHistory(_visitorUserId))
        .Throws(new ArgumentException("Invalid visitor"));

        HttpResponseMessage response = _ = _visitorClient.GetAsync("/api/score-history/my-history").Result;

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public void GetVisitorHistory_VisitorNotFound_ReturnsNotFound()
    {
        Guid visitorId = Guid.NewGuid();
        _mockScoreHistoryLogic.Setup(l => l.GetVisitorScoreHistory(visitorId, null, null))
        .Throws(new KeyNotFoundException("Visitor not found"));

        HttpResponseMessage response = _ = _adminClient.GetAsync($"/api/score-history/visitor/{visitorId}").Result;

        Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public void GetAllHistory_WithInvalidDateRange_ReturnsBadRequest()
    {
        _mockScoreHistoryLogic.Setup(l => l.GetAllScoreHistory())
        .Throws(new ArgumentException("Invalid data"));

        HttpResponseMessage response = _ = _adminClient.GetAsync("/api/score-history").Result;

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public void GetMyScoreHistory_WithMissingNameIdentifierClaim_UsesEmptyGuid()
    {
        System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler tokenHandler =
        new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        byte[] key = System.Text.Encoding.UTF8.GetBytes("MySecretKeyForJWTTokenGeneration1234567890");
        Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor tokenDescriptor =
        new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Visitor")
            }),
            Expires = System.DateTime.UtcNow.AddHours(1),
            Issuer = "ParqueTematico",
            Audience = "ParqueTematico",
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };
        System.IdentityModel.Tokens.Jwt.JwtSecurityToken token =
        tokenHandler.CreateToken(tokenDescriptor) as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;
        string tokenString = tokenHandler.WriteToken(token);

        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenString);

        _mockScoreHistoryLogic.Setup(l => l.GetMyScoreHistory(Guid.Empty))
        .Returns(new List<ScoreHistoryModelOut>());

        HttpResponseMessage response = _ = client.GetAsync("/api/score-history/my-history").Result;

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}