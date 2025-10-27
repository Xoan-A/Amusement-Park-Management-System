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

        Microsoft.Extensions.Options.IOptions<Models.JwtSettings> jwtSettings = Microsoft.Extensions.Options.Options.Create(new Models.JwtSettings
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
    public async Task GetAttractionIncidents_ValidRequest_ReturnsIncidents()
    {
        List<string> incidents = new List<string> { "Incidente1" };
        _mockService.Setup(s => s.GetAttractionIncidents(_attractionId)).ReturnsAsync(incidents);
        HttpResponseMessage response = await _operatorClient.GetAsync($"/api/incidents/{_attractionId}");
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        List<string>? result = JsonSerializer.Deserialize<List<string>>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Incidente1", result[0]);
    }

    [TestMethod]
    public async Task GetAttractionIncidents_AttractionNotFound_ReturnsNotFound()
    {
        _mockService.Setup(s => s.GetAttractionIncidents(_attractionId)).ThrowsAsync(new KeyNotFoundException());
        HttpResponseMessage response = await _operatorClient.GetAsync($"/api/incidents/{_attractionId}");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task AddIncident_ValidRequest_AddsIncident()
    {
        _mockService.Setup(s => s.AddIncident(_attractionId, "Incidente")).Returns(Task.CompletedTask);
        object incidentRequest = new { incident = "Incidente" };
        StringContent content = new StringContent(JsonSerializer.Serialize(incidentRequest), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _operatorClient.PutAsync($"/api/incidents/{_attractionId}", content);
        response.EnsureSuccessStatusCode();
        string respContent = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(respContent.Contains("Incident reported successfully"));
    }

    [TestMethod]
    public async Task AddIncident_AttractionNotFound_ReturnsNotFound()
    {
        _mockService.Setup(s => s.AddIncident(_attractionId, "Incidente")).ThrowsAsync(new KeyNotFoundException());
        object incidentRequest = new { incident = "Incidente" };
        StringContent content = new StringContent(JsonSerializer.Serialize(incidentRequest), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _operatorClient.PutAsync($"/api/incidents/{_attractionId}", content);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task RemoveIncident_ValidRequest_RemovesIncident()
    {
        _mockService.Setup(s => s.RemoveIncident(_attractionId, "Incidente")).Returns(Task.CompletedTask);
        HttpResponseMessage response = await _operatorClient.DeleteAsync($"/api/incidents/{_attractionId}?incident=Incidente");
        response.EnsureSuccessStatusCode();
        Assert.IsTrue(response.StatusCode == HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task RemoveIncident_AttractionNotFound_ReturnsNotFound()
    {
        _mockService.Setup(s => s.RemoveIncident(_attractionId, "Incidente")).ThrowsAsync(new KeyNotFoundException());
        HttpResponseMessage response = await _operatorClient.DeleteAsync($"/api/incidents/{_attractionId}?incident=Incidente");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task AddIncident_AdminRole_ReturnsForbidden()
    {
        IncidentRequest request = new IncidentRequest { Incident = "Test Incident" };
        string json = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _adminClient.PutAsync($"/api/incidents/{_attractionId}", content);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task RemoveIncident_AdminRole_ReturnsForbidden()
    {
        HttpResponseMessage response = await _adminClient.DeleteAsync($"/api/incidents/{_attractionId}?incident=TestIncident");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }
}