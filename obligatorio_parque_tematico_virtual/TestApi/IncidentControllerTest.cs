using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Text;
using System.Text.Json;
using IBusinessLogic;
using Domain;
using System.Net;
using BusinessLogic;
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using Microsoft.Data.Sqlite;
using Models.In;

namespace ApiTests;

[TestClass]
public class IncidentControllerTest
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _operatorClient = null!;
    private HttpClient _adminClient = null!;
    private Mock<IAttractionLogic> _mockService = null!;
    private Guid _attractionId;
    private SqliteConnection _connection = null!;

    [TestInitialize]
    public void Setup()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _mockService = new Mock<IAttractionLogic>();
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ServiceDescriptor? descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));

                services.AddSingleton(_mockService.Object);
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
        TokenLogic tokenLogic = new TokenLogic(jwtSettings);
        Models.Out.UserResponse operatorUser = new Models.Out.UserResponse
        {
            Id = Guid.NewGuid(),
            Name = "Operator",
            LastName = "User",
            Email = "operator@example.com",
            UserRoles = new List<string> { Role.OPERATOR }
        };
        string operatorToken = tokenLogic.GenerateToken(operatorUser);
        _operatorClient = _factory.CreateClient();
        _operatorClient.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", operatorToken);

        Models.Out.UserResponse adminUser = new Models.Out.UserResponse
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            LastName = "User",
            Email = "admin@example.com",
            UserRoles = new List<string> { Role.ADMINISTRATOR }
        };
        string adminToken = tokenLogic.GenerateToken(adminUser);
        _adminClient = _factory.CreateClient();
        _adminClient.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        _attractionId = Guid.NewGuid();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _operatorClient?.Dispose();
        _adminClient?.Dispose();
        _factory?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }

    [TestMethod]
    public void AddIncident_ValidRequest_AddsIncident()
    {
        _mockService.Setup(s => s.AddIncident(_attractionId, "Incidente"));
        object incidentRequest = new { incident = "Incidente" };
        StringContent content =
        new StringContent(JsonSerializer.Serialize(incidentRequest), Encoding.UTF8, "application/json");
        HttpResponseMessage response = _ = _operatorClient.PutAsync($"/api/incidents/{_attractionId}", content).Result;
        response.EnsureSuccessStatusCode();
        string respContent = response.Content.ReadAsStringAsync().Result;
        Assert.IsTrue(respContent.Contains("Incident reported successfully"));
    }

    [TestMethod]
    public void AddIncident_AttractionNotFound_ReturnsNotFound()
    {
        _mockService.Setup(s => s.AddIncident(_attractionId, "Incidente")).Throws(new KeyNotFoundException());
        object incidentRequest = new { incident = "Incidente" };
        StringContent content =
        new StringContent(JsonSerializer.Serialize(incidentRequest), Encoding.UTF8, "application/json");
        HttpResponseMessage response = _ = _operatorClient.PutAsync($"/api/incidents/{_attractionId}", content).Result;
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public void RemoveIncident_ValidRequest_RemovesIncident()
    {
        _mockService.Setup(s => s.RemoveIncident(_attractionId, "Incidente"));
        HttpResponseMessage response = _ = _operatorClient.DeleteAsync($"/api/incidents/{_attractionId}?incident=Incidente").Result;
        response.EnsureSuccessStatusCode();
        Assert.IsTrue(response.StatusCode == HttpStatusCode.NoContent);
    }

    [TestMethod]
    public void RemoveIncident_AttractionNotFound_ReturnsNotFound()
    {
        _mockService.Setup(s => s.RemoveIncident(_attractionId, "Incidente")).Throws(new KeyNotFoundException());
        HttpResponseMessage response = _ = _operatorClient.DeleteAsync($"/api/incidents/{_attractionId}?incident=Incidente").Result;
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public void AddIncident_AdminRole_ReturnsForbidden()
    {
        IncidentRequest request = new IncidentRequest { Incident = "Test Incident" };
        string json = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = _ = _adminClient.PutAsync($"/api/incidents/{_attractionId}", content).Result;

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public void RemoveIncident_AdminRole_ReturnsForbidden()
    {
        HttpResponseMessage response = _ = _adminClient.DeleteAsync($"/api/incidents/{_attractionId}?incident=TestIncident").Result;

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }
}