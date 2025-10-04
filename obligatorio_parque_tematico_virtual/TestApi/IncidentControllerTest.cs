using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Text;
using System.Text.Json;
using IBusinessLogic;
using Domain;
using System.Net;
using BusinessLogic;

namespace ApiTests;

[TestClass]
public class IncidentControllerTest
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _operatorClient = null!;
    private Mock<IAttractionService> _mockService = null!;
    private Guid _attractionId;

    [TestInitialize]
    public void Setup()
    {
        _mockService = new Mock<IAttractionService>();
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services => { services.AddSingleton(_mockService.Object); });
        });
        TokenService tokenService = new TokenService();
        Operator operatorUser = new Operator
        {
            Id = Guid.NewGuid(),
            Name = "Operator",
            LastName = "User",
            Email = "operator@example.com"
        };
        string operatorToken = tokenService.GenerateToken(operatorUser);
        _operatorClient = _factory.CreateClient();
        _operatorClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", operatorToken);
        _attractionId = Guid.NewGuid();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _operatorClient?.Dispose();
        _factory?.Dispose();
    }

    [TestMethod]
    public async Task GetAttractionIncidents_ValidRequest_ReturnsIncidents()
    {
        var incidents = new List<string> { "Incidente1" };
        _mockService.Setup(s => s.GetAttractionIncidents(_attractionId)).ReturnsAsync(incidents);
        var response = await _operatorClient.GetAsync($"/api/incidents/{_attractionId}");
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        List<string> result = JsonSerializer.Deserialize<List<string>>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Incidente1", result[0]);
    }

    [TestMethod]
    public async Task GetAttractionIncidents_AttractionNotFound_ReturnsNotFound()
    {
        _mockService.Setup(s => s.GetAttractionIncidents(_attractionId)).ThrowsAsync(new KeyNotFoundException());
        var response = await _operatorClient.GetAsync($"/api/incidents/{_attractionId}");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task AddIncident_ValidRequest_AddsIncident()
    {
        _mockService.Setup(s => s.AddIncident(_attractionId, "Incidente")).Returns(Task.CompletedTask);
        var incidentRequest = new { incident = "Incidente" };
        var content = new StringContent(JsonSerializer.Serialize(incidentRequest), Encoding.UTF8, "application/json");
        var response = await _operatorClient.PostAsync($"/api/incidents/{_attractionId}", content);
        response.EnsureSuccessStatusCode();
        string respContent = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(respContent.Contains("Incident reported successfully"));
    }

    [TestMethod]
    public async Task AddIncident_AttractionNotFound_ReturnsNotFound()
    {
        _mockService.Setup(s => s.AddIncident(_attractionId, "Incidente")).ThrowsAsync(new KeyNotFoundException());
        var incidentRequest = new { incident = "Incidente" };
        var content = new StringContent(JsonSerializer.Serialize(incidentRequest), Encoding.UTF8, "application/json");
        var response = await _operatorClient.PostAsync($"/api/incidents/{_attractionId}", content);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task RemoveIncident_ValidRequest_RemovesIncident()
    {
        _mockService.Setup(s => s.RemoveIncident(_attractionId, "Incidente")).Returns(Task.CompletedTask);
        var incidentRequest = new { incident = "Incidente" };
        var content = new StringContent(JsonSerializer.Serialize(incidentRequest), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/incidents/{_attractionId}")
        { Content = content };
        var response = await _operatorClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        string respContent = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(respContent.Contains("Incident resolved successfully"));
    }

    [TestMethod]
    public async Task RemoveIncident_AttractionNotFound_ReturnsNotFound()
    {
        _mockService.Setup(s => s.RemoveIncident(_attractionId, "Incidente")).ThrowsAsync(new KeyNotFoundException());
        var incidentRequest = new { incident = "Incidente" };
        var content = new StringContent(JsonSerializer.Serialize(incidentRequest), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/incidents/{_attractionId}")
        { Content = content };
        var response = await _operatorClient.SendAsync(request);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}