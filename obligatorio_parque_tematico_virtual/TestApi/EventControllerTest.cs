using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Domain;
using IBusinessLogic;
using Models.In;
using Models.Out;
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using Microsoft.Data.Sqlite;

namespace ApiTests;

[TestClass]
public class EventControllerTest
{
    private SqliteConnection _connection = null!;
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private HttpClient _adminClient = null!;
    private Mock<IEventService> _mockEventService = null!;

    [TestInitialize]
    public void Setup()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _mockEventService = new Mock<IEventService>();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite(_connection));

                services.AddSingleton(_mockEventService.Object);
            });
        });

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();
        }

        _client = _factory.CreateClient();

        var jwtSettings = Microsoft.Extensions.Options.Options.Create(new Models.JwtSettings
        {
            SecretKey = "MySecretKeyForJWTTokenGeneration1234567890",
            Issuer = "ParqueTematico",
            Audience = "ParqueTematico",
            ExpirationHours = 1
        });
        var tokenService = new BusinessLogic.TokenService(jwtSettings);

        var adminUser = new Domain.Administrator
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            LastName = "User",
            Email = "admin@example.com"
        };
        string adminToken = tokenService.GenerateToken(adminUser);
        _adminClient = _factory.CreateClient();
        _adminClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client?.Dispose();
        _adminClient?.Dispose();
        _factory?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }

    [TestMethod]
    public async Task GetEvents_ReturnsOkResult_WithListOfEvents()
    {
        var mockEvents = new List<EventResponse>
        {
            new EventResponse
            {
                Id = Guid.NewGuid(),
                Name = "Event 1",
                Date = DateTime.Now.AddDays(10),
                Hour = 2,
                MaxCapacity = 100,
                CurrentCapacity = 50,
                Cost = 100
            },
            new EventResponse
            {
                Id = Guid.NewGuid(),
                Name = "Event 2",
                Date = DateTime.Now.AddDays(20),
                Hour = 3,
                MaxCapacity = 200,
                CurrentCapacity = 150,
                Cost = 200
            }
        };

        _mockEventService.Setup(service => service.GetAllEvents())
            .ReturnsAsync(mockEvents);

        var response = await _adminClient.GetAsync("/api/events");

        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var eventsResponse = JsonSerializer.Deserialize<List<EventResponse>>(responseString, options);

        Assert.IsNotNull(eventsResponse);
        Assert.AreEqual(2, eventsResponse.Count);
        Assert.AreEqual("Event 1", eventsResponse[0].Name);
        Assert.AreEqual("Event 2", eventsResponse[1].Name);
    }

    [TestMethod]
    public async Task GetEvents_InvalidAuthentication_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/events");

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetEventById_ValidId_ReturnsOkResult_WithEvent()
    {
        Guid eventId = Guid.NewGuid();
        EventResponse mockEvent = new EventResponse
        {
            Id = eventId,
            Name = "Event 1",
            Date = DateTime.Now.AddDays(10),
            Hour = 2,
            MaxCapacity = 100,
            CurrentCapacity = 50,
            Cost = 100
        };

        _mockEventService.Setup(service => service.GetEventById(eventId))
            .ReturnsAsync(mockEvent);

        var response = await _adminClient.GetAsync($"/api/events/{eventId}");

        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var eventResponse = JsonSerializer.Deserialize<EventResponse>(responseString, options);

        Assert.IsNotNull(eventResponse);
        Assert.AreEqual("Event 1", eventResponse.Name);
    }

    [TestMethod]
    public async Task GetEventById_InvalidAuthentication_ReturnsUnauthorized()
    {
        Guid eventId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/events/{eventId}");

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task CreateEvent_ValidRequest_ReturnsCreatedResult()
    {
        Guid newEventId = Guid.NewGuid();
        var newEvent = new EventRequest
        {
            Name = "New Event",
            Date = DateTime.Now.AddDays(30),
            Hour = 4,
            MaxCapacity = 150,
            Cost = 150,
            AttractionIds = new List<Guid>()
        };

        _mockEventService.Setup(service => service.CreateEvent(It.IsAny<EventRequest>()))
            .ReturnsAsync(newEventId);

        var content = new StringContent(JsonSerializer.Serialize(newEvent), Encoding.UTF8, "application/json");
        var response = await _adminClient.PostAsync("/api/events", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Created, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var createResponse = JsonSerializer.Deserialize<CreateEventResponse>(responseString, options);

        Assert.IsNotNull(createResponse);
        Assert.AreEqual(newEventId, createResponse.Id);
        Assert.AreEqual("Event created successfully", createResponse.Message);
    }

    [TestMethod]
    public async Task CreateEvent_InvalidAuthentication_ReturnsUnauthorized()
    {
        Guid newEventId = Guid.NewGuid();
        var newEvent = new EventRequest
        {
            Name = "New Event",
            Date = DateTime.Now.AddDays(30),
            Hour = 4,
            MaxCapacity = 150,
            Cost = 150,
            AttractionIds = new List<Guid>()
        };
        var content = new StringContent(JsonSerializer.Serialize(newEvent), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/events", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _mockEventService.Verify(service => service.CreateEvent(It.IsAny<EventRequest>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteEvent_ValidId_ReturnsNoContent()
    {
        Guid eventId = Guid.NewGuid();

        _mockEventService.Setup(service => service.DeleteEvent(eventId))
            .Returns(Task.CompletedTask);

        var response = await _adminClient.DeleteAsync($"/api/events/{eventId}");

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        _mockEventService.Verify(service => service.DeleteEvent(eventId), Times.Once);
    }

    [TestMethod]
    public async Task DeleteEvent_InvalidAuthentication_ReturnsUnauthorized()
    {
        Guid eventId = Guid.NewGuid();

        var response = await _client.DeleteAsync($"/api/events/{eventId}");

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        _mockEventService.Verify(service => service.DeleteEvent(eventId), Times.Never);
    }
}